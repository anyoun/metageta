Public Interface IMGPluginBase
    Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long)
    Sub Shutdown()

    Function GetUniqueName() As String
    Function GetFriendlyName() As String
    Function GetVersion() As Version

    ReadOnly Property PluginID() As Long
End Interface