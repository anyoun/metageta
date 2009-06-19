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

    Public ReadOnly Property FriendlyName() As String Implements DataStore.IMGPluginBase.FriendlyName
        Get
            Return "FileInfoTaggingPlugin"
        End Get
    End Property
    Public ReadOnly Property UniqueName() As String Implements DataStore.IMGPluginBase.UniqueName
        Get
            Return "FileInfoTaggingPlugin"
        End Get
    End Property
    Public ReadOnly Property Version() As Version Implements DataStore.IMGPluginBase.Version
        Get
            Return New Version(1, 0, 0, 0)
        End Get
    End Property
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

    Public Event SettingChanged(ByVal sender As Object, ByVal e As System.ComponentModel.PropertyChangedEventArgs) Implements IMGPluginBase.SettingChanged
End Class
