Imports MetaGeta.DataStore
Imports TvdbConnector
Imports TvdbConnector.Cache
Imports TvdbConnector.Data
Imports MetaGeta.TVShowPlugin

Public Class TVDBPlugin
    Implements IMGTaggingPlugin

    Private m_DataStore As MGDataStore
    Private m_tvdbHandler As Tvdb

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

            file.Tags.Item(TVShowDataStoreTemplate.EpisodeTitle).Value = exactEpisode.EpisodeName
            file.Tags.Item(TVShowDataStoreTemplate.EpisodeDescription).Value = exactEpisode.Overview
            file.Tags.Item(TVShowDataStoreTemplate.EpisodeID).Value = exactEpisode.Id.ToString()

            If exactEpisode.Banner.LoadBanner() Then
                Console.WriteLine("Found banner: ""{0}"".", exactEpisode.Banner.BannerPath)
                Dim imagefile = System.IO.Path.GetTempFileName()
                exactEpisode.Banner.Banner.Save(imagefile)
                file.Tags.Item(TVShowDataStoreTemplate.EpisodeBanner).Value = imagefile
            End If

        Next
    End Sub

    Private Function GetSeriesID(ByVal file As MGFile) As Integer?
        If Not file.Tags.Item(TVShowDataStoreTemplate.SeriesID).IsSet Then
            Dim seriesName = file.Tags.Item(TVShowDataStoreTemplate.SeriesTitle).Value
            Dim searchResult = m_tvdbHandler.SearchSeries(seriesName)
            Dim exactMatch = searchResult.Find(Function(sr) sr.SeriesName = seriesName)
            If Not exactMatch Is Nothing Then
                Console.WriteLine("Found series: ""{0}"".", exactMatch.SeriesName)
                file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsInteger = exactMatch.Id
                Return exactMatch.Id
            Else
                Console.WriteLine("Couldn't find series: ""{0}"".", seriesName)
                Return Nothing
            End If
        End If
        Return file.Tags.Item(TVShowDataStoreTemplate.SeriesID).ValueAsInteger
    End Function

    Private Function GetSeries(ByVal seriesID As Integer) As TvdbSeries
        Return m_tvdbHandler.GetSeries(seriesID, TvdbLanguage.DefaultLanguage, True, False, False, True)
    End Function

    Private Function GetEpisode(ByVal series As TvdbSeries, ByVal file As MGFile) As TvdbEpisode
        Dim seasonNumber = file.Tags.Item(TVShowDataStoreTemplate.SeasonNumber).ValueAsInteger
        Dim episodeNumber = file.Tags.Item(TVShowDataStoreTemplate.EpisodeNumber).ValueAsInteger
        Dim ep = m_tvdbHandler.GetEpisode(series.Id, seasonNumber, episodeNumber, TvdbEpisode.EpisodeOrdering.DefaultOrder, TvdbLanguage.DefaultLanguage)
        If Not ep Is Nothing Then
            Console.WriteLine("Found episode: {0} - s{1}e{2} - {3}.", series.SeriesName, seasonNumber, episodeNumber, ep.EpisodeName)
        Else
            Console.WriteLine("Couldn't find episode: {0} - s{1}e{2}.", series.SeriesName, seasonNumber, episodeNumber)
        End If
        Return ep
    End Function
End Class
