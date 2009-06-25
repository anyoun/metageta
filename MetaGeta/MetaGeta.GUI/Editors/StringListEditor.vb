Namespace Editors
    Public Class StringListEditor
        Inherits ItemListEditor

        Private m_DialogCaption, m_DialogPrompt As String

        Public Property DialogCaption() As String
            Get
                Return m_DialogCaption
            End Get
            Set(ByVal value As String)
                m_DialogCaption = value
            End Set
        End Property

        Public Property DialogPrompt() As String
            Get
                Return m_DialogPrompt
            End Get
            Set(ByVal value As String)
                m_DialogPrompt = value
            End Set
        End Property

        Protected Overrides Function CreateItem() As String
            Dim prompt As New StringInputPrompt(DialogCaption, DialogPrompt)
            prompt.Owner = Window.GetWindow(Me)
            Dim result = prompt.ShowDialog()
            If result.HasValue AndAlso result.Value Then
                Return prompt.EditingString
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace