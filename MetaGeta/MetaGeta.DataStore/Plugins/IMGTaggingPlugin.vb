Public Interface IMGTaggingPlugin
    Inherits IMGPluginBase

    Sub Process(ByVal files As IEnumerable(Of MGFile), ByVal reporter As IProgressReportCallback)

End Interface