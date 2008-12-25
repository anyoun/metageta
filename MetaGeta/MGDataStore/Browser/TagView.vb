Public Class TagView
    Implements IEnumerable(Of MGTag)

    Private m_Tags As IEnumerable(Of MGTag)

    Friend Sub New(ByVal tags As IEnumerable(Of MGTag))
        m_Tags = From t In tags Order By t.Value
    End Sub

    Public Function GetEnumerator() As IEnumerator(Of MGTag) Implements IEnumerable(Of MGTag).GetEnumerator
        Return m_Tags.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class
