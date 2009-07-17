Public Interface IDataStoreOwner
    Sub DeleteDataStore(ByVal dataStore As MGDataStore)
    Sub EnqueueAction(ByVal action As String, ByVal dataStore As MGDataStore, ByVal file As MGFile)
End Interface
