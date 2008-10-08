Imports MetaGeta.DataStore
Imports MediaInfoLib

Public Class MediaInfoPlugin
    Implements IMGTaggingPlugin

    Private m_DataStore As MGDataStore
    Private m_MediaInfo As MediaInfoWrapper

    Public Sub New()

    End Sub

    Public Sub Initialize(ByVal dataStore As MGDataStore) Implements IMGTaggingPlugin.Initialize
        m_DataStore = dataStore
        m_MediaInfo = New MediaInfoWrapper()
    End Sub

    Public Sub ItemAdded(ByVal file As MGFile) Implements IMGTaggingPlugin.ItemAdded
        Dim fileInfo = m_MediaInfo.ReadFile(file.Path.LocalPath)

        If fileInfo.AudioStreams.Count > 0 Then
            Dim audio = fileInfo.AudioStreams.First()
            file.Tags.Add(New MGTag("AudioCodec", audio.CodecString))
        End If

        If fileInfo.VideoStreams.Count > 0 Then
            Dim video = fileInfo.VideoStreams.First()
            file.Tags.Add(New MGTag("VideoCodec", video.CodecString))
            file.Tags.Add(New MGTag("Resolution", String.Format("{0}x{1}", video.WidthPx, video.HeightPx)))
            file.Tags.Add(New MGTag("PlayTime", video.PlayTime.ToString()))
        End If

    End Sub

    Public Sub Close() Implements IMGTaggingPlugin.Close
        m_DataStore = Nothing
        m_MediaInfo = Nothing
    End Sub

End Class
