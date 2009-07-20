Public Class Tuple(Of TFirst, TSecond)
    Implements IEquatable(Of Tuple(Of TFirst, TSecond))

    Private ReadOnly m_First As TFirst
    Private ReadOnly m_Second As TSecond

    Public Sub New(ByVal first As TFirst, ByVal second As TSecond)
        m_First = first
        m_Second = second
    End Sub

    Public ReadOnly Property First() As TFirst
        Get
            Return m_First
        End Get
    End Property
    Public ReadOnly Property Second() As TSecond
        Get
            Return m_Second
        End Get
    End Property

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return TypeOf (obj) Is Tuple(Of TFirst, TSecond) AndAlso Equals1(CType(obj, Tuple(Of TFirst, TSecond)))
    End Function
    Public Overrides Function GetHashCode() As Integer
        Return First.GetHashCode() Xor Second.GetHashCode()
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("First = {0}, Second = {1}", First, Second)
    End Function

    Public Function Equals1(ByVal other As Tuple(Of TFirst, TSecond)) As Boolean Implements System.IEquatable(Of Tuple(Of TFirst, TSecond)).Equals
        Return EqualityComparer(Of TFirst).Default.Equals(First, other.First) _
        AndAlso EqualityComparer(Of TSecond).Default.Equals(Second, other.Second)
    End Function
End Class

Public Class Tuple(Of T1, T2, T3)
    Implements IEquatable(Of Tuple(Of T1, T2, T3))

    Private ReadOnly m_First As T1
    Private ReadOnly m_Second As T2
    Private ReadOnly m_Third As T3

    Public Sub New(ByVal first As T1, ByVal second As T2, ByVal third As T3)
        m_First = first
        m_Second = second
        m_Third = third
    End Sub

    Public ReadOnly Property First() As T1
        Get
            Return m_First
        End Get
    End Property
    Public ReadOnly Property Second() As T2
        Get
            Return m_Second
        End Get
    End Property

    Public ReadOnly Property Third() As T3
        Get
            Return m_Third
        End Get
    End Property

    Public Overrides Function Equals(ByVal obj As Object) As Boolean
        Return TypeOf (obj) Is Tuple(Of T1, T2, T3) AndAlso Equals1(CType(obj, Tuple(Of T1, T2, T3)))
    End Function
    Public Overrides Function GetHashCode() As Integer
        Return First.GetHashCode() Xor Second.GetHashCode() Xor Third.GetHashCode()
    End Function
    Public Overrides Function ToString() As String
        Return String.Format("First = {0}, Second = {1}, Third = {2}", First, Second, Third)
    End Function

    Public Function Equals1(ByVal other As Tuple(Of T1, T2, T3)) As Boolean Implements System.IEquatable(Of Tuple(Of T1, T2, T3)).Equals
        Return EqualityComparer(Of T1).Default.Equals(First, other.First) _
        AndAlso EqualityComparer(Of T2).Default.Equals(Second, other.Second) _
        AndAlso EqualityComparer(Of T3).Default.Equals(Third, other.Third)
    End Function
End Class
