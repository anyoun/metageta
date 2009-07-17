Partial Public Class JobQueueView
    Private ReadOnly m_DataStoreManager As DataStoreManager

    Public Sub New(ByVal dataStoreManager As DataStoreManager)
        m_DataStoreManager = dataStoreManager
        InitializeComponent()
    End Sub

    Public ReadOnly Property DataStoreManager() As DataStoreManager
        Get
            Return m_DataStoreManager
        End Get
    End Property
End Class
