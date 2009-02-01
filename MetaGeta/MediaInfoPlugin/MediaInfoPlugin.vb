Imports MetaGeta.DataStore
Imports MediaInfoLib

Public Class MediaInfoPlugin
    Implements IMGTaggingPlugin

    Private m_DataStore As MGDataStore
    Private m_MediaInfo As MediaInfoWrapper
    Private m_ID As Long

    Public Sub New()

    End Sub

    Public ReadOnly Property ID() As Long Implements IMGTaggingPlugin.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long) Implements IMGTaggingPlugin.Startup
        m_DataStore = dataStore
        m_MediaInfo = New MediaInfoWrapper()
    End Sub

    Public Function GetFriendlyName() As String Implements DataStore.IMGPluginBase.GetFriendlyName
        Return "MediaInfo Plugin"
    End Function

    Public Function GetUniqueName() As String Implements DataStore.IMGPluginBase.GetUniqueName
        Return "MediaInfoPlugin"
    End Function

    Public Function GetVersion() As Version Implements DataStore.IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function


    Public Sub Process(ByVal files As IEnumerable(Of MGFile), ByVal reporter As IProgressReportCallback) Implements IMGTaggingPlugin.Process
        For Each file As MGFile In New ProgressHelper(reporter, files)
            Dim fileInfo = m_MediaInfo.ReadFile(file.FileName)

            If fileInfo.AudioStreams.Count > 0 Then
                Dim audio = fileInfo.AudioStreams.First()
                file.SetTag("AudioCodec", audio.CodecString)
            End If

            If fileInfo.VideoStreams.Count > 0 Then
                Dim video = fileInfo.VideoStreams.First()
                file.SetTag("VideoCodec", video.CodecString)
                file.SetTag("Resolution", String.Format("{0}x{1}", video.WidthPx, video.HeightPx))
                file.SetTag("PlayTime", video.PlayTime.ToString())
            End If
        Next
    End Sub

    Public Sub Shutdown() Implements IMGTaggingPlugin.Shutdown
        m_DataStore = Nothing
        m_MediaInfo = Nothing
    End Sub

End Class
