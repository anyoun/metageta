Public Class FileView
    Implements IEnumerable(Of MGFile)

    Private m_Files As IEnumerable(Of MGFile)

    Friend Sub New(ByVal files As IEnumerable(Of MGFile))
        m_Files = From f In files Order By f.Name
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGFile) Implements System.Collections.Generic.IEnumerable(Of MGFile).GetEnumerator
        Return m_Files.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class