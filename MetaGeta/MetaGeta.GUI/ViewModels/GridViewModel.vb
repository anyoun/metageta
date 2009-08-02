Public Class GridViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStore As MGDataStore
    Private m_SelectedFiles As IList(Of MGFile)

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
        AddHandler m_DataStore.PropertyChanged, AddressOf DataStorePropertyChanged

        m_ConvertToIphoneCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf ConvertToIphone, AddressOf IsNonEmpty, Function() SelectedFiles)
        m_WriteMp4TagsCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf WriteMp4Tags, AddressOf IsNonEmpty, Function() SelectedFiles)
        m_RemoveFileCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf RemoveFile, AddressOf IsNonEmpty, Function() SelectedFiles)
        m_ShowPropertiesCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf ShowProperties, AddressOf IsExactlyOne, Function() SelectedFiles)
    End Sub

    Private Sub DataStorePropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If e.PropertyName = "Files" Then
            OnPropertyChanged("Files")
        End If
    End Sub

    Public ReadOnly Property ColumnNames() As String()
        Get
            Return m_DataStore.Template.GetColumnNames()
        End Get
    End Property

    Public ReadOnly Property Files() As IList(Of MGFile)
        Get
            Return m_DataStore.Files
        End Get
    End Property

    Public Property SelectedFiles() As IList(Of MGFile)
        Get
            Return m_SelectedFiles
        End Get
        Set(ByVal value As IList(Of MGFile))
            If m_SelectedFiles IsNot value Then
                m_SelectedFiles = value
                OnPropertyChanged("SelectedFiles")
            End If
        End Set
    End Property

#Region "Commands"

#Region "Properties"
    Private m_ConvertToIphoneCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property ConvertToIphoneCommand() As ICommand
        Get
            Return m_ConvertToIphoneCommand
        End Get
    End Property
    Private m_WriteMp4TagsCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property WriteMp4TagsCommand() As ICommand
        Get
            Return m_WriteMp4TagsCommand
        End Get
    End Property
    Private m_RemoveFileCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property RemoveFileCommand() As ICommand
        Get
            Return m_RemoveFileCommand
        End Get
    End Property
    Private m_ShowPropertiesCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property ShowPropertiesCommand() As ICommand
        Get
            Return m_ShowPropertiesCommand
        End Get
    End Property
#End Region

#Region "Execute"
    Public Sub ConvertToIphone(ByVal param As IList(Of MGFile))
        For Each f In param
            m_DataStore.DoAction(f, TranscodePlugin.TranscodePlugin.ConvertActionName)
        Next
    End Sub

    Public Sub WriteMp4Tags(ByVal param As IList(Of MGFile))
        For Each f In param
            m_DataStore.DoAction(f, TranscodePlugin.Mp4TagWriterPlugin.c_WriteTagsAction)
        Next
    End Sub

    Public Sub RemoveFile(ByVal param As IList(Of MGFile))
        m_DataStore.RemoveFiles(param)
    End Sub

    Public Sub ShowProperties(ByVal param As IList(Of MGFile))
        'Dim win As New FilePropertiesView(param.First())
        'PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        'win.Owner = Window.GetWindow(Me)
        'win.ShowDialog()
    End Sub
#End Region

#Region "Command helper functions"
    Private Shared Function IsNonEmpty(ByVal files As IList(Of MGFile)) As Boolean
        Return files IsNot Nothing AndAlso files.Count > 0
    End Function
    Private Shared Function IsExactlyOne(ByVal files As IList(Of MGFile)) As Boolean
        Return files IsNot Nothing AndAlso files.Count = 1
    End Function
#End Region

#End Region

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return "Grid"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return s_ViewImage
        End Get
    End Property

    Private Shared ReadOnly s_ViewImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/view_detailed.png"))
End Class
