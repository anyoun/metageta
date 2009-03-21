Imports MetaGeta.DataStore
Imports MediaInfoLib
Imports System.Text.RegularExpressions

Public Class MediaInfoPlugin
    Implements IMGTaggingPlugin, IMGPluginBase

    Private Shared ReadOnly log As log4net.ILog = log4net.LogManager.GetLogger(Reflection.MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore
    Private m_MediaInfo As MediaInfoWrapper
    Private m_ID As Long

    Public Sub New()

    End Sub

    Public ReadOnly Property ID() As Long Implements IMGPluginBase.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public Sub Startup(ByVal dataStore As MGDataStore, ByVal id As Long) Implements IMGPluginBase.Startup
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


    Public Sub Process(ByVal file As MGFile, ByVal reporter As ProgressStatus) Implements IMGTaggingPlugin.Process
        Dim fileInfo = m_MediaInfo.ReadFile(file.FileName)

        file.SetTag(TVShowDataStoreTemplate.Format, fileInfo.Format)

        If fileInfo.AudioStreams.Count > 0 Then
            Dim audio = fileInfo.AudioStreams.First()
            file.SetTag(TVShowDataStoreTemplate.AudioCodec, audio.CodecString)
        End If

        If fileInfo.VideoStreams.Count > 0 Then
            Dim video = fileInfo.VideoStreams.First()
            file.SetTag(TVShowDataStoreTemplate.VideoCodec, video.CodecString)
            file.SetTag(TVShowDataStoreTemplate.VideoCodecProfile, video.CodecProfile)
            file.SetTag(TVShowDataStoreTemplate.Resolution, String.Format("{0}x{1}", video.WidthPx, video.HeightPx))
            file.SetTag(TVShowDataStoreTemplate.VideoWidthPx, video.WidthPx.ToString())
            file.SetTag(TVShowDataStoreTemplate.VideoHeightPx, video.HeightPx.ToString())
            file.SetTag(TVShowDataStoreTemplate.PlayTime, video.PlayTime.ToString())
            file.SetTag(TVShowDataStoreTemplate.VideoDisplayAspectRatio, video.DisplayAspectRatio.ToString())
            file.SetTag(TVShowDataStoreTemplate.FrameCount, video.FrameCount.ToString())
            file.SetTag(TVShowDataStoreTemplate.FrameRate, video.FrameRate.ToString())
        End If

        file.SetTag(TVShowDataStoreTemplate.iPod5GCompatible, fileInfo.IsCompatible(DeviceType.iPod5G).ToString())
        file.SetTag(TVShowDataStoreTemplate.iPodClassicCompatible, fileInfo.IsCompatible(DeviceType.iPodClassic).ToString())
        file.SetTag(TVShowDataStoreTemplate.iPhoneCompatible, fileInfo.IsCompatible(DeviceType.iPhone).ToString())
    End Sub

    Public Sub Shutdown() Implements IMGPluginBase.Shutdown
        m_DataStore = Nothing
        m_MediaInfo = Nothing
    End Sub

End Class
