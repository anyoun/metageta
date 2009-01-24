Public Interface IMGPluginBase
    Sub Startup(ByVal dataStore As MGDataStore)
    Sub Shutdown()

    Function GetUniqueName() As String
    Function GetFriendlyName() As String
    Function GetVersion() As Version
End Interface