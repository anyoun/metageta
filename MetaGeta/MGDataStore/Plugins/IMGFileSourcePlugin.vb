Public Interface IMGFilesourcePlugin
    Inherits IMGPluginBase

    Function GetFiles() As IEnumerable(Of Uri)

End Interface