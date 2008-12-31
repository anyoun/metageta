Imports MetaGeta.DataStore
Imports MediaInfoLib

Public Class MediaInfoPlugin
    Implements IMGTaggingPlugin

    Private m_DataStore As MGDataStore
    Private m_MediaInfo As MediaInfoWrapper

    Public Sub New()

    End Sub

    Public Sub Startup(ByVal dataStore As MGDataStore) Implements IMGTaggingPlugin.Startup
        m_DataStore = dataStore
        m_MediaInfo = New MediaInfoWrapper()
    End Sub

    Public Sub Process() Implements IMGTaggingPlugin.Process
        For Each file As MGFile In m_DataStore
            Dim fileInfo = m_MediaInfo.ReadFile(file.Path.LocalPath)

            If fileInfo.AudioStreams.Count > 0 Then
                Dim audio = fileInfo.AudioStreams.First()
                file.Tags.SetTag(New MGTag("AudioCodec", audio.CodecString))
            End If

            If fileInfo.VideoStreams.Count > 0 Then
                Dim video = fileInfo.VideoStreams.First()
                file.Tags.SetTag(New MGTag("VideoCodec", video.CodecString))
                file.Tags.SetTag(New MGTag("Resolution", String.Format("{0}x{1}", video.WidthPx, video.HeightPx)))
                file.Tags.SetTag(New MGTag("PlayTime", video.PlayTime.ToString()))
            End If
        Next
    End Sub

    Public Sub Shutdown() Implements IMGTaggingPlugin.Shutdown
        m_DataStore = Nothing
        m_MediaInfo = Nothing
    End Sub

End Class
