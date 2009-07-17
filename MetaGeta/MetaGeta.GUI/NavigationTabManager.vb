Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel

Public Class NavigationTabManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataStoreManager As DataStoreManager
    Private ReadOnly m_DataStoreTabCache As New Dictionary(Of Long, IEnumerable(Of NavigationTab))
    Private ReadOnly m_BaseTabs As IEnumerable(Of NavigationTab)

    Public Sub New(ByVal dataStoreManager As DataStoreManager)
        m_DataStoreManager = dataStoreManager
        AddHandler m_DataStoreManager.DataStores.CollectionChanged, AddressOf DataStoresChanged

        m_BaseTabs = New NavigationTab() { _
            New NavigationTab(New EmptyView(), s_ConfigureImage, "Settings") With {.Group = "MetaGeta"}, _
            New NavigationTab(New JobQueueView(dataStoreManager), s_ConfigureImage, "Jobs") With {.Group = "MetaGeta"} _
        }
    End Sub

    Private Sub DataStoresChanged(ByVal sender As Object, ByVal e As EventArgs)
        OnNavigationTabsChanged()
    End Sub

    Private Function CreateTabs(ByVal dataStore As MGDataStore) As IEnumerable(Of NavigationTab)
        Dim tabs = New List(Of NavigationTab)

        Dim configTab = New NavigationTab(New DataStoreConfiguration(dataStore), s_RunImage, "Configuration")
        BindingOperations.SetBinding(configTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        tabs.Add(configTab)

        'Dim viewTab = New NavigationTab(New DataStoreView(dataStore), s_ViewImage, "View") With {.Group = dataStore.Name}
        'BindingOperations.SetBinding(viewTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        'tabs.Add(viewTab)

        Dim gridTab = New NavigationTab(New MasterDetailGridView(dataStore), s_ViewImage, "Grid") With {.Group = dataStore.Name}
        BindingOperations.SetBinding(gridTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        tabs.Add(gridTab)

        Dim importTab = New NavigationTab(New ImportStatusView(dataStore), s_ViewImage, "Import") With {.Group = dataStore.Name}
        BindingOperations.SetBinding(importTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        tabs.Add(importTab)

        If False Then
            Dim tvShowTab As New NavigationTab(New TVShowView(dataStore), s_ViewImage, "TV Show") With {.Group = dataStore.Name}
            BindingOperations.SetBinding(tvShowTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
            tabs.Add(tvShowTab)
        End If

        Return tabs
    End Function

    Public ReadOnly Property NavigationTabs() As IEnumerable(Of NavigationTab)
        Get
            Dim tabs As New ObservableCollection(Of NavigationTab)
            tabs.AddRange(m_BaseTabs)

            For Each ds In m_DataStoreManager.DataStores
                Dim cachedTabs As IEnumerable(Of NavigationTab) = Nothing
                If Not m_DataStoreTabCache.TryGetValue(ds.ID, cachedTabs) Then
                    cachedTabs = CreateTabs(ds)
                    m_DataStoreTabCache.Add(ds.ID, cachedTabs)
                End If
                tabs.AddRange(cachedTabs)
            Next

            Return tabs
        End Get
    End Property

    Private Sub OnNavigationTabsChanged()
        OnPropertyChanged("NavigationTabs")
    End Sub
    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged


    Public Shared ReadOnly s_RunImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/run.png"))
    Public Shared ReadOnly s_DatabaseImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/db.png"))
    Public Shared ReadOnly s_ConfigureImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"))
    Public Shared ReadOnly s_ViewImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/view_detailed.png"))
End Class
