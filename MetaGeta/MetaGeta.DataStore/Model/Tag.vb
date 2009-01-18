<Serializable()> _
Public Class MGTag
    Implements IEquatable(Of MGTag)

    Private ReadOnly m_Name As String
    Private ReadOnly m_Value As String

    Public Sub New(ByVal name As String, ByVal value As String)
        m_Name = name
        m_Value = value
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property
    Public ReadOnly Property Value() As String
        Get
            Return m_Value
        End Get
    End Property
 
    Public ReadOnly Property IsSet() As Boolean
        Get
            Return m_Value <> Nothing
        End Get
    End Property


    Public Overloads Function Equals(ByVal other As MGTag) As Boolean Implements System.IEquatable(Of MGTag).Equals
        Return other.Name = Name AndAlso other.Value = Value
    End Function

End Class