Imports System.ComponentModel
Imports System.Reflection
Imports Microsoft.Windows.Controls

Partial Public Class GridView
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataStore As MGDataStore
    Private m_SelectedFiles As IList(Of MGFile)

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

    Private m_ConvertToIphoneCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property ConvertToIphoneCommand() As ICommand
        Get
            If m_ConvertToIphoneCommand Is Nothing Then
                m_ConvertToIphoneCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf ConvertToIphone, AddressOf IsNonEmpty, AddressOf GetSelectedFiles)
            End If
            Return m_ConvertToIphoneCommand
        End Get
    End Property
    Public Sub ConvertToIphone(ByVal param As IList(Of MGFile))
        For Each f In param
            DataStore.DoAction(f, TranscodePlugin.TranscodePlugin.ConvertActionName)
        Next
    End Sub

    Private m_WriteMp4TagsCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property WriteMp4TagsCommand() As ICommand
        Get
            If m_WriteMp4TagsCommand Is Nothing Then
                m_WriteMp4TagsCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf WriteMp4Tags, AddressOf IsNonEmpty, AddressOf GetSelectedFiles)
            End If
            Return m_WriteMp4TagsCommand
        End Get
    End Property
    Public Sub WriteMp4Tags(ByVal param As IList(Of MGFile))
        For Each f In param
            DataStore.DoAction(f, TranscodePlugin.Mp4TagWriterPlugin.c_WriteTagsAction)
        Next
    End Sub

    Private m_RemoveFileCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property RemoveFileCommand() As ICommand
        Get
            If m_RemoveFileCommand Is Nothing Then
                m_RemoveFileCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf RemoveFile, AddressOf IsNonEmpty, AddressOf GetSelectedFiles)
            End If
            Return m_RemoveFileCommand
        End Get
    End Property
    Public Sub RemoveFile(ByVal param As IList(Of MGFile))
        DataStore.RemoveFiles(param)
    End Sub

    Private m_ShowPropertiesCommand As RelayCommand(Of IList(Of MGFile))
    Public ReadOnly Property ShowPropertiesCommand() As ICommand
        Get
            If m_ShowPropertiesCommand Is Nothing Then
                m_ShowPropertiesCommand = New RelayCommand(Of IList(Of MGFile))(AddressOf ShowProperties, AddressOf IsExactlyOne, AddressOf GetSelectedFiles)
            End If
            Return m_ShowPropertiesCommand
        End Get
    End Property
    Public Sub ShowProperties(ByVal param As IList(Of MGFile))
        Dim win As New FilePropertiesView(param.First())
        PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        win.Owner = Window.GetWindow(Me)
        win.ShowDialog()
    End Sub

    Private Shared Function IsNonEmpty(ByVal files As IList(Of MGFile)) As Boolean
        Return files IsNot Nothing AndAlso files.Count > 0
    End Function
    Private Shared Function IsExactlyOne(ByVal files As IList(Of MGFile)) As Boolean
        Return files IsNot Nothing AndAlso files.Count = 1
    End Function

    Public Function GetSelectedFiles() As IList(Of MGFile)
        Return m_SelectedFiles
    End Function

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged

    Private Sub DataGrid1_SelectedCellsChanged(ByVal sender As System.Object, ByVal e As Microsoft.Windows.Controls.SelectedCellsChangedEventArgs)
        Dim cells = DataGrid1.SelectedCells.ToArray()
        m_SelectedFiles = (From cell In cells Select CType(cell.Item, MGFile)).Distinct().ToList()
    End Sub
End Class
