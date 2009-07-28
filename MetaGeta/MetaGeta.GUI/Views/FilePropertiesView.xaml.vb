Partial Public Class FilePropertiesView

    Private ReadOnly m_File As MGFile
    Private ReadOnly m_Tags As MGTagCollection

    Public Sub New(ByVal file As MGFile)
        m_File = file
        m_Tags = m_File.Tags

        InitializeComponent()
    End Sub

    Public ReadOnly Property Tags() As MGTagCollection
        Get
            Return m_Tags
        End Get
    End Property

    Private Sub Button_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs)
        Me.Close()
    End Sub
End Class
