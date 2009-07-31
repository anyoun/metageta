Public Class JobQueueViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStoreManager As DataStoreManager

    Public Sub New(ByVal dataStoreManager As DataStoreManager)
        m_DataStoreManager = dataStoreManager
    End Sub

    Public ReadOnly Property JobQueue() As JobQueue
        Get
            Return m_DataStoreManager.JobQueue
        End Get
    End Property

    ReadOnly Overrides Public Property Caption() As String
        Get
            Return "Jobs"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As ImageSource
        Get
            Return s_ConfigureImage
        End Get
    End Property

    Private Shared ReadOnly s_ConfigureImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"))
End Class
