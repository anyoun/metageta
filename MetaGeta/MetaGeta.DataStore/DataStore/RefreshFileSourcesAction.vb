Public Class RefreshFileSourcesAction
    Implements IMGFileActionPlugin

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
    End Sub

    Private Function GetActions() As IEnumerable(Of String) Implements IMGFileActionPlugin.GetActions
        Return New String() {c_RefreshFileSourcesAction}
    End Function

    Private Sub DoAction(ByVal action As String, ByVal file As MGFile, ByVal progress As ProgressStatus) Implements IMGFileActionPlugin.DoAction
        If action <> c_RefreshFileSourcesAction OrElse file IsNot Nothing Then Throw New ArgumentException()
        m_DataStore.RefreshFileSources()
    End Sub

    Public Const c_RefreshFileSourcesAction As String = "RefreshFileSources"
End Class
