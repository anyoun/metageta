Imports System.Reflection

Partial Public Class ImportStatusView

    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        m_DataStore = DataStore
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnRefresh.Click
        If DataStore IsNot Nothing Then
            DataStore.EnqueueRefreshFileSources()
        End If
    End Sub

End Class
