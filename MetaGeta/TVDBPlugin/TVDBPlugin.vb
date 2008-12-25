Imports MetaGeta.DataStore
Imports TvdbConnector
Imports TvdbConnector.Cache
Imports TvdbConnector.Data
Imports MetaGeta.TVShowPlugin

Public Class TVDBPlugin
    Implements IMGTaggingPlugin

    Private m_tvdbHandler As Tvdb

    Public Sub Initialize(ByVal dataStore As MetaGeta.DataStore.MGDataStore) Implements MetaGeta.DataStore.IMGTaggingPlugin.Initialize
        m_tvdbHandler = New Tvdb(New XmlCacheProvider("C:\temp\tvdbcache"), "BC8024C516DFDA3B")
        m_tvdbHandler.InitCache()
    End Sub

    Public Sub Close() Implements MetaGeta.DataStore.IMGTaggingPlugin.Close
        m_tvdbHandler.SaveCache()
    End Sub

    Public Sub ItemAdded(ByVal file As MetaGeta.DataStore.MGFile) Implements MetaGeta.DataStore.IMGTaggingPlugin.ItemAdded
        'Should use cache first
        'prompt user for series name lookups??

        Dim seriesID = GetSeriesID(file)
        If seriesID Is Nothing Then Return
        Dim series = GetSeries(seriesID)

        file.Tags.Item(TVShowDataStoreTemplate.SeriesDescription).Value = series.Overview

        Dim exactEpisode = GetEpisode(series, file)

        file.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value = exactEpisode.EpisodeName
        file.Tags.Item(TVShowDataStoreTemplate.EpisodeDescription).Value = exactEpisode.Overview
        file.Tags.Item(TVShowDataStoreTemplate.EpisodeID).Value = exactEpisode.Id

        If exactEpisode.Banner.LoadBanner() Then
            Dim imagefile = System.IO.Path.GetTempFileName()
            exactEpisode.Banner.Banner.Save(imagefile)
            file.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).Value = imagefile
        End If
    End Sub

    Private Function GetSeriesID(ByVal file As MGFile) As Integer?
        If Not file.Tags.Item(TVShowDataStoreTemplate.SeriesID).IsSet Then
            Dim seriesName = file.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value
            Dim searchResult = m_tvdbHandler.SearchSeries(seriesName)
            Dim exactMatch = searchResult.Find(Function(sr) sr.SeriesName = seriesName)
            If Not exactMatch Is Nothing Then
                file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsNumber = exactMatch.Id
                Return exactMatch.Id
            Else
                Return Nothing
            End If
        End If
        Return file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsNumber
    End Function

    Private Function GetSeries(ByVal seriesID As Integer) As TvdbSeries
        Return m_tvdbHandler.GetSeries(seriesID, TvdbLanguage.DefaultLanguage, True, False, False, True)
    End Function

    Private Function GetEpisode(ByVal series As TvdbSeries, ByVal file As MGFile) As TvdbEpisode
        Dim seasonNumber = file.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).ValueAsNumber
        Dim episodeNumber = file.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).ValueAsNumber
        Return m_tvdbHandler.GetEpisode(series.Id, seasonNumber, episodeNumber, TvdbEpisode.EpisodeOrdering.DefaultOrder, TvdbLanguage.DefaultLanguage)
    End Function
End Class
