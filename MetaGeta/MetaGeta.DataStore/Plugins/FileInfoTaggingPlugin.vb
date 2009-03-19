Public Class FileInfoTaggingPlugin
    Implements IMGTaggingPlugin, IMGPluginBase

    Private m_ID As Long
    Private m_DataStore As MGDataStore

    Public Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long) Implements IMGPluginBase.Startup
        m_DataStore = dataStore
        m_ID = id
    End Sub

    Public Sub Shutdown() Implements IMGPluginBase.Shutdown

    End Sub

    Public Function GetFriendlyName() As String Implements IMGPluginBase.GetFriendlyName
        Return "FileInfoTaggingPlugin"
    End Function
    Public Function GetUniqueName() As String Implements IMGPluginBase.GetUniqueName
        Return "FileInfoTaggingPlugin"
    End Function
    Public Function GetVersion() As System.Version Implements IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function
    Public ReadOnly Property PluginID() As Long Implements IMGPluginBase.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public Sub Process(ByVal file As MGFile, ByVal reporter As ProgressStatus) Implements IMGTaggingPlugin.Process
        Dim fileInfo = New System.IO.FileInfo(file.FileName)
        file.SetTag("LastWriteTime", fileInfo.LastWriteTime.ToString())
        file.SetTag("Extension", fileInfo.Extension)
        file.SetTag("FileSizeBytes", fileInfo.Length.ToString())
    End Sub
End Class
