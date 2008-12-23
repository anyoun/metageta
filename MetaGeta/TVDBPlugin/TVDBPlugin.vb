Imports MetaGeta.DataStore
Imports TvdbConnector
Imports TvdbConnector.Cache
Imports TvdbConnector.Data

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

    Public Sub ItemAdded(ByVal File As MetaGeta.DataStore.MGFile) Implements MetaGeta.DataStore.IMGTaggingPlugin.ItemAdded
        'Should use cache first
        'prompt user for series name lookups??

        Dim seriesName = File.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.SeriesTitle).Value
        Dim episodeNumber = Integer.Parse(File.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.EpisodeNumber).Value)
        Dim seasonNumber = Integer.Parse(File.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.SeasonNumber).Value)

        Dim searchResult = m_tvdbHandler.SearchSeries(seriesName)
        Dim exactMatch = searchResult.Find(Function(sr) sr.SeriesName = seriesName)

        If Not exactMatch Is Nothing Then
            Dim s = m_tvdbHandler.GetSeries(exactMatch.Id, TvdbLanguage.DefaultLanguage, True, False, False, True)
            Dim exactEpisode = s.Episodes.Find(Function(e) e.SeasonNumber = seasonNumber And e.EpisodeNumber = episodeNumber)
            If Not exactEpisode Is Nothing Then
                File.Tags.Item(MetaGeta.TVShowPlugin.TVShowDataStoreTemplate.EpisodeTitle).Value = exactEpisode.EpisodeName
                'Should add description etc
            End If
        End If
    End Sub
End Class
