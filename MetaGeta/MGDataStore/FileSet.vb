<Serializable()> _
Public Class Bucket
    Inherits FileSet

    Private m_Tag As MGTag

    Public Sub New(ByVal tag As MGTag)
        m_Tag = tag
    End Sub

    Public ReadOnly Property Tag() As MGTag
        Get
            Return m_Tag
        End Get
    End Property

End Class

<Serializable()> _
Public Class FileSet
    Implements IEnumerable(Of MGFile)

    Protected m_Items As List(Of MGFile)

    Public Sub New()
        m_Items = New List(Of MGFile)()
    End Sub

    Public Sub New(ByVal c As IEnumerable(Of MGFile))
        m_Items = New List(Of MGFile)(c)
        m_Items.Sort(New MGFileIDComparer())
    End Sub

    Public Sub Add(ByVal file As MGFile)
        m_Items.Add(file)
        m_Items.Sort(New MGFileIDComparer())
    End Sub

    Public Function GetAllTags(ByVal tagName As String) As IEnumerable(Of MGTag)
        Dim tags As New List(Of MGTag)
        For Each file As MGFile In m_Items
            tags.Add(file.Tags.Item(tagName))
        Next
        Return From t In tags Group By t.Value Into tg = First() Select tg
    End Function

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGFile) Implements IEnumerable(Of MGFile).GetEnumerator
        Return m_Items.GetEnumerator()
    End Function
    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements IEnumerable.GetEnumerator
        Return m_Items.GetEnumerator()
    End Function

    Public Shared Function Intesect(ByVal left As FileSet, ByVal right As FileSet) As FileSet
        Dim common As New FileSet

        Dim l As Integer = 0, r As Integer = 0
        While (l < left.m_Items.Count AndAlso r < right.m_Items.Count)
            If left.m_Items(l).ID = right.m_Items(r).ID Then
                common.m_Items.Add(left.m_Items(l))
                l += 1
                l += 1
            ElseIf left.m_Items(l).ID.CompareTo(right.m_Items(r).ID) < 0 Then
                l += 1
            Else
                r += 1
            End If
        End While

        Return common
    End Function
End Class