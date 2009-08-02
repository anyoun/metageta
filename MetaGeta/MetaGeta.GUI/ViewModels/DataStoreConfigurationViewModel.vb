Public Class DataStoreConfigurationViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStore As MGDataStore
    Private m_SelectedPlugin As IMGPluginBase

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
        AddHandler m_DataStore.PropertyChanged, AddressOf DataStore_PropertyChanged

        m_DeleteCommand = New RelayCommand(AddressOf Delete)
    End Sub


    Private m_DeleteCommand As RelayCommand
    Public ReadOnly Property DeleteCommand() As ICommand
        Get
            Return m_DeleteCommand
        End Get
    End Property
    Private Sub Delete()
        m_DataStore.Delete()
    End Sub

    Public Property Name() As String
        Get
            Return m_DataStore.Name
        End Get
        Set(ByVal value As String)
            m_DataStore.Name = value
        End Set
    End Property

    Public Property Description() As String
        Get
            Return m_DataStore.Description
        End Get
        Set(ByVal value As String)
            m_DataStore.Description = value
        End Set
    End Property

    Public ReadOnly Property Plugins() As IEnumerable(Of IMGPluginBase)
        Get
            Return m_DataStore.Plugins
        End Get
    End Property
    Public Property SelectedPlugin() As IMGPluginBase
        Get
            Return m_SelectedPlugin
        End Get
        Set(ByVal value As IMGPluginBase)
            If m_SelectedPlugin IsNot value Then
                m_SelectedPlugin = value
                OnPropertyChanged("SelectedPlugin")
                OnPropertyChanged("SelectedPluginSettings")
            End If
        End Set
    End Property

    Private Sub DataStore_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If e.PropertyName = "Name" Then
            OnPropertyChanged("Name")
        ElseIf e.PropertyName = "Description" Then
            OnPropertyChanged("Description")
        End If
    End Sub

    Public ReadOnly Property SelectedPluginSettings() As SettingInfoCollection
        Get
            If SelectedPlugin Is Nothing Then
                Return Nothing
            Else
                Return SettingInfoCollection.GetSettings(CType(SelectedPlugin, IMGPluginBase))
            End If
        End Get
    End Property

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return "Configuration"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return s_RunImage
        End Get
    End Property

    Private Shared ReadOnly s_RunImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/run.png"))
End Class
