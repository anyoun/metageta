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
