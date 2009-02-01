Public Class TVShowDataStoreTemplate
    Implements IDataStoreTemplate

    Public Function GetDimensionNames() As String() Implements IDataStoreTemplate.GetDimensionNames
        Return New String() {SeriesTitle, EpisodeTitle, EpisodeNumber, SeasonNumber, PartNumber, _
                             SeriesID, EpisodeID, SeriesDescription, EpisodeDescription, EpisodeBanner, _
                             CRC32, Group, AudioCodec, VideoCodec, Resolution, PlayTime}
    End Function

    Public Function GetColumnNames() As String() Implements IDataStoreTemplate.GetColumnNames
        Return New String() {SeriesTitle, EpisodeTitle, EpisodeNumber, SeasonNumber, PartNumber, _
                             CRC32, Group, AudioCodec, VideoCodec, Resolution, PlayTime}
    End Function

    Public Function GetPluginTypeNames() As String() Implements IDataStoreTemplate.GetPluginTypeNames
        Return New String() { _
                             "MetaGeta.TVShowPlugin.EducatedGuessImporter, TVShowPlugin", _
                             "MetaGeta.DirectoryFileSourcePlugin.DirectoryFileSourcePlugin, DirectoryFileSourcePlugin"}
        '"MetaGeta.TVDBPlugin.TVDBPlugin, TVDBPlugin", _
        '"MetaGeta.MediaInfoPlugin.MediaInfoPlugin, MediaInfoPlugin", _
    End Function

    Public Function GetName() As String Implements IDataStoreTemplate.GetName
        Return "TVShow"
    End Function

#Region "Constants"
    Public Const SeriesTitle As String = "SeriesTitle"
    Public Const EpisodeTitle As String = "EpisodeTitle"

    Public Const EpisodeNumber As String = "EpisodeNumber"
    Public Const SeasonNumber As String = "SeasonNumber"
    Public Const PartNumber As String = "PartNumber"

    Public Const SeriesID As String = "SeriesID"
    Public Const EpisodeID As String = "EpisodeID"

    Public Const EpisodeBanner As String = "EpisodeBanner"

    Public Const SeriesDescription As String = "SeriesDescription"
    Public Const EpisodeDescription As String = "EpisodeDescription"

    Public Const CRC32 As String = "CRC32"
    Public Const Group As String = "Group"

    Public Const AudioCodec As String = "AudioCodec"
    Public Const VideoCodec As String = "VideoCodec"
    Public Const Resolution As String = "Resolution"
    Public Const PlayTime As String = "PlayTime"
#End Region
End Class