Imports System.Text
Imports System.IO

<Serializable()> _
Public Class MGDataStore
    'Implements INotifyCollectionChanged
    Implements IEnumerable(Of MGFile)

    Private m_Items As New List(Of MGFile)
    Private m_Dimensions As New Dictionary(Of String, Dimension)

    Private m_Name As String
    Private m_Description As String

    <NonSerialized()> _
    Private m_TaggingPlugins As New List(Of IMGTaggingPlugin)

    Public Sub New(ByVal template As IDataStoreTemplate)
        For Each s As String In template.GetDimensionNames()
            m_Dimensions.Add(s, New Dimension(s))
        Next
    End Sub

    Public Sub NewFile(ByVal path As Uri)
        Dim f As New MGFile(path, Guid.NewGuid())
        m_Items.Add(f)

        For Each plugin As IMGTaggingPlugin In m_TaggingPlugins
            'plugin.ItemAdded(f)
        Next

        'Bucket along dimensions

        For Each d As Dimension In m_Dimensions.Values
            d.IndexNewItem(f)
        Next

        RaiseEvent ItemAdded(Me, New MGFileEventArgs(f))
        'RaiseEvent CollectionChanged(Me, New Collections.Specialized.NotifyCollectionChangedEventArgs(Specialized.NotifyCollectionChangedAction.Add, f))

        'Threading.Thread.Sleep(1000)
    End Sub

    Public Sub AddTaggingPlugin(ByVal plugin As IMGTaggingPlugin)
        plugin.Startup(Me)
        m_TaggingPlugins.Add(plugin)
    End Sub

    Public Function Browse(ByVal path As String) As Browser
        Return New Browser(Me, path)
    End Function

    Friend Function GetFilesWhere(ByVal tag As MGTag) As FileSet
        Return m_Dimensions(tag.Name).Item(tag.Value)
    End Function
    Friend Function GetAllFiles() As FileSet
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
    'Public Event CollectionChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs) Implements INotifyCollectionChanged.CollectionChanged

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

