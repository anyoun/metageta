Imports System.IO
Imports System.Text
Imports log4net.Config
Imports System.Reflection
Imports System.ComponentModel

Partial Public Class MainWindow
    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_Navigation As NavigationTabManager
    Private m_DataStoreManager As DataStoreManager

    Public Sub New()
        m_DataStoreManager = New DataStoreManager(designMode:=False)
        m_Navigation = New NavigationTabManager(m_DataStoreManager)

        InitializeComponent()

        Diagnostics.PresentationTraceSources.SetTraceLevel(Me, PresentationTraceLevel.High)

        Me.DataContext = m_Navigation
    End Sub

    Private Sub MainWindow_Closing(ByVal sender As Object, ByVal e As EventArgs) Handles MainWindow.Closing
        m_DataStoreManager.Shutdown()
    End Sub

    Private Sub btnNew_Click(ByVal sender As System.Object, ByVal e As System.Windows.RoutedEventArgs) Handles btnNewDataStore.Click
        Dim win As New NewDataStoreWindow()
        PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
        win.Owner = Me
        Dim dr = win.ShowDialog()
        If dr.HasValue AndAlso dr.Value Then
            Dim args = win.DataStoreCreationArguments
            Dim ds = m_DataStoreManager.NewDataStore(args.Name, args.Tempate)
            ds.SetCreationArguments(args)
        End If
    End Sub

    Private Sub lbDataStores_PreviewMouseDown(ByVal sender As System.Object, ByVal e As System.Windows.Input.MouseButtonEventArgs)
        Dim originalItem As DependencyObject = CType(e.OriginalSource, DependencyObject)
        Dim nextTreeViewItem As DependencyObject = lbDataStores, lastTreeViewItem As DependencyObject = lbDataStores
        While nextTreeViewItem IsNot Nothing AndAlso TypeOf nextTreeViewItem Is ItemsControl
            lastTreeViewItem = nextTreeViewItem
            nextTreeViewItem = CType(ItemsControl.ContainerFromElement(CType(lastTreeViewItem, ItemsControl), originalItem), ItemsControl)
        End While

        If lastTreeViewItem IsNot Nothing Then
            If TypeOf CType(lastTreeViewItem, FrameworkElement).DataContext Is NavigationTabGroupBase Then
                e.Handled = True
            Else
                e.Handled = False
            End If
        End If
    End Sub

End Class