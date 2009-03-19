Public Interface IMGFileActionPlugin
    Function GetActions() As IEnumerable(Of String)
    Sub DoAction(ByVal action As String, ByVal file As MGFile, ByVal progress As ProgressStatus)
End Interface