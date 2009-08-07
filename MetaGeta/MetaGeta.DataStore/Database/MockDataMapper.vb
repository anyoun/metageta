﻿Namespace Database
    Public Class MockDataMapper
        Implements IDataMapper

        Public Sub Initialize() Implements IDataMapper.Initialize

        End Sub

        Public Sub Close() Implements IDataMapper.Close

        End Sub

        Public Function GetTagOnAllFiles(ByVal dataStore As MGDataStore, ByVal tagName As String) _
            As IList(Of Tuple(Of MGTag, MGFile)) Implements IDataMapper.GetTagOnAllFiles
            Return New List(Of Tuple(Of MGTag, MGFile))
        End Function

        Public Function GetDataStores(ByVal owner As IDataStoreOwner) As IEnumerable(Of MGDataStore) Implements IDataMapper.GetDataStores
            Return New MGDataStore() {New MGDataStore(owner, Me) With {.Name = "Sample DataStore"}}
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

        Public Sub WriteFile(ByVal file As MGFile) Implements IDataMapper.WriteFile

        End Sub
    End Class
End Namespace