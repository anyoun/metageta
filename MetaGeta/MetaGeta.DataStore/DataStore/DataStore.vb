<Serializable()> _
Public Class MGDataStore
    Implements INotifyPropertyChanged

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_Owner As IDataStoreOwner
    Private ReadOnly m_DataMapper As DataMapper
    Private m_ID As Long = -1
    Private m_Name As String
    Private m_Template As IDataStoreTemplate
    Private m_Description As String = ""

    Private ReadOnly m_AllPlugins As New List(Of IMGPluginBase)
    Private ReadOnly m_FileActionDictionary As New Dictionary(Of String, IMGFileActionPlugin)

    Private ReadOnly m_ShutdownEvent As New ManualResetEvent(False)

    Friend Sub New(ByVal owner As IDataStoreOwner, ByVal dataMapper As DataMapper)
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

    Friend Function GetTag(ByVal file As MGFile, ByVal tagName As String, Optional ByVal tran As DbTransaction = Nothing) As String
        Return m_DataMapper.GetTag(file, tagName)
    End Function
    Friend Sub SetTag(ByVal file As MGFile, ByVal tagName As String, ByVal tagValue As String, Optional ByVal tran As DbTransaction = Nothing)
        m_DataMapper.WriteTag(file, tagName, tagValue)
    End Sub

    Friend Function GetAllTags(ByVal fileId As Long) As MGTagCollection
        Return m_DataMapper.GetAllTags(fileId)
    End Function

    Public Function GetAllTagOnFiles(ByVal tagName As String) As IList(Of Tuple(Of MGTag, MGFile))
        Return m_DataMapper.GetAllTagOnFiles(Me, tagName)
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
                m_DataMapper.WriteDataStore(Me)
                OnNameChanged()
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
                m_DataMapper.WriteDataStore(Me)
                OnDescriptionChanged()
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
                m_DataMapper.WriteDataStore(Me)
                OnTemplateChanged()
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

    Public Function GetCreationArguments() As DataStoreCreationArguments
        Return New DataStoreCreationArguments() With {.Name = Name, _
                                                      .Description = Description, _
                                                      .Tempate = Template, _
                                                      .DirectoriesToWatch = GetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "DirectoriesToWatch"), _
                                                      .Extensions = GetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "Extensions") _
                                                      }
    End Function

    Public Sub SetCreationArguments(ByVal args As DataStoreCreationArguments)
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

Public Class DataStoreCreationArguments
    Implements INotifyPropertyChanged, IDataErrorInfo

    Private m_Name As String, m_Description As String
    Private m_Template As IDataStoreTemplate
    Private m_DirectoriesToWatch As String, m_Extensions As String

    Public Sub New()
        m_Name = "TV Shows"
        m_DirectoriesToWatch = "F:\ipod\"
        m_Extensions = "mp4;avi;mkv"
    End Sub

    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            If m_Name <> value Then
                m_Name = value
                OnPropertyChanged("Name")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(ByVal value As String)
            If m_Description <> value Then
                m_Description = value
                OnPropertyChanged("Description")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Tempate() As IDataStoreTemplate
        Get
            Return m_Template
        End Get
        Set(ByVal value As IDataStoreTemplate)
            If m_Template IsNot value Then
                m_Template = value
                OnPropertyChanged("Template")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property DirectoriesToWatch() As String
        Get
            Return m_DirectoriesToWatch
        End Get
        Set(ByVal value As String)
            If m_DirectoriesToWatch <> value Then
                m_DirectoriesToWatch = value
                OnPropertyChanged("DirectoriesToWatch")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property
    Public Property Extensions() As String
        Get
            Return m_Extensions
        End Get
        Set(ByVal value As String)
            If m_Extensions <> value Then
                m_Extensions = value
                OnPropertyChanged("Extensions")
                OnPropertyChanged("IsValid")
            End If
        End Set
    End Property

    Private ReadOnly Property DirectoriesArray() As String()
        Get
            Return DirectoriesToWatch.Split(";"c)
        End Get
    End Property

    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Public ReadOnly Property [Error]() As String Implements IDataErrorInfo.Error
        Get
            Return Nothing
        End Get
    End Property

    Default Public ReadOnly Property Item(ByVal propertyName As String) As String Implements IDataErrorInfo.Item
        Get
            Select Case propertyName
                Case "Name"
                    Return If(String.IsNullOrEmpty(Name), "Please enter a Name.", Nothing)
                Case "DirectoriesToWatch"
                    If DirectoriesArray.Length < 1 Then
                        Return "Please select at least one directory."
                    End If
                    Dim invalidDir = DirectoriesArray.FirstOrDefault(Function(s) Not Directory.Exists(s))
                    Return If(invalidDir Is Nothing, Nothing, String.Format("Can't find directory ""{0}"".", invalidDir))
                Case "Extensions"
                    If Extensions.Length < 1 Then
                        Return "Please select at least one extension."
                    Else
                        Return Nothing
                    End If
                Case Else
                    Return Nothing
            End Select
        End Get
    End Property

    Public ReadOnly Property IsValid() As Boolean
        Get
            Return Item("Name") Is Nothing AndAlso Item("DirectoriesToWatch") Is Nothing AndAlso Item("Extensions") Is Nothing
        End Get
    End Property
End Class
