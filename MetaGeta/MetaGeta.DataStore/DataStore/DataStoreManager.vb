Public Class DataStoreManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataMapper As DataMapper
    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)

    Public Sub New(Optional ByVal filename As String = "metageta.db3")
        m_DataMapper = New DataMapper(filename)
    End Sub

    Public Sub Startup()
        m_DataMapper.Initialize()
        m_DataStores.AddRange(m_DataMapper.GetDataStores())
        OnDataStoresChanged()
    End Sub

    Public Sub Shutdown()
        For Each ds In m_DataStores
            ds.Close()
        Next
        m_DataMapper.Close()
    End Sub

    Public Function NewDataStore(ByVal name As String, ByVal template As IDataStoreTemplate) As MGDataStore
        Dim data As New MGDataStore(template, name, m_DataMapper)
        m_DataMapper.WriteNewDataStore(data)
        m_DataStores.Add(data)
        OnDataStoresChanged()
        Return data
    End Function

    Public ReadOnly Property DataStores() As ObservableCollection(Of MGDataStore)
        Get
            Return m_DataStores
        End Get
    End Property

    Private Sub OnDataStoresChanged()
        OnPropertyChanged("DataStores")
    End Sub
    Private Sub OnPropertyChanged(ByVal name As String)
        RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs(name))
    End Sub
    Public Event PropertyChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements System.ComponentModel.INotifyPropertyChanged.PropertyChanged
End Class

Public Class DesignTimeDataStoreManager
    Inherits DataStoreManager

    Public Sub New()
        MyBase.New("c:\temp\metageta.db3")
        Me.Startup()
    End Sub
End Class