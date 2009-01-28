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

    Friend Sub New(ByVal template As IDataStoreTemplate, ByVal name As String, ByVal dataMapper As DataMapper)
        m_Template = template
        m_Name = name
        m_DataMapper = dataMapper
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

    Public Sub AddTaggingPlugin(ByVal assemblyQualifiedTypeName As String)
        AddTaggingPlugin(Type.GetType(assemblyQualifiedTypeName, True))
    End Sub
    Public Sub AddTaggingPlugin(ByVal type As Type)
        log.DebugFormat("Loading tagging plugin {0}", type)
        Dim plugin = CType(Activator.CreateInstance(type), IMGTaggingPlugin)
        plugin.Startup(Me)
        m_TaggingPlugins.Add(plugin)
    End Sub

    Public Sub AddFileSourcePlugin(ByVal assemblyQualifiedTypeName As String)
        AddTaggingPlugin(Type.GetType(assemblyQualifiedTypeName, True))
    End Sub
    Public Sub AddFileSourcePlugin(ByVal type As Type)
        log.DebugFormat("Loading file source plugin {0}", type)
        Dim plugin = CType(Activator.CreateInstance(type), IMGFileSourcePlugin)
        plugin.Startup(Me)
        m_FileSourcePlugins.Add(plugin)
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
        Return m_DataMapper.GetPluginSetting(Me, plugin.GetUniqueName(), settingName)
    End Function
    Public Sub SetPluginSetting(ByVal plugin As IMGPluginBase, ByVal settingName As String, ByVal settingValue As String)
        m_DataMapper.WritePluginSetting(Me, plugin.GetUniqueName(), settingName, settingValue)
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
