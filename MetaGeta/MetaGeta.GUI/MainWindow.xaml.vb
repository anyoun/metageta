Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MainWindow
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_NavigationTabs As IEnumerable(Of NavigationTab)
    Private ReadOnly m_DataStoreManager As DataStoreManager

    Public Sub New()
        DataStoreManager.IsInDesignMode = DesignerProperties.GetIsInDesignMode(Me)
        m_DataStoreManager = New DataStoreManager()
        m_NavigationTabs = CreateNavigation()
        InitializeComponent()
        'lbDataStores.SelectedIndex = -1
        Diagnostics.PresentationTraceSources.SetTraceLevel(Me, PresentationTraceLevel.High)
    End Sub

    Private Sub MainWindow_Loaded(ByVal sender As Object, ByVal e As RoutedEventArgs) Handles MainWindow.Loaded

    End Sub
    Private Sub MainWindow_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles MainWindow.Closing
        DataStoreManager.Shutdown()
    End Sub

    Public ReadOnly Property CurrentView() As UserControl
        Get
            Return SelectedNavigationTab.View
        End Get
    End Property

    Public ReadOnly Property DataStoreManager() As DataStoreManager
        Get
            Return m_DataStoreManager
        End Get
    End Property

    Public ReadOnly Property NavigationTabs() As IEnumerable(Of NavigationTab)
        Get
            Return m_NavigationTabs
        End Get
    End Property


#Region "Selection"

    'Private Sub lbDataStores_SelectionChanged(ByVal sender As Object, ByVal e As RoutedPropertyChangedEventArgs(Of Object)) Handles lbDataStores.SelectedItemChanged
    Private Sub lbDataStores_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs) Handles lbDataStores.SelectionChanged
        OnMyPropertyChanged("IsDataStoreSelectionValid")
        OnMyPropertyChanged("SelectedDataStore")
        OnMyPropertyChanged("CurrentView")
    End Sub

    Private ReadOnly Property SelectedDataStore() As MGDataStore
        Get
            Return Nothing
        End Get
    End Property
    Private ReadOnly Property SelectedNavigationTab() As NavigationTab
        Get
            Return CType(lbDataStores.SelectedItem, NavigationTab)
        End Get
    End Property
    Public ReadOnly Property IsDataStoreSelectionValid() As Boolean
        Get
            Return lbDataStores.SelectedItem IsNot Nothing
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

#End Region

    Private Function CreateNavigation() As IEnumerable(Of NavigationTab)
        Dim runImage = New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/run.png"))
        Dim dbImage = New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/db.png"))
        Dim configureImage = New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"))
        Dim viewImage = New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/view_detailed.png"))

        Dim tabs = New List(Of NavigationTab)

        tabs.Add(New NavigationTab(New EmptyView(), configureImage, "Settings") With {.Group = "MetaGeta"})

        For Each ds In DataStoreManager.DataStores
            tabs.Add(New NavigationTab(New EmptyView(), runImage, "Configuration") With {.Group = ds.Name})
            tabs.Add(New NavigationTab(New DataStoreView(ds), viewImage, "View") With {.Group = ds.Name})
        Next

        Return tabs
    End Function

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class