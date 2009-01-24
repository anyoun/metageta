<Serializable()> _
Public Class MGFile
    Implements IEquatable(Of MGFile)

    Private ReadOnly m_ID As Long
    Private ReadOnly m_DataStore As MGDataStore

    Friend Sub New(ByVal id As Long, ByVal datastore As MGDataStore)
        m_ID = id
        m_DataStore = datastore
    End Sub

    Public Function GetTag(ByVal tagName As String) As String
        Return m_DataStore.GetTag(Me, tagName)
    End Function

    Public Sub SetTag(ByVal tagName As String, ByVal tagValue As String)
        m_DataStore.SetTag(Me, tagName, tagValue)
    End Sub

    Public ReadOnly Property ID() As Long
        Get
            Return m_ID
        End Get
    End Property

    Public Overloads Function Equals(ByVal other As MGFile) As Boolean Implements System.IEquatable(Of MGFile).Equals
        Return other.ID = ID
    End Function

    Public ReadOnly Property FileName() As String
        Get
            Return GetTag("FileName")
        End Get
    End Property

    Public Function GetTags() As MGTagCollection
        Throw New NotImplementedException()
    End Function
End Class

Public Class MGFileIDComparer
    Implements IComparer(Of MGFile)

    Public Function Compare(ByVal x As MGFile, ByVal y As MGFile) As Integer Implements IComparer(Of MGFile).Compare
        Return x.ID.CompareTo(y.ID)
    End Function
End Class

Public Class MGFileEventArgs
    Inherits EventArgs

    Private ReadOnly m_File As MGFile

    Public Sub New(ByVal f As MGFile)
        m_File = f
    End Sub

    Public ReadOnly Property File() As MGFile
        Get
            Return m_File
        End Get
    End Property

End Class