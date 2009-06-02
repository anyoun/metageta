<Serializable()> _
Public Class MGDataStore
    Implements INotifyPropertyChanged

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_Template As IDataStoreTemplate
    Private ReadOnly m_DataMapper As DataMapper
    Private m_ID As Long = -1
    Private m_Name As String
    Private m_Description As String = ""

    Private ReadOnly m_AllPlugins As New List(Of IMGPluginBase)
    Private ReadOnly m_TaggingPlugins As New List(Of IMGTaggingPlugin)
    Private ReadOnly m_FileSourcePlugins As New List(Of IMGFileSourcePlugin)

    Private ReadOnly m_FileActionPlugins As New List(Of IMGFileActionPlugin)
    Private ReadOnly m_FileActionDictionary As New Dictionary(Of String, IMGFileActionPlugin)

    Private ReadOnly m_ProcessActionThread As Thread
    Private ReadOnly m_StopProcessingActions As New ManualResetEvent(False)


    Friend Sub New(ByVal template As IDataStoreTemplate, ByVal name As String, ByVal plugins As IEnumerable(Of IMGPluginBase), ByVal dataMapper As DataMapper)
        m_Template = template
        m_Name = name
        m_DataMapper = dataMapper
        For Each plugin In plugins
            AddPlugin(plugin)
        Next

        Dim ifa = New ImportFileAction(Me)
        m_FileActionPlugins.Add(ifa)
        SetUpAction(ifa)

        Dim rfsa = New RefreshFileSourcesAction(Me)
        m_FileActionPlugins.Add(rfsa)
        SetUpAction(rfsa)

        m_ProcessActionThread = New Thread(AddressOf ProcessActionQueueThread)
        m_ProcessActionThread.Start()
    End Sub

    Public Sub Close()
        CloseActionQueue()

        For Each plugin In m_AllPlugins
            plugin.Shutdown()
        Next
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

#Region "Plugins"

    Friend Sub AddPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long)
        AddPlugin(plugin)
        StartupPlugin(plugin, id)
    End Sub
    Friend Sub AddPlugin(ByVal plugin As IMGPluginBase)
        log.InfoFormat("Adding plugin: {0}", plugin.GetUniqueName())
        m_AllPlugins.Add(plugin)
        If TypeOf plugin Is IMGTaggingPlugin Then
            m_TaggingPlugins.Add(CType(plugin, IMGTaggingPlugin))
        ElseIf TypeOf plugin Is IMGFileSourcePlugin Then
            m_FileSourcePlugins.Add(CType(plugin, IMGFileSourcePlugin))
        ElseIf TypeOf plugin Is IMGFileActionPlugin Then
            m_FileActionPlugins.Add(CType(plugin, IMGFileActionPlugin))
        Else
            Throw New Exception("Unknown plugin type: " + plugin.GetType().FullName)
        End If
    End Sub
    Friend Sub StartupPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long)
        plugin.Startup(Me, id)
        If TypeOf plugin Is IMGFileActionPlugin Then
            SetUpAction(CType(plugin, IMGFileActionPlugin))
        End If
    End Sub

    Private Sub SetUpAction(ByVal plugin As IMGFileActionPlugin)
        For Each s In plugin.GetActions()
            m_FileActionDictionary.Add(s, plugin)
        Next
    End Sub

    Public ReadOnly Property TaggingPlugins() As IEnumerable(Of IMGTaggingPlugin)
        Get
            Return m_TaggingPlugins
        End Get
    End Property
    Public ReadOnly Property FileSourcePlugins() As IEnumerable(Of IMGFileSourcePlugin)
        Get
            Return m_FileSourcePlugins
        End Get
    End Property
    Public ReadOnly Property FileActionPlugins() As IEnumerable(Of IMGFileActionPlugin)
        Get
            Return m_FileActionPlugins
        End Get
    End Property

    Public ReadOnly Property Plugins() As IEnumerable(Of IMGPluginBase)
        Get
            Return m_AllPlugins
        End Get
    End Property

#End Region

#Region "Importing"

    Public Sub EnqueueRefreshFileSources()
        DoAction(Nothing, RefreshFileSourcesAction.c_RefreshFileSourcesAction)
    End Sub

    Friend Sub RefreshFileSources()
        For Each path In From fs In Me.FileSourcePlugins _
                         From file In fs.GetFilesToAdd() _
                         Select file
            Dim file = CreateFile(path)
            EnqueueImportFile(file)
        Next
    End Sub

    Public Function CreateFile(ByVal path As Uri) As MGFile
        log.DebugFormat("CreateFile() ""{0}""", path.AbsolutePath)
        Dim f As New MGFile(Me)
        m_DataMapper.WriteNewFile(f, Me)
        f.SetTag(MGFile.FileNameKey, path.AbsoluteUri)
        RaiseEvent ItemAdded(Me, New MGFileEventArgs(f))
        Return f
    End Function

    Public Sub EnqueueImportFile(ByVal file As MGFile)
        DoAction(file, ImportFileAction.c_ImportAction)
    End Sub

    Friend Sub ImportFile(ByVal file As MGFile, ByVal progress As ProgressStatus)
        For Each plugin As IMGTaggingPlugin In Me.TaggingPlugins
            plugin.Process(file, progress)
        Next
        OnFilesChanged()
    End Sub

#End Region

#Region "Global Settings"
    Public Function GetGlobalSetting(ByVal settingName As String) As String
        Return m_DataMapper.GetGlobalSetting(settingName)
    End Function
    Public Sub SetGlobalSetting(ByVal settingName As String, ByVal settingValue As String)
        m_DataMapper.WriteGlobalSetting(settingName, settingValue)
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
    Public ReadOnly Property Template() As IDataStoreTemplate
        Get
            Return m_Template
        End Get
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


    Public Function GetPluginSetting(ByVal plugin As IMGPluginBase, ByVal settingName As String) As String
        Return m_DataMapper.GetPluginSetting(Me, plugin.PluginID, settingName)
    End Function
    Public Sub SetPluginSetting(ByVal plugin As IMGPluginBase, ByVal settingName As String, ByVal settingValue As String)
        m_DataMapper.WritePluginSetting(Me, plugin.PluginID, settingName, settingValue)
    End Sub

    Public Event ItemAdded As EventHandler(Of MGFileEventArgs)

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

#Region "Action Queue"
    Private ReadOnly m_ActionWaitingQueue As New Queue(Of ActionQueueItem)
    Private ReadOnly m_AllActionItems As New List(Of ActionQueueItem)

    Public ReadOnly Property ActionItems() As IEnumerable(Of ActionQueueItem)
        Get
            SyncLock m_ActionWaitingQueue
                Return m_AllActionItems.ToArray()
            End SyncLock
        End Get
    End Property

    Private Sub EnqueueAction(ByVal action As String, ByVal file As MGFile)
        SyncLock m_ActionWaitingQueue
            Dim item As New ActionQueueItem(action, file)
            m_ActionWaitingQueue.Enqueue(item)
            Monitor.PulseAll(m_ActionWaitingQueue)
            m_AllActionItems.Add(item)
        End SyncLock
        OnActionItemsChanged()
    End Sub

    Private Sub CloseActionQueue()
        log.Info("Stopping action queue...")
        SyncLock m_ActionWaitingQueue
            m_ActionWaitingQueue.Enqueue(Nothing)
            Monitor.PulseAll(m_ActionWaitingQueue)
        End SyncLock
        m_StopProcessingActions.Set()
        log.Info("Joining...")
        m_ProcessActionThread.Join()
        log.Info("Joined")
    End Sub

    Private Sub ProcessActionQueueThread()
        Thread.CurrentThread.Name = "ProcessActionQueue"
        While True
            If m_StopProcessingActions.WaitOne(0, False) Then Return

            Dim nextItem As ActionQueueItem
            SyncLock m_ActionWaitingQueue
                While m_ActionWaitingQueue.Count = 0
                    Monitor.Wait(m_ActionWaitingQueue)
                End While
                nextItem = m_ActionWaitingQueue.Dequeue()
            End SyncLock
            If nextItem Is Nothing Then Return

            Dim action = m_FileActionDictionary(nextItem.Action)
            nextItem.Status.Start()
            Try
                log.DebugFormat("Starting action ""{0}"" for file ""{1}""...", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""))
                action.DoAction(nextItem.Action, nextItem.File, nextItem.Status)
                nextItem.Status.Done(True)
                nextItem.Status.StatusMessage = String.Empty
                log.DebugFormat("Action ""{0}"" for file ""{1}"" done.", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""))
            Catch ex As Exception
                nextItem.Status.Done(False)
                nextItem.Status.StatusMessage = ex.ToString()
                log.ErrorFormat("Action ""{0}"" for file ""{1}"" failed with exception:\n{2}", nextItem.Action, If(nextItem.File IsNot Nothing, nextItem.File.FileName, ""), ex)
            End Try
        End While
    End Sub

#End Region

#Region "Action Plugins"
    Public Sub DoAction(ByVal file As MGFile, ByVal action As String)
        EnqueueAction(action, file)
    End Sub
#End Region

#Region "Property Changed"

    Private Sub OnNameChanged()
        OnPropertyChanged("Name")
    End Sub
    Private Sub OnDescriptionChanged()
        OnPropertyChanged("Description")
    End Sub
    Private Sub OnFilesChanged()
        OnPropertyChanged("Files")
    End Sub

    Private Sub OnActionItemsChanged()
        OnPropertyChanged("ActionItems")
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

    Public Sub SetCreationArguemnts(ByVal args As DataStoreCreationArguments)
        Name = args.Name
        Description = args.Description
        SetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "DirectoriesToWatch", args.DirectoriesToWatch)
        SetPluginSetting(CType(FileSourcePlugins.Single(), IMGPluginBase), "Extensions", args.Extensions)
    End Sub

#End Region

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
