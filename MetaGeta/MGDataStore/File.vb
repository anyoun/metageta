Imports System.Reflection

<Serializable()> _
Public Class MGTagCollection
    Inherits List(Of MGTag)

    Public Overloads ReadOnly Property Item(ByVal tagName As String) As MGTag
        Get
            For Each t As MGTag In Me
                If t.Name = tagName Then
                    Return t
                End If
            Next

            Dim newTag As New MGTag(tagName, String.Empty)
            Add(newTag)
            Return newTag
        End Get
    End Property
    Public Function HasTag(ByRef tag As MGTag) As Boolean
        For Each t As MGTag In Me
            If t.Equals(tag) Then
                Return True
            End If
        Next
        Return False
    End Function
End Class

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


    Public ReadOnly Property Something() As String
        Get
            Return Path.ToString()
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

<Serializable()> _
Public Class MGTag
    Implements IEquatable(Of MGTag)

    Private m_Value As String
    Private m_Type As TagType
    Private m_Name As String
    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property
    Public Property Value() As String
        Get
            Return m_Value
        End Get
        Set(ByVal value As String)
            m_Value = value
        End Set
    End Property
    Public ReadOnly Property Type() As TagType
        Get
            Return m_Type
        End Get
    End Property

    Public Sub New(ByVal name As String, ByVal value As String)
        m_Name = name
        m_Value = value
    End Sub

    Public Overloads Function Equals(ByVal other As MGTag) As Boolean Implements System.IEquatable(Of MGTag).Equals
        Return other.Name = Name AndAlso other.Value = Value AndAlso other.Type = Type
    End Function

    Public Enum TagType
        Text
        Number
        File
    End Enum
End Class


Public Interface IMGTaggingPlugin
    Sub Initialize(ByVal dataStore As MGDataStore)
    Sub ItemAdded(ByVal File As MGFile)
    Sub Close()
End Interface

Public Class MGFileEventArgs
    Inherits EventArgs

    Public File As MGFile

    Public Sub New(ByVal f As MGFile)
        File = f
    End Sub

End Class