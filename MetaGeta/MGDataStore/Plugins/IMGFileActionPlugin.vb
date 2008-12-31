Public Interface IMGFileActionPlugin
    Inherits IMGPluginBase

    Function GetActions(ByVal file As MGFile) As IEnumerable(Of String)
    Sub DoAction(ByVal action As String, ByVal file As MGFile)

End Interface