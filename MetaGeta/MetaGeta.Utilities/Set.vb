Public Class LookupSet(Of T As IEquatable(Of T))
    Implements IEnumerable(Of T)

    Private ReadOnly m_Items As New Dictionary(Of T, Boolean)

    Public Sub New()

    End Sub

    Public Sub New(ByVal items As IEnumerable(Of T))
        Me.New()
        For Each item In items
            m_Items(item) = True
        Next
    End Sub

    Public Sub Add(ByVal item As T)
        m_Items(item) = True
    End Sub

    Public Sub Remove(ByVal item As T)
        m_Items.Remove(item)
    End Sub

    Public Function Contains(ByVal item As T) As Boolean
        Return m_Items.ContainsKey(item)
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of T) Implements System.Collections.Generic.IEnumerable(Of T).GetEnumerator
        Return m_Items.Keys.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return m_Items.Keys.GetEnumerator()
    End Function
End Class
