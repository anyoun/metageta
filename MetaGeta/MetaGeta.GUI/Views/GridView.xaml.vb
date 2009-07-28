Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.Windows.Controls

Partial Public Class GridView
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore

        ' This call is required by the Windows Form Designer.
        InitializeComponent()

        Me.DataContext = Me

        For Each t In dataStore.Template.GetColumnNames()
            Dim column As New DataGridTextColumn()
            column.Binding = New Binding() With { _
                .Converter = New MGFileConverter(), _
                .ConverterParameter = t, _
                .Mode = BindingMode.OneWay}
            column.Header = t
            DataGrid1.Columns.Add(column)
        Next
    End Sub

    Private m_ConvertToIphoneCommand As RelayCommand(Of MGFile)
    Public ReadOnly Property ConvertToIphoneCommand() As ICommand
        Get
            If m_ConvertToIphoneCommand Is Nothing Then
                m_ConvertToIphoneCommand = New RelayCommand(Of MGFile)(AddressOf ConvertToIphone)
            End If
            Return m_ConvertToIphoneCommand
        End Get
    End Property
    Public Sub ConvertToIphone(ByVal param As MGFile)
        DataStore.DoAction(param, TranscodePlugin.TranscodePlugin.ConvertActionName)
    End Sub

    Private m_WriteMp4TagsCommand As RelayCommand(Of MGFile)
    Public ReadOnly Property WriteMp4TagsCommand() As ICommand
        Get
            If m_WriteMp4TagsCommand Is Nothing Then
                m_WriteMp4TagsCommand = New RelayCommand(Of MGFile)(AddressOf WriteMp4Tags)
            End If
            Return m_WriteMp4TagsCommand
        End Get
    End Property
    Public Sub WriteMp4Tags(ByVal param As MGFile)
        DataStore.DoAction(param, TranscodePlugin.Mp4TagWriterPlugin.c_WriteTagsAction)
    End Sub

    Private m_RemoveFileCommand As RelayCommand(Of MGFile)
    Public ReadOnly Property RemoveFileCommand() As ICommand
        Get
            If m_RemoveFileCommand Is Nothing Then
                m_RemoveFileCommand = New RelayCommand(Of MGFile)(AddressOf RemoveFile)
            End If
            Return m_RemoveFileCommand
        End Get
    End Property
    Public Sub RemoveFile(ByVal param As MGFile)
        DataStore.RemoveFiles(param.SingleToEnumerable())
    End Sub

    Private m_ShowPropertiesCommand As RelayCommand(Of MGFile)
    Public ReadOnly Property ShowPropertiesCommand() As ICommand
        Get
            If m_ShowPropertiesCommand Is Nothing Then
                m_ShowPropertiesCommand = New RelayCommand(Of MGFile)(AddressOf ShowProperties)
            End If
            Return m_ShowPropertiesCommand
        End Get
    End Property
    Public Sub ShowProperties(ByVal param As MGFile)
        Dim win As New FilePropertiesView(param)
        PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        win.Owner = Window.GetWindow(Me)
        win.ShowDialog()
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
