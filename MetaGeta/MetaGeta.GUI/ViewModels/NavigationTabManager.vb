Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel

Public Class NavigationTabManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataStoreManager As DataStoreManager
    Private ReadOnly m_TabGroups As New ObservableCollection(Of NavigationTabGroupBase)

    Public Sub New(ByVal dataStoreManager As DataStoreManager)
        m_DataStoreManager = dataStoreManager
        AddHandler m_DataStoreManager.DataStores.CollectionChanged, AddressOf DataStoresChanged

        Dim mgGroup As New NamedNavigationTabGroup("MetaGeta")
        mgGroup.Children.Add(New NullViewModel("Settings", s_ConfigureImage))
        mgGroup.Children.Add(New JobQueueViewModel(dataStoreManager))
        m_TabGroups.Add(mgGroup)

        AddTabGroups(dataStoreManager.DataStores)

        'm_BaseTabs = New NavigationTab() { _
        '    New NavigationTab(New NullView(), s_ConfigureImage, "Settings") With {.Group = "MetaGeta"}, _
        '    New NavigationTab(New JobQueueView(dataStoreManager), s_ConfigureImage, "Jobs") With {.Group = "MetaGeta"} _
        '}
    End Sub

    Private Sub DataStoresChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        Dim oldItems = e.OldItems.Cast(Of MGDataStore)()
        Dim newItems = e.NewItems.Cast(Of MGDataStore)()

        Select Case e.Action
            Case NotifyCollectionChangedAction.Reset
                RemoveTabGroupForDataStore(oldItems)
                AddTabGroups(newItems)

            Case NotifyCollectionChangedAction.Add
                AddTabGroups(newItems)

            Case NotifyCollectionChangedAction.Move
                RemoveTabGroupForDataStore(oldItems)
                AddTabGroups(newItems)

            Case NotifyCollectionChangedAction.Remove
                RemoveTabGroupForDataStore(oldItems)

            Case NotifyCollectionChangedAction.Replace
                RemoveTabGroupForDataStore(oldItems)
                AddTabGroups(newItems)

            Case Else
                Throw New Exception()
        End Select
    End Sub

    Private Sub RemoveTabGroupForDataStore(ByVal dataStores As IEnumerable(Of MGDataStore))
        For Each foo In dataStores
            Dim ds = foo
            m_TabGroups.Remove(m_TabGroups.First(Function(tab) TypeOf tab Is DataStoreNavigationTabGroup AndAlso CType(tab, DataStoreNavigationTabGroup).DataStore Is ds))
        Next
    End Sub

    Private Sub AddTabGroups(ByVal dataStores As IEnumerable(Of MGDataStore))
        For Each ds In dataStores
            m_TabGroups.Add(CreateTabGroup(ds))
        Next
    End Sub

    Private Function CreateTabGroup(ByVal dataStore As MGDataStore) As NavigationTabGroupBase
        Dim group As New DataStoreNavigationTabGroup(dataStore)

        group.Children.Add(New DataStoreConfigurationViewModel(dataStore))
        group.Children.Add(New GridViewModel(dataStore))
        group.Children.Add(New ImportStatusViewModel(dataStore))
        'group.Children.Add(New TvShowViewModel(dataStore))

        'Dim tabs = New List(Of NavigationTab)

        'Dim configTab = New NavigationTab(New DataStoreConfigurationView(dataStore), s_RunImage, "Configuration")
        'BindingOperations.SetBinding(configTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        'tabs.Add(configTab)

        'Dim gridViewTab = New NavigationTab(New GridView(dataStore), s_ViewImage, "Grid") With {.Group = dataStore.Name}
        'BindingOperations.SetBinding(gridViewTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        'tabs.Add(gridViewTab)

        'Dim importTab = New NavigationTab(New ImportStatusView(dataStore), s_ViewImage, "Import") With {.Group = dataStore.Name}
        'BindingOperations.SetBinding(importTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        'tabs.Add(importTab)

        'If False Then
        '    Dim tvShowTab As New NavigationTab(New TvShowView(dataStore), s_ViewImage, "TV Show") With {.Group = dataStore.Name}
        '    BindingOperations.SetBinding(tvShowTab, NavigationTab.GroupProperty, New Binding("Name") With {.Source = dataStore})
        '    tabs.Add(tvShowTab)
        'End If

        'Return tabs
        Return group
    End Function

    Public ReadOnly Property Tabs() As ObservableCollection(Of NavigationTabGroupBase)
        Get
            Return m_TabGroups
        End Get
    End Property

    Private Sub OnTabsChanged()
        OnPropertyChanged("Tabs")
    End Sub
    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs) Implements INotifyPropertyChanged.PropertyChanged

    Private Shared ReadOnly s_ConfigureImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"))
End Class
