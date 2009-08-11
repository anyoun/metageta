<Serializable()> _
Public Class MGDataStore
    Implements INotifyPropertyChanged

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_Owner As IDataStoreOwner
    Private ReadOnly m_DataMapper As IDataMapper
    Private m_ID As Long = -1
    Private m_Name As String
    Private m_Template As IDataStoreTemplate
    Private m_Description As String = ""

    Private ReadOnly m_AllPlugins As New List(Of IMGPluginBase)
    Private ReadOnly m_FileActionDictionary As New Dictionary(Of String, IMGFileActionPlugin)

    Private ReadOnly m_ShutdownEvent As New ManualResetEvent(False)

    Friend Sub New(ByVal owner As IDataStoreOwner, ByVal dataMapper As IDataMapper)
        m_Owner = owner
        m_DataMapper = dataMapper

        m_ImportThread.IsBackground = True
        m_ImportThread.Start()
    End Sub

    Public Sub Close()
        m_ShutdownEvent.Set()
        m_ImportThread.Join()
        For Each plugin In m_AllPlugins
            plugin.Shutdown()
        Next
    End Sub

    Public Sub Delete()
        m_Owner.DeleteDataStore(Me)
    End Sub

    Public Function GetAllTagOnFiles(ByVal tagName As String) As IList(Of Tuple(Of MGTag, MGFile))
        Return m_DataMapper.GetTagOnAllFiles(Me, tagName)
    End Function

    Public ReadOnly Property Files() As IList(Of MGFile)
        Get
            Return m_DataMapper.GetFiles(Me)
        End Get
    End Property

    Public Sub RemoveFiles(ByVal files As IEnumerable(Of MGFile))
        m_DataMapper.RemoveFiles(files, Me)
        OnFilesChanged()
    End Sub

#Region "Plugins"

    Friend Sub AddExistingPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long)
        m_AllPlugins.Add(plugin)
        StartupPlugin(plugin, id, False)
    End Sub

    Friend Sub AddNewPlugin(ByVal pluginTypeName As String)
        Dim t = Type.GetType(pluginTypeName)
        If t Is Nothing Then
            Throw New Exception(String.Format("Can't find type ""{0}"".", pluginTypeName))
        End If
        Dim plugin = CType(Activator.CreateInstance(t), IMGPluginBase)
        m_AllPlugins.Add(plugin)
        'Datamapper will call StartupPlugin() after IDs have been assigned
    End Sub

    Friend Sub StartupPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long, ByVal firstRun As Boolean)
        If Not Plugins.Contains(plugin) Then
            Throw New Exception("Can't find plugin in StartupPlugin().")
        End If

        log.InfoFormat("StartupPlugin(): {0}", plugin.GetType().FullName)

        plugin.Startup(Me, id)

        Dim settings = SettingInfoCollection.GetSettings(plugin)
        For Each setting In settings
            If firstRun Then
                setting.Value = setting.Metadata.DefaultValue
                SetPluginSetting(plugin, setting.SettingName, setting.GetDefaultValueAsString())
            Else
                setting.SetValueAsString(GetPluginSetting(plugin, setting.SettingName))
            End If
        Next
        AddHandler plugin.SettingChanged, AddressOf Plugin_SettingChanged

        If TypeOf plugin Is IMGFileActionPlugin Then
            SetUpAction(CType(plugin, IMGFileActionPlugin))
        End If
    End Sub

    Private Sub SetUpAction(ByVal ap As IMGFileActionPlugin)
        For Each s In ap.GetActions()
            m_FileActionDictionary.Add(s, ap)
        Next
    End Sub

    Public ReadOnly Property TaggingPlugins() As IEnumerable(Of IMGTaggingPlugin)
        Get
            Return m_AllPlugins.Where(Function(p) TypeOf p Is IMGTaggingPlugin).Cast(Of IMGTaggingPlugin)()
        End Get
    End Property
    Public ReadOnly Property FileSourcePlugins() As IEnumerable(Of IMGFileSourcePlugin)
        Get
            Return m_AllPlugins.Where(Function(p) TypeOf p Is IMGFileSourcePlugin).Cast(Of IMGFileSourcePlugin)()
        End Get
    End Property
    Public ReadOnly Property FileActionPlugins() As IEnumerable(Of IMGFileActionPlugin)
        Get
            Return m_AllPlugins.Where(Function(p) TypeOf p Is IMGFileActionPlugin).Cast(Of IMGFileActionPlugin)()
        End Get
    End Property

    Public ReadOnly Property Plugins() As IEnumerable(Of IMGPluginBase)
        Get
            Return m_AllPlugins
        End Get
    End Property

#End Region

#Region "Importing"

    Private ReadOnly m_ImportThread As New Thread(AddressOf ImportThread)
    Private ReadOnly m_ImportNow As New AutoResetEvent(False)

    Private m_LastImportStatus As New ImportStatus()
    Private m_LastImportStatusLock As New Object()

    ''' <summary>
    ''' Starts an asynchronous refresh and import from all FileSourcePlugins.
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub BeginRefresh()
        m_ImportNow.Set()
    End Sub

    Public ReadOnly Property ImportStatus() As ImportStatus
        Get
            SyncLock m_LastImportStatusLock
                Return m_LastImportStatus
            End SyncLock
        End Get
    End Property

    Private Sub SetImportStatus(ByVal message As String, Optional ByVal progressPct As Integer = -1)
        SyncLock m_LastImportStatusLock
            If progressPct = -1 Then
                m_LastImportStatus = New ImportStatus(message)
            Else
                m_LastImportStatus = New ImportStatus(message, progressPct)
            End If
        End SyncLock
        OnPropertyChanged("ImportStatus")
    End Sub

    Public Sub ImportThread()
        While True
            Dim wakeupHandle = WaitHandle.WaitAny(New WaitHandle() {m_ImportNow, m_ShutdownEvent})
            If wakeupHandle = 1 Then Return

            SetImportStatus("Listing files...")

            'Get list of files
            Dim newPaths = (From fs In Me.FileSourcePlugins _
                      From path In fs.GetFilesToAdd() _
                      Select path).ToArray()
            Dim files = (From p In newPaths Select New MGFile(Me)).ToArray()
            m_DataMapper.WriteNewFiles(files, Me)

            For Each fileAndPath In files.IndexInnerJoin(newPaths)
                Dim newfile = fileAndPath.First
                Dim filePath = fileAndPath.Second
                Dim index = fileAndPath.Third
                SetImportStatus(String.Format("Importing ""{0}""", filePath.LocalPath), CInt(index / files.Length * 100))
                ImportNewFile(newfile, filePath)
            Next

            SetImportStatus("Import complete.", 100)

            OnFilesChanged()
        End While
    End Sub

    Public Function AddNewFile(ByVal path As Uri) As MGFile
        Dim file As New MGFile(Me)
        m_DataMapper.WriteNewFiles(file.SingleToEnumerable(), Me)
        ImportNewFile(file, path)
        OnFilesChanged()
        Return file
    End Function

    Private Sub ImportNewFile(ByVal newfile As MGFile, ByVal filePath As Uri)
        newfile.SetTag(MGFile.FileNameKey, filePath.AbsoluteUri)
        newfile.SetTag("ImportComplete", False.ToString())

        log.DebugFormat("Importing file: {0}....", filePath)

        For Each plugin As IMGTaggingPlugin In Me.TaggingPlugins
            log.DebugFormat("Importing {0} with {1}....", newfile.FileName, plugin.GetType().FullName)
            Try
                plugin.Process(newfile, New ProgressStatus())
            Catch ex As Exception
                log.Warn(String.Format("Importing ""{0}"" with {1} failed.", filePath, plugin.GetType().FullName), ex)
            End Try
            m_DataMapper.WriteFile(newfile)
        Next
    End Sub

    Private m_ImportQueueCache As ReadOnlyCollection(Of MGFile)
    Private ReadOnly Property ImportQueue() As IEnumerable(Of MGFile)
        Get
            If m_ImportQueueCache Is Nothing OrElse m_ImportQueueCache.Count = 0 Then
                m_ImportQueueCache = New ReadOnlyCollection(Of MGFile)(Files.Where(Function(f) Not Boolean.Parse(f.GetTag("ImportComplete").Coalesce("False"))).ToArray())
            End If
            Return m_ImportQueueCache
        End Get
    End Property

#End Region

#Region "Settings"
    Public Sub Plugin_SettingChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        Dim plugin = CType(sender, IMGPluginBase)
        Dim value = SettingInfoCollection.GetSettings(plugin).GetSetting(e.PropertyName).GetValueAsString()
        SetPluginSetting(plugin, e.PropertyName, value)
    End Sub

    Public Function GetPluginSetting(ByVal plugin As IMGPluginBase, ByVal settingName As String) As String
        Return m_DataMapper.GetPluginSetting(Me, plugin.PluginID, settingName)
    End Function
    Public Sub SetPluginSetting(ByVal plugin As IMGPluginBase, ByVal settingName As String, ByVal settingValue As String)
        m_DataMapper.WritePluginSetting(Me, plugin.PluginID, settingName, settingValue)
    End Sub
#End Region

    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            If value <> m_Name Then
                m_Name = value
                If Not AreUpdatesSuspended Then
                    m_DataMapper.WriteDataStore(Me)
                    OnNameChanged()
                End If
            End If
        End Set
    End Property
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(ByVal value As String)
            If value <> m_Description Then
                m_Description = value
                If Not AreUpdatesSuspended Then
                    m_DataMapper.WriteDataStore(Me)
                    OnDescriptionChanged()
                End If
            End If
        End Set
    End Property
    Public Property Template() As IDataStoreTemplate
        Get
            Return m_Template
        End Get
        Set(ByVal value As IDataStoreTemplate)
            If value IsNot m_Template Then
                m_Template = value
                If Not AreUpdatesSuspended Then
                    m_DataMapper.WriteDataStore(Me)
                    OnTemplateChanged()
                End If
            End If
        End Set
    End Property
    Public Property ID() As Long
        Get
            Return m_ID
        End Get
        Set(ByVal value As Long)
            If value <> m_ID Then
                m_ID = value
            End If
        End Set
    End Property

#Region "Suspending Updates"

    Private m_SuspendCount As Integer = 0

    Private ReadOnly Property AreUpdatesSuspended() As Boolean
        Get
            Return m_SuspendCount <> 0
        End Get
    End Property

    Public Function SuspendUpdates() As IDisposable
        m_SuspendCount += 1
        Return New SuspendUpdatesToken(Me)
    End Function

    Private Class SuspendUpdatesToken
        Implements IDisposable

        Private ReadOnly m_DataStore As MGDataStore

        Public Sub New(ByVal dataStore As MGDataStore)
            m_DataStore = dataStore
        End Sub

        Private disposedValue As Boolean = False        ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    m_DataStore.m_SuspendCount -= 1
                End If

                ' TODO: free your own state (unmanaged objects).
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

#Region " IDisposable Support "
        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

#End Region

    Public Overrides Function ToString() As String
        Return "MGDataStore: " & Name
    End Function

    Public Function ToCsv() As String
        Dim sb As New StringBuilder()
        sb.AppendLine(Aggregate s In Template.GetDimensionNames() Order By s Into JoinToCsv(s))
        For Each f In Me.Files
            sb.AppendLine(Aggregate t In f.Tags Order By t.Name Into JoinToCsv(t.Value))
        Next
        Return sb.ToString()
    End Function

#Region "Action Plugins"
    Public Sub DoAction(ByVal file As MGFile, ByVal action As String)
        m_Owner.EnqueueAction(action, Me, file)
    End Sub

    Friend Function LookupAction(ByVal action As String) As IMGFileActionPlugin
        Return m_FileActionDictionary(action)
    End Function
#End Region

#Region "Property Changed"

    Private Sub OnNameChanged()
        OnPropertyChanged("Name")
    End Sub
    Private Sub OnDescriptionChanged()
        OnPropertyChanged("Description")
    End Sub
    Private Sub OnTemplateChanged()
        OnPropertyChanged("Template")
    End Sub
    Private Sub OnFilesChanged()
        OnPropertyChanged("Files")
    End Sub

    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
#End Region

#Region "DataStoreCreationArguments"

    Public Function GetCreationArguments() As DataStoreBuilder
        Return New DataStoreBuilder() With {.Name = Name, _
                                                      .Description = Description, _
                                                      .Tempate = Template, _
                                                      .DirectoriesToWatch = GetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "DirectoriesToWatch"), _
                                                      .Extensions = GetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "Extensions") _
                                                      }
    End Function

    Public Sub SetCreationArguments(ByVal args As DataStoreBuilder)
        Name = args.Name
        Description = args.Description
        SetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "DirectoriesToWatch", args.DirectoriesToWatch)
        SetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "Extensions", args.Extensions)
    End Sub

#End Region

#Region "Images"
    Public Function GetImageDirectory() As String
        Dim imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images")
        If Not Directory.Exists(imagePath) Then
            Directory.CreateDirectory(imagePath)
        End If
        Return imagePath
    End Function
#End Region
End Class

Public Class ImportStatus
    Private ReadOnly m_StatusMessage As String
    Private ReadOnly m_ProgressPct As Integer
    Private ReadOnly m_IsImporting As Boolean
    Private ReadOnly m_IsIndeterminate As Boolean

    Public Sub New(ByVal statusMessage As String, ByVal progressPct As Integer)
        m_StatusMessage = statusMessage
        m_ProgressPct = progressPct
        m_IsImporting = True
        m_IsIndeterminate = False
    End Sub

    Public Sub New()
        Me.New(String.Empty, 0)
        m_IsImporting = False
    End Sub

    Public Sub New(ByVal statusMessage As String)
        Me.New(statusMessage, 0)
        m_IsIndeterminate = True
    End Sub

    Public ReadOnly Property StatusMessage() As String
        Get
            Return m_StatusMessage
        End Get
    End Property

    Public ReadOnly Property ProgressPct() As Integer
        Get
            Return m_ProgressPct
        End Get
    End Property

    Public ReadOnly Property IsImporting() As Boolean
        Get
            Return m_IsImporting
        End Get
    End Property

    Public ReadOnly Property IsIndeterminate() As Boolean
        Get
            Return m_IsIndeterminate
        End Get
    End Property
End Class