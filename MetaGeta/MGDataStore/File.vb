<Serializable()> _
Public Class MGFile
    Implements IEquatable(Of MGFile)

    Private m_ID As Guid
    Private m_Path As Uri
    Private m_Tags As New MGTagCollection

    Friend Sub New(ByVal path As Uri, ByVal id As Guid)
        m_ID = id
        m_Path = path
    End Sub

    Public ReadOnly Property Path() As Uri
        Get
            Return m_Path
        End Get
    End Property
    Public ReadOnly Property ID() As Guid
        Get
            Return m_ID
        End Get
    End Property
    Public ReadOnly Property Tags() As MGTagCollection
        Get
            Return m_Tags
        End Get
    End Property


    Public Function Equals1(ByVal other As MGFile) As Boolean Implements System.IEquatable(Of MGFile).Equals
        Return other.Path = Path AndAlso other.ID = ID
    End Function

    Public ReadOnly Property Name() As String
        Get
            Return Path.LocalPath
        End Get
    End Property
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