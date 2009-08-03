Imports MetaGeta.DataStore.Database

Public Class DataStoreManager
    Implements INotifyPropertyChanged
    Implements IDataStoreOwner

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private ReadOnly m_DataMapper As IDataMapper
    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)
    Private ReadOnly m_JobQueue As JobQueue

    Public Sub New(ByVal designMode As Boolean)
        log.InfoFormat("DataStoreManager ctor")

        If Not designMode Then
            m_DataMapper = New DataMapper("metageta.db3")
        Else
            m_DataMapper = New MockDataMapper()
        End If

        m_JobQueue = New JobQueue(designMode:=designMode)

        m_DataMapper.Initialize()
        m_DataStores.AddRange(m_DataMapper.GetDataStores(Me))
        OnDataStoresChanged()
    End Sub

    Public Sub WaitForQueueToEmpty()
        m_JobQueue.WaitForQueueToEmpty()
    End Sub

    Public Sub Shutdown()
        m_JobQueue.Dispose()
        For Each ds In m_DataStores
            ds.Close()
        Next
        m_DataMapper.Close()
    End Sub

    Public Function NewDataStore(ByVal name As String, ByVal template As IDataStoreTemplate) As MGDataStore
        Dim data As New MGDataStore(Me, m_DataMapper)
        Using (data.SuspendUpdates())
            data.Template = template
            data.Name = name
        End Using
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

    Public Sub EnqueueAction(ByVal action As String, ByVal dataStore As MGDataStore, ByVal file As MGFile) Implements IDataStoreOwner.EnqueueAction
        m_JobQueue.EnqueueAction(action, dataStore, file)
    End Sub

    Public ReadOnly Property JobQueue() As JobQueue
        Get
            Return m_JobQueue
        End Get
    End Property

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