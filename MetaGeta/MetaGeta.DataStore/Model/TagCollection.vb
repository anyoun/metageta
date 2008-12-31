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