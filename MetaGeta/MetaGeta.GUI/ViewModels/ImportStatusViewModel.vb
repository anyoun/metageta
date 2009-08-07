Public Class ImportStatusViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
        AddHandler m_DataStore.PropertyChanged, AddressOf DataStorePropertyChanged

        m_ImportCommand = New RelayCommand(AddressOf Import)
    End Sub

    Private Sub DataStorePropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If e.PropertyName = "ImportStatus" Then
            OnPropertyChanged("ImportStatus")
        End If
    End Sub

    Private Sub Import()
        m_DataStore.BeginRefresh()
    End Sub

    Private m_ImportCommand As RelayCommand
    Public ReadOnly Property ImportCommand() As ICommand
        Get
            Return m_ImportCommand
        End Get
    End Property

    Public ReadOnly Property ImportStatus() As ImportStatus
        Get
            Return m_DataStore.ImportStatus
        End Get
    End Property

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return "Import"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return s_ViewImage
        End Get
    End Property

    Private Shared ReadOnly s_ViewImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/fileimport.png"))
End Class
