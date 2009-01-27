Public Interface IMGFileSourcePlugin
    Inherits IMGPluginBase

    Function GetFilesToAdd() As ICollection(Of Uri)

End Interface