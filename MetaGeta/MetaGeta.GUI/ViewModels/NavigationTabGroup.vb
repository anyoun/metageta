Public MustInherit Class NavigationTabGroupBase
    Inherits NavigationTab
    Private ReadOnly m_Children As New ObservableCollection(Of NavigationTab)
    Private m_SelectedChild As NavigationTab

    Public ReadOnly Property Children() As ObservableCollection(Of NavigationTab)
        Get
            Return m_Children
        End Get
    End Property

    Public Property SelectedChild() As NavigationTab
        Get
            Return m_SelectedChild
        End Get
        Set(ByVal value As NavigationTab)
            If m_SelectedChild IsNot value Then
                m_SelectedChild = value
                OnPropertyChanged("SelectedChild")
            End If
        End Set
    End Property
End Class


Public Class NamedNavigationTabGroup
    Inherits NavigationTabGroupBase

    Private ReadOnly m_Caption As String

    Public Sub New(ByVal caption As String)
        m_Caption = caption
    End Sub

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return m_Caption
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As ImageSource
        Get
            Return s_MetaGetaImage
        End Get
    End Property

    Private Shared ReadOnly s_MetaGetaImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/MetaGeta_Image.png"))
End Class

Public Class DataStoreNavigationTabGroup
    Inherits NavigationTabGroupBase

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
        AddHandler m_DataStore.PropertyChanged, AddressOf DataStore_PropertyChanged
    End Sub

    Private Sub DataStore_PropertyChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
        If e.PropertyName = "Name" Then
            OnPropertyChanged("Caption")
        End If
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return m_DataStore.Name
        End Get
    End Property

    ReadOnly Overrides Public Property Icon() As ImageSource
        Get
            Return s_DatabaseImage
        End Get
    End Property

    Private Shared ReadOnly s_DatabaseImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/db.png"))
End Class