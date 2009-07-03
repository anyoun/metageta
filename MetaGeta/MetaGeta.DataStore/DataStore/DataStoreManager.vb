Imports System.Reflection

Public Class DataStoreManager
    Implements INotifyPropertyChanged
    Implements IDataStoreOwner

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataMapper As DataMapper
    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)

    Public Sub New()
        log.InfoFormat("DataStoreManager ctor")
        Dim filename As String
        If IsInDesignMode Then
            filename = "c:\temp\metageta.db3"
        Else
            filename = "metageta.db3"
        End If
        m_DataMapper = New DataMapper(filename)

        m_DataMapper.Initialize()
        m_DataStores.AddRange(m_DataMapper.GetDataStores(Me))
        OnDataStoresChanged()
    End Sub

    Public Sub WaitForQueueToEmpty()
        For Each ds In m_DataStores
            ds.WaitForQueueToEmpty()
        Next
    End Sub

    Public Sub Shutdown()
        For Each ds In m_DataStores
            ds.Close()
        Next
        m_DataMapper.Close()
    End Sub

    Public Function NewDataStore(ByVal name As String, ByVal template As IDataStoreTemplate) As MGDataStore
        Dim data As New MGDataStore(Me, m_DataMapper)
        data.Template = template
        data.Name = name
        For Each pluginTypeName In template.GetPluginTypeNames()
            data.AddNewPlugin(pluginTypeName)
        Next
        m_DataMapper.WriteNewDataStore(data)
        m_DataStores.Add(data)
        OnDataStoresChanged()
        Return data
    End Function

    Public Sub DeleteDataStore(ByVal dataStore As MGDataStore) Implements IDataStoreOwner.DeleteDataStore
        dataStore.Close()
        m_DataMapper.RemoveDataStore(dataStore)
        DataStores.Remove(dataStore)
    End Sub

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

#Region "Design Mode"
    Private Shared s_IsInDesignMode As Boolean = False

    Public Shared Property IsInDesignMode() As Boolean
        Get
            Return s_IsInDesignMode
        End Get
        Set(ByVal value As Boolean)
            s_IsInDesignMode = value
        End Set
    End Property
#End Region
End Class