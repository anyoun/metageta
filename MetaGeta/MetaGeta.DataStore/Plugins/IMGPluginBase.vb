Public Interface IMGPluginBase
    Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long)
    Sub Shutdown()

    ReadOnly Property UniqueName() As String
    ReadOnly Property FriendlyName() As String
    ReadOnly Property Version() As Version

    ReadOnly Property PluginID() As Long

    Event SettingChanged(ByVal sender As Object, ByVal e As PropertyChangedEventArgs)
End Interface