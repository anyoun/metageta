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
            plugin.ItemAdded(f)
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
        plugin.Initialize(Me)
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
            plugin.Close()
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

Public Class Browser
    Private m_DimensionPath As List(Of String)
    Private m_CurrentLocation As New List(Of MGTag)
    Private m_CurrentFiles As FileSet
    Private m_DataStore As MGDataStore

    Friend Sub New(ByVal ds As MGDataStore, ByVal path As String)
        m_DataStore = ds
        m_DimensionPath = New List(Of String)(path.Split("/"c))
        UpdateCurrent()
    End Sub

    Public Function GetCurrentView() As TagView
        Return New TagView(m_CurrentFiles.GetAllTags(NextTagName()))
    End Function

    Public Function GetFileView() As FileView
        Return New FileView(m_CurrentFiles)
    End Function

    Public Sub Pop()
        m_CurrentLocation.RemoveAt(m_CurrentLocation.Count - 1)
        UpdateCurrent()
    End Sub
    Public Sub Push(ByVal tag As MGTag)
        If m_CurrentLocation.Count >= m_DimensionPath.Count Then Throw New Exception()
        If NextTagName() <> tag.Name Then Throw New Exception()

        m_CurrentLocation.Add(tag)
        UpdateCurrent()
    End Sub

    Public ReadOnly Property CanPush() As Boolean
        Get
            Return m_CurrentLocation.Count = m_DimensionPath.Count
        End Get
    End Property
    Public ReadOnly Property CanPop() As Boolean
        Get
            Return m_CurrentLocation.Count <> 0
        End Get
    End Property

    Public ReadOnly Property CurrentLocation() As String
        Get
            Return (From t In m_CurrentLocation Select t.Value).JoinToString(" } ")
        End Get
    End Property



    Private Sub UpdateCurrent()
        m_CurrentFiles = m_DataStore.GetAllFiles()
        For Each tag As MGTag In m_CurrentLocation
            If (m_CurrentFiles Is Nothing) Then
                m_CurrentFiles = m_DataStore.GetFilesWhere(tag)
            Else
                m_CurrentFiles = FileSet.Intesect(m_CurrentFiles, m_DataStore.GetFilesWhere(tag))
            End If
        Next
    End Sub

    Private ReadOnly Property NextTagName() As String
        Get
            Return m_DimensionPath(m_CurrentLocation.Count)
        End Get
    End Property

End Class

Public Class TagView
    Implements IEnumerable(Of MGTag)

    Private m_Tags As IEnumerable(Of MGTag)

    Friend Sub New(ByVal tags As IEnumerable(Of MGTag))
        m_Tags = From t In tags Order By t.Value
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of MGTag) Implements IEnumerable(Of MGTag).GetEnumerator
        Return m_Tags.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class

Public Class FileView
    Implements IEnumerable(Of MGFile)

    Private m_Files As IEnumerable(Of MGFile)

    Friend Sub New(ByVal files As IEnumerable(Of MGFile))
        m_Files = From f In files Order By f.Name
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGFile) Implements System.Collections.Generic.IEnumerable(Of MGFile).GetEnumerator
        Return m_Files.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class