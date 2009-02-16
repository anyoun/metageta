<Serializable()> _
Public Class MGDataStore
    Implements INotifyPropertyChanged

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_Template As IDataStoreTemplate
    Private ReadOnly m_DataMapper As DataMapper
    Private m_ID As Long = -1
    Private m_Name As String
    Private m_Description As String = ""

    Private ReadOnly m_TaggingPlugins As New List(Of IMGTaggingPlugin)
    Private ReadOnly m_FileSourcePlugins As New List(Of IMGFileSourcePlugin)

    Friend Sub New(ByVal template As IDataStoreTemplate, ByVal name As String, ByVal plugins As IEnumerable(Of IMGPluginBase), ByVal dataMapper As DataMapper)
        m_Template = template
        m_Name = name
        m_DataMapper = dataMapper
        For Each plugin In plugins
            AddPlugin(plugin)
        Next
    End Sub

    Public Sub Close()
        For Each plugin As IMGTaggingPlugin In m_TaggingPlugins
            plugin.Shutdown()
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

    Friend Sub AddPlugin(ByVal plugin As IMGPluginBase)
        AddPlugin(plugin, -1, callStartup:=False)
    End Sub

    Friend Sub AddPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long)
        AddPlugin(plugin, id, callStartup:=True)
    End Sub

    Private Sub AddPlugin(ByVal plugin As IMGPluginBase, ByVal id As Long, ByVal callStartup As Boolean)
        log.InfoFormat("Adding plugin: {0}", plugin.GetUniqueName())
        If TypeOf plugin Is IMGTaggingPlugin Then
            m_TaggingPlugins.Add(CType(plugin, IMGTaggingPlugin))
        ElseIf TypeOf plugin Is IMGFileSourcePlugin Then
            m_FileSourcePlugins.Add(CType(plugin, IMGFileSourcePlugin))
        End If
        If callStartup Then plugin.Startup(Me, id)
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

    Public ReadOnly Property Plugins() As IEnumerable(Of IMGPluginBase)
        Get
            Return TaggingPlugins.Cast(Of IMGPluginBase)().Union(FileSourcePlugins.Cast(Of IMGPluginBase)())
        End Get
    End Property

#End Region

    Public Sub RefreshFileSources()
        Dim files = (From fs In Me.FileSourcePlugins From file In fs.GetFilesToAdd() Select CreateFile(file)).ToList()

        For Each plugin As IMGTaggingPlugin In Me.TaggingPlugins
            Dim fp As New FileProgress(plugin.GetFriendlyName())
            plugin.Process(files, fp)
        Next

        OnFilesChanged()
    End Sub

    Public Property Name() As String
        Get
            Return m_Name
        End Get
        Set(ByVal value As String)
            m_Name = value
        End Set
    End Property
    Public Property Description() As String
        Get
            Return m_Description
        End Get
        Set(ByVal value As String)
            m_Description = value
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
            m_ID = value
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
            sb.AppendLine(Aggregate t In f.GetTags() Order By t.Name Into JoinToCsv(t.Value))
        Next
        Return sb.ToString()
    End Function

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
    Private Sub OnPropertyChanged(ByVal propertyName As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(propertyName))
    End Sub

    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
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
