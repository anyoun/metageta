Imports Microsoft.Windows.Controls

Partial Public Class GridView
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_ViewModel As GridViewModel

    Public Sub New()
        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Protected Overrides Sub OnPropertyChanged(ByVal e As System.Windows.DependencyPropertyChangedEventArgs)
        MyBase.OnPropertyChanged(e)

        If e.Property Is DataContextProperty Then
            If m_ViewModel IsNot Nothing Then
                RemoveHandler m_ViewModel.PropertyChanged, AddressOf ViewModelPropertyChanged
            End If
            m_ViewModel = CType(e.NewValue, GridViewModel)

            If m_ViewModel IsNot Nothing Then
                AddHandler m_ViewModel.PropertyChanged, AddressOf ViewModelPropertyChanged
                CopyColumnDefinitionsToGrid()
            End If
        End If
    End Sub

    Private Sub CopyColumnDefinitionsToGrid()

        For Each t In m_ViewModel.ColumnNames
            Dim column As New DataGridTextColumn()
            column.Binding = New Binding() With { _
                .Converter = New MGFileConverter(), _
                .ConverterParameter = t, _
                .Mode = BindingMode.OneWay}
            column.Header = t
            DataGrid1.Columns.Add(column)
        Next
    End Sub

    Private Sub ViewModelPropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If e.PropertyName = "ColumnNames" Then
            CopyColumnDefinitionsToGrid()
        End If
    End Sub

    Private Sub DataGrid1_SelectedCellsChanged(ByVal sender As System.Object, ByVal e As Microsoft.Windows.Controls.SelectedCellsChangedEventArgs)
        If m_ViewModel IsNot Nothing Then
            m_ViewModel.SelectedFiles = (From cell In DataGrid1.SelectedCells Where TypeOf cell.Item Is MGFile Select CType(cell.Item, MGFile)).Distinct().ToList()
        End If
    End Sub
End Class
