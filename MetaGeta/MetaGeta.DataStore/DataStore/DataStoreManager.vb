Public Class DataStoreManager
    Implements INotifyPropertyChanged

    Private ReadOnly m_DataMapper As DataMapper
    Private ReadOnly m_DataStores As New ObservableCollection(Of MGDataStore)

    Public Sub New()
        Dim filename As String
        If IsInDesignMode Then
            filename = "c:\temp\metageta.db3"
        Else
            filename = "metageta.db3"
        End If
        m_DataMapper = New DataMapper(filename)

        m_DataMapper.Initialize()
        m_DataStores.AddRange(m_DataMapper.GetDataStores())
        OnDataStoresChanged()
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
        Dim plugins As New List(Of IMGPluginBase)
        For Each pluginTypeName In template.GetPluginTypeNames()
            Dim t = Type.GetType(pluginTypeName)
            If t Is Nothing Then
                Throw New Exception(String.Format("Can't find type ""{0}"".", pluginTypeName))
            End If
            plugins.Add(CType(Activator.CreateInstance(t), IMGPluginBase))
        Next
        Dim data As New MGDataStore(template, name, plugins, m_DataMapper)
        m_DataMapper.WriteNewDataStore(Data)
        m_DataStores.Add(Data)
        OnDataStoresChanged()
        Return Data
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
