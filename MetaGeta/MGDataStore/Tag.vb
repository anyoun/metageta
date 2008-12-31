<Serializable()> _
Public Class MGTag
    Implements IEquatable(Of MGTag)

    Private ReadOnly m_Type As TagType
    Private ReadOnly m_Name As String
    Private m_Value As String

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
    Public Property ValueAsNumber() As Double
        Get
            Return Double.Parse(m_Value)
        End Get
        Set(ByVal value As Double)
            m_Value = value.ToString()
        End Set
    End Property
    Public ReadOnly Property Type() As TagType
        Get
            Return m_Type
        End Get
    End Property
    Public ReadOnly Property IsSet() As Boolean
        Get
            Return Not String.IsNullOrEmpty(m_Value)
        End Get
    End Property

    Public Sub New(ByVal name As String, ByVal value As String)
        m_Name = name
        m_Value = value
    End Sub

    Public Sub New(ByVal name As String)
        m_Name = name
        m_Value = String.Empty
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


<Serializable()> _
Public Class MGTagCollection
    Implements IEnumerable(Of MGTag)

    Private ReadOnly m_Items As New Dictionary(Of String, MGTag)

    Public Overloads ReadOnly Property Item(ByVal tagName As String) As MGTag
        Get
            Dim tag As MGTag
            If Not m_Items.TryGetValue(tagName, tag) Then
                tag = New MGTag(tagName)
                m_Items.Add(tag.Name, tag)
            End If
            Return tag
        End Get
    End Property

    Public Sub SetTag(ByVal tag As MGTag)
        m_Items(tag.Name) = tag
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGTag) Implements System.Collections.Generic.IEnumerable(Of MGTag).GetEnumerator
        Return m_Items.Values.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class