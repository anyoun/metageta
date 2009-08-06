<Serializable()> _
Public Class MGTagCollection
    Implements IEnumerable(Of MGTag)

    Private ReadOnly m_Items As New Dictionary(Of String, MGTag)

    Public Sub New()

    End Sub

    Public Sub New(ByVal tags As IEnumerable(Of MGTag))
        For Each tag In tags
            m_Items.Add(tag.Name, tag)
        Next
    End Sub

    Public Overloads ReadOnly Property Item(ByVal tagName As String) As MGTag
        Get
            Dim tag As MGTag = Nothing
            If m_Items.TryGetValue(tagName, tag) Then
                Return tag
            Else
                Return Nothing
            End If
        End Get
    End Property

    Public Function GetValue(ByVal tagName As String) As String
        Dim tag As MGTag = Nothing
        If m_Items.TryGetValue(tagName, tag) Then
            Return tag.Value
        Else
            Return Nothing
        End If
    End Function

    Public Sub SetValue(ByVal tagName As String, ByVal value As String)
        If value Is Nothing Then
            m_Items.Remove(tagName)
        Else
            m_Items(tagName) = New MGTag(tagName, value)
        End If
    End Sub

    Public Function GetEnumerator() As System.Collections.Generic.IEnumerator(Of MGTag) Implements System.Collections.Generic.IEnumerable(Of MGTag).GetEnumerator
        Return m_Items.Values.GetEnumerator()
    End Function

    Public Function GetEnumerator1() As System.Collections.IEnumerator Implements System.Collections.IEnumerable.GetEnumerator
        Return GetEnumerator()
    End Function
End Class