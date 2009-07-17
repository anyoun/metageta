Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MasterDetailGridView
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore

        ' This call is required by the Windows Form Designer.
        InitializeComponent()
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Public ReadOnly Property SelectedDataStoreColumnsView() As ViewBase
        Get
            Dim grid As New GridView()
            If Not DataStore Is Nothing Then
                grid.Columns.Clear()

                For Each t In DataStore.Template.GetColumnNames()
                    Dim col = New GridViewColumn
                    Dim b As New Binding()
                    b.Converter = New MGFileConverter()
                    b.ConverterParameter = t
                    b.Mode = BindingMode.OneWay
                    col.DisplayMemberBinding = b
                    col.Header = t
                    grid.Columns.Add(col)
                Next
            End If
            Return grid
        End Get
    End Property

    Private Sub lvItems_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs) Handles lvItems.SelectionChanged
        OnMyPropertyChanged("IsFileSelectionValid")
    End Sub

    Private ReadOnly Property SelectedFiles() As IEnumerable(Of MGFile)
        Get
            Return lvItems.SelectedItems.Cast(Of MGFile)()
        End Get
    End Property
    Public ReadOnly Property IsFileSelectionValid() As Boolean
        Get
            Return lvItems.SelectedItems.Count > 0
        End Get
    End Property

#Region "Button event handlers"

    Private Sub btnWriteTags_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnWriteTags.Click
        For Each f In SelectedFiles
            DataStore.DoAction(f, TranscodePlugin.Mp4TagWriterPlugin.c_WriteTagsAction)
        Next
    End Sub
    Private Sub btnConvertToIpod_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConvertToIpod.Click
        For Each f In SelectedFiles
            DataStore.DoAction(f, TranscodePlugin.TranscodePlugin.ConvertActionName)
        Next
    End Sub
    Private Sub btnRemove_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnRemove.Click
        DataStore.RemoveFiles(SelectedFiles.ToArray())
    End Sub
#End Region

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class
