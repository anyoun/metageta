Namespace Editors
    Partial Public Class StringInputPrompt

        Private ReadOnly m_LabelText As String
        Private m_EditingString As String

        Public Sub New(ByVal caption As String, ByVal prompt As String)
            m_LabelText = prompt

            InitializeComponent()

            Me.Title = caption
        End Sub

        Private Sub btnOK_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
            DialogResult = True
        End Sub

        Private Sub btnCancel_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
            DialogResult = False
        End Sub

        Public ReadOnly Property LabelText() As String
            Get
                Return m_LabelText
            End Get
        End Property

        Public Property EditingString() As String
            Get
                Return m_EditingString
            End Get
            Set(ByVal value As String)
                m_EditingString = value
            End Set
        End Property
    End Class
End Namespace