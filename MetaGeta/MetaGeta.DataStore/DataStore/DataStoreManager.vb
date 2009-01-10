Public Class DataStoreManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)

#Region "New DataStore"

    Public Sub NewDataStore(ByVal name As String, ByVal template As IDataStoreTemplate)
        Dim data As New MGDataStore(template)

        data.Name = name
        data.Description = "description"
        m_DataStores.Add(data)
        OnDataStoresChanged()
    End Sub


    'Public Function BeginImportAll(ByVal ds As MGDataStore) As FileProgress
    '    Dim state As New ImportCallbackState
    '    state.DataStore = ds

    '    state.FilesToImport = Directory.GetFiles("\\larvandad\public\Anime", "*.mp4", SearchOption.AllDirectories).Union( _
    '                            Directory.GetFiles("\\larvandad\public2\Anime", "*.mp4", SearchOption.AllDirectories))
    '    state.Progress = New FileProgress(state.FilesToImport)

    '    ThreadPool.QueueUserWorkItem(AddressOf Import, state)

    '    Return state.Progress
    'End Function

    'Private Class ImportCallbackState
    '    Public FilesToImport As IEnumerable(Of String)
    '    Public Progress As FileProgress
    '    Public DataStore As MGDataStore
    'End Class

    'Private Sub Import(ByVal obj As Object)
    '    Dim state = DirectCast(obj, ImportCallbackState)
    '    For Each file As String In state.FilesToImport
    '        state.Progress.StartWorkingOnNextItem(file)
    '        state.DataStore.NewFile(New Uri(file, UriKind.Absolute))
    '    Next
    '    state.Progress.SetDone()
    'End Sub

#End Region

#Region "Disk Persistence"

    Public Sub SaveToDisk()
        m_DataStores.ForEach(AddressOf WriteDataStore)
    End Sub

    Private Sub WriteDataStore(ByVal ds As MGDataStore)
        ds.WriteToFile(Path.Combine(GetDataStorePersistanceDirectory(), ds.Name + ".mgds"))
    End Sub

    Public Sub LoadAllDataStoresFromDisk()
        m_DataStores.AddRange(From filename In Directory.GetFiles(GetDataStorePersistanceDirectory(), "*.mgds") Select MGDataStore.ReadFromFile(filename))
    End Sub

    Private Shared Function GetDataStorePersistanceDirectory() As String
        Return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "datastores")
    End Function
#End Region

    'Public Function StartBrowsing(ByVal ds As MGDataStore) As Browser
    '   Return ds.Browse("SeriesTitle/EpisodeNumber")
    'End Function

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