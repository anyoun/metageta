Namespace Database
    Public Interface IDataMapper
        Sub Initialize()
        Sub Close()
        Sub WriteNewDataStore(ByVal dataStore As MGDataStore)
        Sub WriteNewFiles(ByVal files As IEnumerable(Of MGFile), ByVal dataStore As MGDataStore)
        Function GetDataStores(ByVal owner As IDataStoreOwner) As IEnumerable(Of MGDataStore)
        Function GetFiles(ByVal dataStore As MGDataStore) As IList(Of MGFile)
        Function GetTagOnAllFiles(ByVal dataStore As MGDataStore, ByVal tagName As String) As IList(Of Tuple(Of MGTag, MGFile))
        Sub WriteFile(ByVal file As MGFile)
        Sub WriteDataStore(ByVal dataStore As MGDataStore)
        Sub RemoveDataStore(ByVal datastore As MGDataStore)
        Sub RemoveFiles(ByVal files As IEnumerable(Of MGFile), ByVal store As MGDataStore)
        Function GetPluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, ByVal settingName As String) As String
        Sub WritePluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, ByVal settingName As String, ByVal settingValue As String)
        Function GetGlobalSetting(ByVal settingName As String) As String
        Sub WriteGlobalSetting(ByVal settingName As String, ByVal settingValue As String)
        Function ReadGlobalSettings() As IList(Of GlobalSetting)
    End Interface
End Namespace