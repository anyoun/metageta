Public Class ImportFileAction
    Implements IMGFileActionPlugin

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
    End Sub

    Private Function GetActions() As IEnumerable(Of String) Implements IMGFileActionPlugin.GetActions
        Return New String() {c_ImportAction}
    End Function

    Private Sub DoAction(ByVal action As String, ByVal file As MGFile, ByVal progress As ProgressStatus) Implements IMGFileActionPlugin.DoAction
        If action <> c_ImportAction Then Throw New ArgumentException()
        m_DataStore.ImportFile(file, progress)
    End Sub

    Public Const c_ImportAction As String = "Import"
End Class
