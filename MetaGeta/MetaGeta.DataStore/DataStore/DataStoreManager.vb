Imports System.Reflection

Public Class DataStoreManager
    Implements INotifyPropertyChanged

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
        m_DataStores.AddRange(m_DataMapper.GetDataStores())
        LoadGlobalSettings()
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
        m_DataMapper.WriteNewDataStore(data)
        m_DataStores.Add(data)
        OnDataStoresChanged()
        Return data
    End Function

    Public Sub RemoveDataStore(ByVal dataStore As MGDataStore)
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

    Private Sub LoadGlobalSettings()
        For Each attrib In FindAllGlobalSettingsAttributes()
            m_DataMapper.CreateGlobalSetting(attrib)
        Next
    End Sub

    Public Shared Function FindAllGlobalSettingsAttributes() As IList(Of GlobalSettingAttribute)
        Assembly.Load("TVShowPlugin")
        Assembly.Load("DirectoryFileSourcePlugin")
        Assembly.Load("MediaInfoPlugin")
        Assembly.Load("TVDBPlugin")
        Assembly.Load("TranscodePlugin")

        Dim list As New List(Of GlobalSettingAttribute)
        For Each asm In AppDomain.CurrentDomain.GetAssemblies()
            For Each attrib As GlobalSettingAttribute In asm.GetCustomAttributes(GetType(GlobalSettingAttribute), False)
                list.Add(attrib)
            Next
        Next
        Return list
    End Function

    Public Function GetGlobalSettings() As IList(Of GlobalSetting)
        Return m_DataMapper.ReadGlobalSettings()
    End Function

    Public Function GetGlobalSetting(ByVal settingName As String) As String
        Return m_DataMapper.GetGlobalSetting(settingName)
    End Function
    Public Sub SetGlobalSetting(ByVal settingName As String, ByVal settingValue As String)
        m_DataMapper.WriteGlobalSetting(settingName, settingValue)
    End Sub
End Class