Namespace Database
    Public Class MockDataMapper
        Implements IDataMapper

        Public Sub Initialize() Implements IDataMapper.Initialize

        End Sub

        Public Sub Close() Implements IDataMapper.Close

        End Sub

        Public Function GetAllTagOnFiles(ByVal dataStore As MGDataStore, ByVal tagName As String) _
            As IList(Of Tuple(Of MGTag, MGFile)) Implements IDataMapper.GetAllTagOnFiles
            Return New List(Of Tuple(Of MGTag, MGFile))
        End Function

        Public Function GetAllTags(ByVal fileId As Long) As MGTagCollection Implements IDataMapper.GetAllTags
            Return New MGTagCollection(New MGTag() {})
        End Function

        Public Function GetDataStores(ByVal owner As IDataStoreOwner) As IEnumerable(Of MGDataStore) Implements IDataMapper.GetDataStores
            Return New MGDataStore() {New MGDataStore(owner, Me)}
        End Function

        Public Function GetFiles(ByVal dataStore As MGDataStore) As IList(Of MGFile) Implements IDataMapper.GetFiles
            Return New MGFile() {}
        End Function

        Public Function GetGlobalSetting(ByVal settingName As String) As String Implements IDataMapper.GetGlobalSetting
            Return Nothing
        End Function

        Public Function GetPluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, _
                                         ByVal settingName As String) As String Implements IDataMapper.GetPluginSetting
            Return String.Empty
        End Function

        Public Function GetTag(ByVal file As MGFile, ByVal tagName As String) As String Implements IDataMapper.GetTag
            Return String.Empty
        End Function

        Public Function ReadGlobalSettings() As IList(Of GlobalSetting) Implements IDataMapper.ReadGlobalSettings
            Return New GlobalSetting() {}
        End Function

        Public Sub RemoveDataStore(ByVal datastore As MGDataStore) Implements IDataMapper.RemoveDataStore

        End Sub

        Public Sub RemoveFiles(ByVal files As IEnumerable(Of MGFile), ByVal store As MGDataStore) _
            Implements IDataMapper.RemoveFiles

        End Sub

        Public Sub WriteDataStore(ByVal dataStore As MGDataStore) Implements IDataMapper.WriteDataStore

        End Sub

        Public Sub WriteGlobalSetting(ByVal settingName As String, ByVal settingValue As String) _
            Implements IDataMapper.WriteGlobalSetting

        End Sub

        Public Sub WriteNewDataStore(ByVal dataStore As MGDataStore) Implements IDataMapper.WriteNewDataStore

        End Sub

        Public Sub WriteNewFiles(ByVal files As IEnumerable(Of MGFile), ByVal dataStore As MGDataStore) _
            Implements IDataMapper.WriteNewFiles

        End Sub

        Public Sub WritePluginSetting(ByVal dataStore As MGDataStore, ByVal pluginID As Long, _
                                      ByVal settingName As String, ByVal settingValue As String) _
            Implements IDataMapper.WritePluginSetting

        End Sub

        Public Sub WriteTag(ByVal file As MGFile, ByVal tagName As String, ByVal tagValue As String) _
            Implements IDataMapper.WriteTag

        End Sub
    End Class
End Namespace