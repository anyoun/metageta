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