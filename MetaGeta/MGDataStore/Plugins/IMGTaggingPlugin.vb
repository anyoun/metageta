Public Interface IMGTaggingPlugin
    Sub Initialize(ByVal dataStore As MGDataStore)
    Sub ItemAdded(ByVal File As MGFile)
    Sub Close()
End Interface