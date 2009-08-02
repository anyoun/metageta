Imports System.Text.RegularExpressions
Imports System.Net.Cache
Imports System.Globalization

Public Class TvShowViewModel
    Inherits NavigationTab

    Private ReadOnly m_DataStore As MGDataStore

    Public Sub New(ByVal dataStore As MGDataStore)
        m_DataStore = dataStore
    End Sub

    Public ReadOnly Property DataStore() As MGDataStore
        Get
            Return m_DataStore
        End Get
    End Property

    Public Overrides ReadOnly Property Caption() As String
        Get
            Return "TV Shows"
        End Get
    End Property

    Public Overrides ReadOnly Property Icon() As System.Windows.Media.ImageSource
        Get
            Return s_ViewImage
        End Get
    End Property

    Private Shared ReadOnly s_ViewImage As New BitmapImage(New Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/view_detailed.png"))
End Class

Public Class TvShowDataContext
    Inherits DesignOrRuntimeDataContext(Of ObservableCollection(Of TvSeries))

    Public Shared ReadOnly DataStoreProperty As DependencyProperty = DependencyProperty.Register("DataStore", GetType(MGDataStore), MethodBase.GetCurrentMethod().DeclaringType)

    Public Property DataStore() As MGDataStore
        Get
            Return CType(GetValue(DataStoreProperty), MGDataStore)
        End Get
        Set(ByVal value As MGDataStore)
            SetValue(DataStoreProperty, value)
        End Set
    End Property

    Protected Overrides Function CreateDesigntimeData() As ObservableCollection(Of TvSeries)
        Dim results As New ObservableCollection(Of TvSeries)
        results.Add(New TvSeries("Mythbusters") With {.SeriesBannerPath = "f:\ipod\73388-g.jpg", .SeriesPosterPath = "f:\ipod\img_posters_73388-1.jpg"})
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 8, 1, "Demolition Derby"))
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 8, 2, "Alaska Special 2"))
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 8, 3, "Banana Slip/Double-Dip"))
        results.Add(New TvSeries("Arrested Development") With {.SeriesBannerPath = "f:\ipod\72173-g.jpg", .SeriesPosterPath = "f:\ipod\72173-1.jpg"})
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 1, 1, "Pilot") With {.AirDate = New DateTime(2003, 11, 2), .Length = New TimeSpan(0, 22, 0)})
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 1, 2, "Top Banana") With {.AirDate = New DateTime(2003, 11, 9), .Length = New TimeSpan(0, 22, 0)})
        results.Last().Episodes.Add(New TvEpisode(results.Last(), 1, 3, "Bringing Up Buster") With {.AirDate = New DateTime(2003, 11, 16), .Length = New TimeSpan(0, 22, 0)})
        Return results
    End Function

    Protected Overrides Function CreateRuntimeData() As ObservableCollection(Of TvSeries)
        Dim results As New ObservableCollection(Of TvSeries)
        Dim seriesList = From f In DataStore.Files Group By SeriesName = f.GetTag(TVShowDataStoreTemplate.SeriesTitle) Into Group Order By SeriesName
        For Each seriesGroup In seriesList
            Dim series As New TvSeries(seriesGroup.SeriesName)
            series.SeriesBannerPath = seriesGroup.Group.Select(Function(file) file.GetTag(TVShowDataStoreTemplate.SeriesBanner)).Coalesce()
            series.SeriesPosterPath = seriesGroup.Group.Select(Function(file) file.GetTag(TVShowDataStoreTemplate.SeriesPoster)).Coalesce()

            Dim episodeGroups = From f In seriesGroup.Group Group By _
                                EpisodeNumber = f.GetTag(TVShowDataStoreTemplate.EpisodeNumber), _
                                SeasonNumber = f.GetTag(TVShowDataStoreTemplate.SeasonNumber) _
                                Into Group _
                                Order By SeasonNumber, EpisodeNumber _
                                Select SeasonNumber, _
                                    EpisodeNumber, _
                                    Group, _
                                    FirstAired = Group.Select(Function(file) file.GetTag(TVShowDataStoreTemplate.EpisodeFirstAired)).Coalesce(), _
                                    Length = Group.Select(Function(file) file.GetTag(TVShowDataStoreTemplate.PlayTime)).Coalesce(), _
                                    Title = Group.Select(Function(file) file.GetTag(TVShowDataStoreTemplate.EpisodeTitle)).Coalesce()
            Dim episodes = From ep In episodeGroups _
                           Select New TvEpisode(series, ep.SeasonNumber, ep.EpisodeNumber, ep.Title) With { _
                               .AirDate = DateTime.ParseExact(ep.FirstAired, "u", CultureInfo.CurrentCulture), _
                               .Length = TimeSpan.Parse(ep.Length) _
                           }

            series.Episodes.AddRange(episodes)
            results.Add(series)
        Next
        Return results
    End Function


    Protected Overrides Function CreateInstanceCore() As System.Windows.Freezable
        Return New TvShowDataContext()
    End Function
End Class

Public Class TvSeries
    Private ReadOnly m_Name As String
    Private m_Episodes As New List(Of TvEpisode)
    Private m_SeriesBannerPath, m_SeriesPosterPath As String

    Public Sub New(ByVal seriesName As String)
        m_Name = seriesName
    End Sub

    Public ReadOnly Property Name() As String
        Get
            Return m_Name
        End Get
    End Property

    Public ReadOnly Property Episodes() As IList(Of TvEpisode)
        Get
            Return m_Episodes
        End Get
    End Property

    Public Property SeriesBannerPath() As String
        Get
            Return m_SeriesBannerPath
        End Get
        Set(ByVal value As String)
            m_SeriesBannerPath = value
        End Set
    End Property

    Public Property SeriesPosterPath() As String
        Get
            Return m_SeriesPosterPath
        End Get
        Set(ByVal value As String)
            m_SeriesPosterPath = value
        End Set
    End Property
End Class

Public Class TvEpisode
    Private ReadOnly m_Title As String
    Private m_AirDate As DateTime
    Private m_Length As TimeSpan
    Private ReadOnly m_Series As TvSeries
    Private ReadOnly m_SeasonNumber As Integer, m_EpisodeNumber As Integer


    Public Sub New(ByVal series As TvSeries, ByVal seasonNumber As String, ByVal episodeNumber As String, ByVal title As String)
        Me.New(series, 1, 1, title)
        If Not Integer.TryParse(seasonNumber, m_SeasonNumber) Then m_SeasonNumber = 0
        If Not Integer.TryParse(episodeNumber, m_EpisodeNumber) Then m_EpisodeNumber = 0
    End Sub

    Public Sub New(ByVal series As TvSeries, ByVal seasonNumber As Integer, ByVal episodeNumber As Integer, ByVal title As String)
        m_Series = series
        m_Title = title
        m_AirDate = AirDate
        m_Length = Length
    End Sub

    Public ReadOnly Property Series() As TvSeries
        Get
            Return m_Series
        End Get
    End Property

    Public ReadOnly Property SeasonNumber() As Integer
        Get
            Return m_SeasonNumber
        End Get
    End Property

    Public ReadOnly Property EpisodeNumber() As Integer
        Get
            Return m_EpisodeNumber
        End Get
    End Property

    Public ReadOnly Property Title() As String
        Get
            Return m_Title
        End Get
    End Property

    Public Property AirDate() As Date
        Get
            Return m_AirDate
        End Get
        Set(ByVal value As Date)
            m_AirDate = value
        End Set
    End Property

    Public Property Length() As TimeSpan
        Get
            Return m_Length
        End Get
        Set(ByVal value As TimeSpan)
            m_Length = value
        End Set
    End Property
End Class

<ValueConversion(GetType(String), GetType(ImageSource))> _
Public Class UriImageConverter
    Implements IValueConverter

    Public Function Convert(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.Convert
        If value Is Nothing Then
            Return Nothing
        Else
            Return New BitmapImage(New Uri(CType(value, String)), New RequestCachePolicy(RequestCacheLevel.Default))
        End If
    End Function

    Public Function ConvertBack(ByVal value As Object, ByVal targetType As System.Type, ByVal parameter As Object, ByVal culture As System.Globalization.CultureInfo) As Object Implements System.Windows.Data.IValueConverter.ConvertBack
        Throw New NotImplementedException()
    End Function
End Class