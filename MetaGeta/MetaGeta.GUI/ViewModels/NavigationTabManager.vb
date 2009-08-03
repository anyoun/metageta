Imports System.Collections.ObjectModel
Imports System.Collections.Specialized
Imports System.ComponentModel

Public Class NavigationTabManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataStoreManager As DataStoreManager
    Private ReadOnly m_TabGroups As New ObservableCollection(Of NavigationTabGroupBase)
    Private ReadOnly m_MetaGetaTabGroup As NamedNavigationTabGroup

    Public Sub New(ByVal dataStoreManager As DataStoreManager)
        Me.New()

        m_DataStoreManager = dataStoreManager
        AddHandler m_DataStoreManager.DataStores.CollectionChanged, AddressOf DataStoresChanged
        m_MetaGetaTabGroup.Children.Add(New JobQueueViewModel(dataStoreManager))
        AddTabGroups(dataStoreManager.DataStores)
    End Sub

    Private Sub New()
        m_MetaGetaTabGroup = New NamedNavigationTabGroup("MetaGeta")
        m_MetaGetaTabGroup.Children.Add(New NullViewModel("Settings", s_ConfigureImage))
        m_TabGroups.Add(m_MetaGetaTabGroup)
    End Sub


    Private Sub DataStoresChanged(ByVal sender As Object, ByVal e As NotifyCollectionChangedEventArgs)
        Select Case e.Action
            Case NotifyCollectionChangedAction.Reset
                RemoveTabGroupForDataStore(e.OldItems.Cast(Of MGDataStore)())
                AddTabGroups(e.NewItems.Cast(Of MGDataStore)())

            Case NotifyCollectionChangedAction.Add
                AddTabGroups(e.NewItems.Cast(Of MGDataStore)())

            Case NotifyCollectionChangedAction.Move
                RemoveTabGroupForDataStore(e.OldItems.Cast(Of MGDataStore)())
                AddTabGroups(e.NewItems.Cast(Of MGDataStore)())

            Case NotifyCollectionChangedAction.Remove
                RemoveTabGroupForDataStore(e.OldItems.Cast(Of MGDataStore)())

            Case NotifyCollectionChangedAction.Replace
                RemoveTabGroupForDataStore(e.OldItems.Cast(Of MGDataStore)())
                AddTabGroups(e.NewItems.Cast(Of MGDataStore)())

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

    Public Class DesignTimeNavigationTabManager
        Inherits NavigationTabManager

        Public Sub New()
            MyBase.New(New DataStoreManager(designMode:=True))
        End Sub
    End Class
End Class
