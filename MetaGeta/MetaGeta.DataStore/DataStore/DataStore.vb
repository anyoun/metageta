<Serializable()> _
Public Class MGDataStore
    Implements IEnumerable(Of MGFile)

    Private m_Items As New List(Of MGFile)

    Private ReadOnly m_Template As IDataStoreTemplate
    Private m_Name As String
    Private m_Description As String

    <NonSerialized()> _
    Private m_TaggingPlugins As New List(Of IMGTaggingPlugin)

    Public Sub New(ByVal template As IDataStoreTemplate)
        m_Template = template
    End Sub

    Public Sub NewFile(ByVal path As Uri)
        Dim f As New MGFile(path, Guid.NewGuid())
        m_Items.Add(f)

        RaiseEvent ItemAdded(Me, New MGFileEventArgs(f))
    End Sub

    Public Sub RunTaggingPlugins()
        For Each plugin As IMGTaggingPlugin In m_TaggingPlugins
            plugin.Process()
        Next
    End Sub
    Public Sub AddTaggingPlugin(ByVal plugin As IMGTaggingPlugin)
        plugin.Startup(Me)
        m_TaggingPlugins.Add(plugin)
    End Sub

    Public Function GetFilesWhere(ByVal tag As MGTag) As FileSet
        Return New FileSet(From file In m_Items Where file.Tags.Item(tag.Name).Equals(tag))
    End Function
    Public Function GetAllFiles() As FileSet
        Return New FileSet(m_Items)
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
        Return m_Items.GetEnumerator()
    End Function
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class

