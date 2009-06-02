Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MainWindow
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Public Sub New()
        DataStoreManager.IsInDesignMode = DesignerProperties.GetIsInDesignMode(Me)
        InitializeComponent()
        lbDataStores.SelectedIndex = -1
        Diagnostics.PresentationTraceSources.SetTraceLevel(lvQueue, PresentationTraceLevel.High)
    End Sub

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles Window1.Loaded
        OnMyPropertyChanged("SelectedDataStoreColumnsView")
    End Sub
    Private Sub MainWindow_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles Window1.Closing
        DataStoreManager.Shutdown()
    End Sub

    Private Sub RightHandGrid_DataContextChanged(ByVal sender As System.Object, ByVal e As System.Windows.DependencyPropertyChangedEventArgs) Handles RightHandGrid.DataContextChanged
        OnMyPropertyChanged("SelectedDataStoreColumnsView")
    End Sub
    Private ReadOnly Property DataStoreManager() As DataStoreManager
        Get
            Return CType(Me.DataContext, DataStoreManager)
        End Get
    End Property
    Public ReadOnly Property SelectedDataStoreColumnsView() As ViewBase
        Get
            Dim grid As New GridView()
            If Not SelectedDataStore Is Nothing Then
                grid.Columns.Clear()

                For Each t In SelectedDataStore.Template.GetColumnNames()
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

#Region "Selection"

    Private Sub lbDataStores_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs) Handles lbDataStores.SelectionChanged
        OnMyPropertyChanged("IsDataStoreSelectionValid")
    End Sub

    Private ReadOnly Property SelectedDataStore() As MGDataStore
        Get
            Return CType(lbDataStores.SelectedItem, MGDataStore)
        End Get
    End Property
    Public ReadOnly Property IsDataStoreSelectionValid() As Boolean
        Get
            Return lbDataStores.SelectedItem IsNot Nothing
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

#End Region

#Region "Button event handlers"

    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnNewDataStore.Click
        Dim win As New NewDataStoreWindow()
        PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        win.Owner = Me
        Dim dr = win.ShowDialog()
        If dr.HasValue AndAlso dr.Value Then
            Dim args = win.DataStoreCreationArguments
            Dim ds = DataStoreManager.NewDataStore(args.Name, args.Tempate)
            ds.SetCreationArguemnts(args)
        End If
    End Sub

    Private Sub btnRemoveDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnRemoveDataStore.Click
        DataStoreManager.RemoveDataStore(SelectedDataStore)
    End Sub

    Private Sub btnEditDataStore_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnEditDataStore.Click
        Dim win As New NewDataStoreWindow(SelectedDataStore.GetCreationArguments())
        PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        win.Owner = Me
        Dim dr = win.ShowDialog()
        If dr.HasValue AndAlso dr.Value Then
            SelectedDataStore.SetCreationArguemnts(win.DataStoreCreationArguments)
        End If
    End Sub

    Private Sub btnImport_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnImport.Click
        If SelectedDataStore IsNot Nothing Then
            SelectedDataStore.EnqueueRefreshFileSources()
        End If
    End Sub

    Private Sub btnWriteTags_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnWriteTags.Click
        For Each f In SelectedFiles
            SelectedDataStore.DoAction(f, TranscodePlugin.Mp4TagWriterPlugin.c_WriteTagsAction)
        Next
        tabQueue.IsSelected = True
    End Sub

    Private Sub btnGlobalSettings_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnGlobalSettings.Click
        Dim settings = DataStoreManager.GetGlobalSettings()
        Dim win As New GlobalSettingsEditor(settings)
        win.Owner = Me
        Dim result = win.ShowDialog()
        If result.HasValue AndAlso result.Value Then
            'Set settings...
            For Each setting In win.SettingsList
                DataStoreManager.SetGlobalSetting(setting.Name, setting.ValueOrNull)
            Next
        End If
    End Sub

    Private Sub btnConvertToIpod_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnConvertToIpod.Click
        For Each f In SelectedFiles
            SelectedDataStore.DoAction(f, TranscodePlugin.TranscodePlugin.ConvertActionName)
        Next
        tabQueue.IsSelected = True
    End Sub

#End Region

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class

<ValueConversion(GetType(MGFile), GetType(String), ParameterType:=GetType(String))> _
Public Class MGFileConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        Dim file As MGFile = CType(value, MGFile)
        Return file.GetTag(CType(parameter, String))
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class
