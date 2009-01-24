<Serializable()> _
Public Class MGDataStore
    Implements IEnumerable(Of MGFile)

    'Private m_Items As New List(Of MGFile)

    Private ReadOnly m_Template As IDataStoreTemplate
    Private ReadOnly m_DataMapper As DataMapper
    Private m_ID As Long = -1
    Private m_Name As String
    Private m_Description As String

    <NonSerialized()> _
    Private m_TaggingPlugins As New List(Of IMGTaggingPlugin)

    Friend Sub New(ByVal template As IDataStoreTemplate, ByVal name As String, ByVal dataMapper As DataMapper)
        m_Template = template
        m_Name = name
        m_DataMapper = dataMapper
    End Sub

    Public Function CreateFile(ByVal path As Uri) As MGFile
        Dim f As New MGFile(Me)
        m_DataMapper.WriteNewFile(f, Me)
        SetTag(f, "FileName", path.AbsoluteUri)
        RaiseEvent ItemAdded(Me, New MGFileEventArgs(f))
        Return f
    End Function

    Friend Function GetTag(ByVal file As MGFile, ByVal tagName As String, Optional ByVal tran As DbTransaction = Nothing) As String
        Return m_DataMapper.GetTag(file, tagName)
    End Function

    Friend Sub SetTag(ByVal file As MGFile, ByVal tagName As String, ByVal tagValue As String, Optional ByVal tran As DbTransaction = Nothing)
        m_DataMapper.WriteTag(file, tagName, tagValue)
    End Sub

    Private Function GetFiles() As IList(Of MGFile)
        Return m_DataMapper.Getfiles(Me)
    End Function

    Friend Function GetTag(ByVal fileId As Long) As MGTagCollection
        Return m_DataMapper.GetAllTags(fileId)
    End Function

    Public Sub AddTaggingPlugin(ByVal assemblyQualifiedTypeName As String)
        Dim t = Type.GetType(assemblyQualifiedTypeName, True)
        Dim plugin = CType(Activator.CreateInstance(t), IMGTaggingPlugin)
        AddTaggingPlugin(plugin)
    End Sub
    Public Sub AddTaggingPlugin(ByVal plugin As IMGTaggingPlugin)
        plugin.Startup(Me)
        m_TaggingPlugins.Add(plugin)
    End Sub

    Public ReadOnly Property Plugins() As IEnumerable(Of IMGTaggingPlugin)
        Get
            Return m_TaggingPlugins
        End Get
    End Property

    Public Function GetAllFiles() As FileSet
        Return New FileSet(GetFiles())
    End Function

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

#Region "Disk Persistance"

    Public Sub WriteToFile(ByVal filename As String)
        Using fileStream As Stream = File.OpenWrite(filename)
            Dim bf As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            bf.Serialize(fileStream, Me)
        End Using
    End Sub

    Public Shared Function ReadFromFile(ByVal filename As String) As MGDataStore
        Dim ds As MGDataStore
        Using fileStream As Stream = File.OpenRead(filename)
            Dim bf As New System.Runtime.Serialization.Formatters.Binary.BinaryFormatter()
            ds = (DirectCast(bf.Deserialize(fileStream), MGDataStore))
        End Using

        ds.m_TaggingPlugins = New List(Of IMGTaggingPlugin)
        Return ds
    End Function

#End Region

    Public Sub Close()
        For Each plugin As IMGTaggingPlugin In m_TaggingPlugins
            plugin.Shutdown()
        Next
    End Sub

    Public Event ItemAdded As EventHandler(Of MGFileEventArgs)

    Public Overrides Function ToString() As String
        Return "MGDataStore: " & Name
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGFile) Implements System.Collections.Generic.IEnumerable(Of MGFile).GetEnumerator
        Return GetFiles().GetEnumerator()
    End Function
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function

    Public Sub AddDirectory(ByVal dir As String)
        If File.Exists(dir) Then
            CreateFile(New Uri(dir))
        ElseIf Directory.Exists(dir) Then
            For Each file In Directory.GetFiles(dir)
                CreateFile(New Uri(file))
            Next
        Else
            Throw New Exception(String.Format("Can't find path: {0}", dir))
        End If
    End Sub

    Public Function ToCsv() As String
        Dim sb As New StringBuilder()
        sb.AppendLine(Aggregate s In Template.GetDimensionNames() Order By s Into JoinToCsv(s))
        For Each f In Me
            sb.AppendLine(Aggregate t In f.GetTags() Order By t.Name Into JoinToCsv(t.Value))
        Next
        Return sb.ToString()
    End Function

End Class
