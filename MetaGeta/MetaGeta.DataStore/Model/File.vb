<Serializable()> _
Public Class MGFile
    Implements IEquatable(Of MGFile)

    Private m_ID As Long = -1
    Private ReadOnly m_DataStore As MGDataStore
    Private m_Tags As New MGTagCollection()

    Friend Sub New(ByVal datastore As MGDataStore)
        m_DataStore = datastore
    End Sub
    Friend Sub New(ByVal id As Long, ByVal datastore As MGDataStore)
        m_DataStore = datastore
        m_ID = id
    End Sub
    Public Sub New()
        Me.New(Nothing)
    End Sub

    Public Function GetTag(ByVal tagName As String) As String
        Return m_Tags.GetValue(tagName)
    End Function

    Public Sub SetTag(ByVal tagName As String, ByVal tagValue As String)
        m_Tags.SetValue(tagName, tagValue)
    End Sub

    Public Property ID() As Long
        Get
            Return m_ID
        End Get
        Set(ByVal value As Long)
            If m_ID <> value Then
                m_ID = value
            End If
        End Set
    End Property

    Public Overloads Function Equals(ByVal other As MGFile) As Boolean Implements System.IEquatable(Of MGFile).Equals
        Return other.ID = ID
    End Function

    Public ReadOnly Property FileName() As String
        Get
            Dim fn = GetTag(FileNameKey)
            If fn Is Nothing Then
                Throw New Exception("Can't find filename for file.")
            End If
            Return New Uri(fn).LocalPath
        End Get
    End Property

    Public ReadOnly Property Tags() As MGTagCollection
        Get
            Return m_Tags
        End Get
    End Property

#Region "Constants"
    Public Shared ReadOnly Property FileNameKey() As String
        Get
            Return "FileName"
        End Get
    End Property
    Public Shared ReadOnly Property TimeStampKey() As String
        Get
            Return "TimeStamp"
        End Get
    End Property
#End Region

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