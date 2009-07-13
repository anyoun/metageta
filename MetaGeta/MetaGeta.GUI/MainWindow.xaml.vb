Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MainWindow
    Implements INotifyPropertyChanged
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_Navigation As NavigationTabManager
    Private WithEvents m_DataStoreManager As DataStoreManager

    Public Sub New()
        DataStoreManager.IsInDesignMode = DesignerProperties.GetIsInDesignMode(Me)
        m_DataStoreManager = New DataStoreManager()
        m_Navigation = New NavigationTabManager(m_DataStoreManager)
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

    Public ReadOnly Property NavigationManager() As NavigationTabManager
        Get
            Return m_Navigation
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
            ds.SetCreationArguments(args)
        End If
    End Sub

#End Region

    Private Sub OnMyPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class