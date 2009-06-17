Imports System.ComponentModel

Partial Public Class DataStoreConfiguration
    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore

        InitializeComponent()
        PresentationTraceSources.SetTraceLevel(Me, PresentationTraceLevel.High)
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Private Sub btnDeleteDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnDeleteDataStore.Click
        DataStore.Delete()
    End Sub
End Class
