Imports MetaGeta.DataStore
Imports TvdbConnector
Imports TvdbConnector.Cache
Imports TvdbConnector.Data
Imports MetaGeta.TVShowPlugin
Imports log4net
Imports System.Reflection

Public Class TVDBPlugin
    Implements IMGTaggingPlugin

    Private Shared ReadOnly log As ILog = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType)

    Private m_DataStore As MGDataStore
    Private m_tvdbHandler As Tvdb
    Private m_SeriesNameDictionary As New Dictionary(Of String, Integer?)

    Public Sub Startup(ByVal dataStore As MetaGeta.DataStore.MGDataStore) Implements IMGTaggingPlugin.Startup
        m_DataStore = dataStore
        m_tvdbHandler = New Tvdb(New XmlCacheProvider("C:\temp\tvdbcache"), "BC8024C516DFDA3B")
        m_tvdbHandler.InitCache()
    End Sub

    Public Sub Shutdown() Implements IMGTaggingPlugin.Shutdown
        m_tvdbHandler.SaveCache()
    End Sub

    Public Sub Process() Implements IMGTaggingPlugin.Process
        'prompt user for series name lookups??

        For Each file As MGFile In m_DataStore
            Dim seriesID = GetSeriesID(file)
            If seriesID Is Nothing Then Return
            Dim series = GetSeries(seriesID.Value)

            file.Tags.Item(TVShowDataStoreTemplate.SeriesDescription).Value = series.Overview

            Dim exactEpisode = GetEpisode(series, file)

            If exactEpisode IsNot Nothing Then

                file.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value = exactEpisode.EpisodeName
                file.Tags.Item(TVShowDataStoreTemplate.EpisodeDescription).Value = exactEpisode.Overview
                file.Tags.Item(TVShowDataStoreTemplate.EpisodeID).Value = exactEpisode.Id.ToString()

                If False AndAlso exactEpisode.Banner.LoadBanner() Then
                    log.DebugFormat("Found banner: ""{0}"".", exactEpisode.Banner.BannerPath)

                    Dim imagefile = System.IO.Path.GetTempFileName()
                    exactEpisode.Banner.Banner.Save(imagefile)
                    file.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).Value = imagefile

                End If
            End If

        Next
    End Sub

    Private Function GetSeriesID(ByVal file As MGFile) As Integer?
        If file.Tags.Item(TVShowDataStoreTemplate.SeriesID).IsSet Then
            Return file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsInteger
        End If

        Dim seriesName = file.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value
        Dim seriesID As Integer?

        If Not m_SeriesNameDictionary.TryGetValue(seriesName, seriesID) Then
            Dim searchResult = m_tvdbHandler.SearchSeries(seriesName)
            Dim exactMatch = searchResult.Find(Function(sr) sr.SeriesName.Equals(seriesName, StringComparison.CurrentCultureIgnoreCase))
            If Not exactMatch Is Nothing Then
                log.DebugFormat("Found series: ""{0}"" -> ""{1}"".", seriesName, exactMatch.SeriesName)
                seriesID = exactMatch.Id
            Else
                log.DebugFormat("Couldn't find series: ""{0}"".", seriesName)
                seriesID = Nothing
            End If
            m_SeriesNameDictionary.Add(seriesName, seriesID)
        End If
        If seriesID.HasValue Then
            file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsInteger = seriesID.Value
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
        Dim seasonNumber = file.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).ValueAsInteger
        Dim episodeNumber = file.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).ValueAsInteger
        Dim ep As TvdbEpisode = Nothing
        Try
            ep = m_tvdbHandler.GetEpisode(series.Id, seasonNumber, episodeNumber, TvdbEpisode.EpisodeOrdering.DefaultOrder, TvdbLanguage.DefaultLanguage)
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
