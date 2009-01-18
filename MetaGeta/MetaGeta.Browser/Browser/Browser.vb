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
        'm_CurrentFiles = m_DataStore.GetAllFiles()
        'For Each tag As MGTag In m_CurrentLocation
        '    If (m_CurrentFiles Is Nothing) Then
        '        m_CurrentFiles = m_DataStore.GetFilesWhere(tag)
        '    Else
        '        m_CurrentFiles = FileSet.Intesect(m_CurrentFiles, m_DataStore.GetFilesWhere(tag))
        '    End If
        'Next
    End Sub

    Private ReadOnly Property NextTagName() As String
        Get
            Return m_DimensionPath(m_CurrentLocation.Count)
        End Get
    End Property

End Class
