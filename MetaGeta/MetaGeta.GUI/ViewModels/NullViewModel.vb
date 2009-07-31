Public Class NullViewModel
    Inherits NavigationTab

    Private ReadOnly m_Caption As String
    Private ReadOnly m_Icon As System.Windows.Media.ImageSource

    Public Sub New(ByVal caption As String, ByVal icon As ImageSource)
        m_Caption = caption
        m_Icon = icon
    End Sub

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return m_Caption
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return m_Icon
        End Get
    End Property
End Class
