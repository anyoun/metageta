Imports System.ComponentModel

Partial Public Class DataStoreConfiguration
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore

        InitializeComponent()
        PresentationTraceSources.SetTraceLevel(Me, PresentationTraceLevel.High)
        PresentationTraceSources.SetTraceLevel(sePluginSettings, PresentationTraceLevel.High)
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Public Sub lstPlugins_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs) Handles lstPlugins.SelectionChanged
        OnPropertyChanged("SelectedPluginSettings")
    End Sub

    Public ReadOnly Property SelectedPluginSettings() As SettingInfoCollection
        Get
            Return SettingInfoCollection.GetSettings(CType(lstPlugins.SelectedItem, IMGPluginBase))
        End Get
    End Property

    Private Sub btnDeleteDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnDeleteDataStore.Click
        DataStore.Delete()
    End Sub

    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
