Imports MetaGeta.DataStore
Imports TvdbConnector
Imports TvdbConnector.Cache
Imports TvdbConnector.Data
Imports MetaGeta.TVShowPlugin
Imports log4net
Imports System.Reflection
Imports MetaGeta.Utilities

Public Class TVDBPlugin
    Implements IMGTaggingPlugin, IMGPluginBase

    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore
    Private m_tvdbHandler As Tvdb
    Private m_SeriesNameDictionary As New Dictionary(Of String, Integer?)
    Private m_ID As Long


    Public Sub Startup(ByVal dataStore As MetaGeta.DataStore.MGDataStore, ByVal id As Long) Implements IMGPluginBase.Startup
        m_DataStore = dataStore
        m_tvdbHandler = New Tvdb(New XmlCacheProvider("C:\temp\tvdbcache"), "BC8024C516DFDA3B")
        m_tvdbHandler.InitCache()
    End Sub

    Public Sub Shutdown() Implements IMGPluginBase.Shutdown
        m_tvdbHandler.SaveCache()
    End Sub

    Public ReadOnly Property ID() As Long Implements IMGPluginBase.PluginID
        Get
            Return m_ID
        End Get
    End Property

    Public Function GetFriendlyName() As String Implements DataStore.IMGPluginBase.GetFriendlyName
        Return "The TVDB Plugin"
    End Function

    Public Function GetUniqueName() As String Implements DataStore.IMGPluginBase.GetUniqueName
        Return "TVDBPlugin"
    End Function

    Public Function GetVersion() As Version Implements DataStore.IMGPluginBase.GetVersion
        Return New Version(1, 0, 0, 0)
    End Function


    Public Sub Process(ByVal file As MGFile, ByVal reporter As ProgressStatus) Implements IMGTaggingPlugin.Process
        'prompt user for series name lookups??
        Dim seriesID = GetSeriesID(file)
        If seriesID Is Nothing Then Return
        Dim series = GetSeries(seriesID.Value)

        file.SetTag(TVShowDataStoreTemplate.SeriesDescription, series.Overview)

        Dim exactEpisode = GetEpisode(series, file)

        If exactEpisode IsNot Nothing Then

            file.SetTag(TVShowDataStoreTemplate.EpisodeTitle, exactEpisode.EpisodeName)
            file.SetTag(TVShowDataStoreTemplate.EpisodeDescription, exactEpisode.Overview)
            file.SetTag(TVShowDataStoreTemplate.EpisodeID, exactEpisode.Id.ToString())
            file.SetTag(TVShowDataStoreTemplate.EpisodeFirstAired, exactEpisode.FirstAired.ToUniversalTime().ToString("u"))

            If False AndAlso exactEpisode.Banner.LoadBanner() Then
                log.DebugFormat("Found banner: ""{0}"".", exactEpisode.Banner.BannerPath)

                Dim imagefile = System.IO.Path.GetTempFileName()
                exactEpisode.Banner.Banner.Save(imagefile)
                file.SetTag(TVShowDataStoreTemplate.EpisodeBanner, imagefile)

            End If
        End If
    End Sub

    Private Function GetSeriesID(ByVal file As MGFile) As Integer?
        If file.GetTag(TVShowDataStoreTemplate.SeriesID) <> Nothing Then
            Return Integer.Parse(file.GetTag(TVShowDataStoreTemplate.SeriesID))
        End If

        Dim seriesName = file.GetTag(TVShowDataStoreTemplate.SeriesTitle)

        If seriesName Is Nothing Then Return Nothing

        Dim seriesID As Integer?

        If Not m_SeriesNameDictionary.TryGetValue(seriesName, seriesID) Then
            Dim searchResult = m_tvdbHandler.SearchSeries(seriesName)
            If searchResult.Count = 0 Then
                log.DebugFormat("Couldn't find series: ""{0}"".", seriesName)
                seriesID = Nothing
            ElseIf searchResult.Count = 1 Then
                log.DebugFormat("Found series: ""{0}"" -> ""{1}"".", seriesName, searchResult.Single().SeriesName)
                seriesID = searchResult.Single().Id
            Else
                log.DebugFormat("Multiple results for ""{0}"": {1}", seriesName, searchResult.Select(Function(s) s.SeriesName).JoinToString(", "))
                Dim exactMatch = searchResult.FirstOrDefault(Function(s) String.Equals(s.SeriesName, seriesName, StringComparison.CurrentCultureIgnoreCase))
                If (Not exactMatch Is Nothing) Then
                    seriesID = exactMatch.Id
                    TVDBPlugin.log.Debug("Found an exact match.")
                Else
                    seriesID = Nothing
                    TVDBPlugin.log.Debug("No exact match.")
                End If

            End If
            m_SeriesNameDictionary.Add(seriesName, seriesID)
        End If
        If seriesID.HasValue Then
            file.SetTag(TVShowDataStoreTemplate.SeriesID, seriesID.Value.ToString())
            Return seriesID.Value
        Else
            Return Nothing
        End If
    End Function

    Private Function GetSeries(ByVal seriesID As Integer) As TvdbSeries
        log.DebugFormat("Getting series {0}. IsCached: {1}", seriesID, m_tvdbHandler.IsCached(seriesID, TvdbLanguage.DefaultLanguage, True, False, False))
        Dim s = m_tvdbHandler.GetSeries(seriesID, TvdbLanguage.DefaultLanguage, True, False, False, True)
        log.DebugFormat("{0} is {1}", seriesID, s)
        Return s
    End Function

    Private Function GetEpisode(ByVal series As TvdbSeries, ByVal file As MGFile) As TvdbEpisode
        Dim seasonNumber = Integer.Parse(file.GetTag(TVShowDataStoreTemplate.SeasonNumber))
        Dim episodeNumber = Integer.Parse(file.GetTag(TVShowDataStoreTemplate.EpisodeNumber))
        Dim ep As TvdbEpisode = Nothing
        Try
            ep = series.Episodes.Find(Function(episode) episode.EpisodeNumber = episodeNumber AndAlso episode.SeasonNumber = seasonNumber)
        Catch ex As Exception
            log.WarnFormat("TVDB exception", ex)
        End Try

        If Not ep Is Nothing Then
            log.DebugFormat("Found episode: {0} - s{1}e{2} - {3}.", series.SeriesName, seasonNumber, episodeNumber, ep.EpisodeName)
        Else
            log.DebugFormat("Couldn't find episode: {0} - s{1}e{2}.", series.SeriesName, seasonNumber, episodeNumber)
        End If
        Return ep
    End Function
End Class
