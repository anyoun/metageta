// Copyright 2009 Will Thomas
// 
// This file is part of MetaGeta.
// 
// MetaGeta is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MetaGeta is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MetaGeta. If not, see <http://www.gnu.org/licenses/>.

#region

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.GUI {
    public class TvShowViewModel : NavigationTab {
        private static readonly BitmapImage s_ViewImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/tv.png"));
        private readonly MGDataStore m_DataStore;

        private ObservableCollection<TvSeries> m_Series;

        public TvShowViewModel(MGDataStore dataStore) {
            m_DataStore = dataStore;
            m_DataStore.PropertyChanged += DataStorePropertyChanged;

            m_Series = CreateRuntimeData();
        }

        public override string Caption {
            get { return "TV Shows"; }
        }


        public ObservableCollection<TvSeries> Series {
            get { return m_Series; }
        }

        public override ImageSource Icon {
            get { return s_ViewImage; }
        }

        private void DataStorePropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ImportStatus") {
                m_Series = CreateRuntimeData();
                OnPropertyChanged("Series");
            }
        }

        private ObservableCollection<TvSeries> CreateRuntimeData() {
            var results = new ObservableCollection<TvSeries>();
            IOrderedEnumerable<IGrouping<string, MGFile>> seriesList = from f in m_DataStore.Files
                                                                       group f by f.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle)
                                                                       into asdf
                                                                           orderby asdf.Key
                                                                           select asdf;

            foreach (IGrouping<string, MGFile> seriesGroup in seriesList) {
                var series = new TvSeries(seriesGroup.Key);
                series.SeriesBannerPath = seriesGroup.Select(f => f.Tags.GetString(TVShowDataStoreTemplate.SeriesBanner)).Coalesce();
                series.SeriesPosterPath = seriesGroup.Select(f => f.Tags.GetString(TVShowDataStoreTemplate.SeriesPoster)).Coalesce();

                var episodeGroups = from f in seriesGroup
                                    group f by
                                        new {
                                                EpisodeNumber = f.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber),
                                                SeasonNumber = f.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber)
                                            }
                                    into grp
                                        select new {
                                                       grp.Key.SeasonNumber,
                                                       grp.Key.EpisodeNumber,
                                                       Episodes = grp,
                                                       FirstAired = grp.Select(file => file.Tags.GetDateTime(TVShowDataStoreTemplate.EpisodeFirstAired)).Coalesce(),
                                                       Length = grp.Select(file => file.Tags.GetTimeSpan(TVShowDataStoreTemplate.PlayTime)).Coalesce(),
                                                       Title = grp.Select(file => file.Tags.GetString(TVShowDataStoreTemplate.EpisodeTitle)).Coalesce(),
                                                       Ipod = grp.Any(file => true == file.Tags.GetBool(TVShowDataStoreTemplate.iPodClassicCompatible)),
                                                       Iphone = grp.Any(file => true == file.Tags.GetBool(TVShowDataStoreTemplate.iPhoneCompatible)),
                                                       Computer = grp.Any()
                                                   };

                IEnumerable<TvEpisode> episodes = from ep in episodeGroups
                                                  where ep.SeasonNumber.HasValue && ep.EpisodeNumber.HasValue
                                                  select new TvEpisode(series, ep.SeasonNumber.Value, ep.EpisodeNumber.Value, ep.Title) {
                                                                                                                                            AirDate = ep.FirstAired,
                                                                                                                                            Length = ep.Length,
                                                                                                                                            HasComputerVersion = ep.Computer,
                                                                                                                                            HasIphoneVersion = ep.Iphone,
                                                                                                                                            HasIpodVersion = ep.Ipod
                                                                                                                                        };
                episodes = from ep in episodes orderby ep.SeasonNumber , ep.EpisodeNumber select ep;

                series.Episodes.AddRange(episodes);
                results.Add(series);
            }
            return results;
        }
    }

    public class DesignTimeTvShowViewModel {
        public ObservableCollection<TvSeries> Series {
            get {
                var results = new ObservableCollection<TvSeries>();
                results.Add(new TvSeries("Mythbusters") {
                                                            SeriesBannerPath = "f:\\ipod\\73388-g.jpg",
                                                            SeriesPosterPath = "f:\\ipod\\img_posters_73388-1.jpg"
                                                        });
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 8, 1, "Demolition Derby"));
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 8, 2, "Alaska Special 2"));
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 8, 3, "Banana Slip/Double-Dip"));
                results.Add(new TvSeries("Arrested Development") {
                                                                     SeriesBannerPath = "f:\\ipod\\72173-g.jpg",
                                                                     SeriesPosterPath = "f:\\ipod\\72173-1.jpg"
                                                                 });
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 1, 1, "Pilot") {
                                                                                             AirDate = new DateTime(2003, 11, 2),
                                                                                             Length = new TimeSpan(0, 22, 0)
                                                                                         });
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 1, 2, "Top Banana") {
                                                                                                  AirDate = new DateTime(2003, 11, 9),
                                                                                                  Length = new TimeSpan(0, 22, 0)
                                                                                              });
                results.Last().Episodes.Add(new TvEpisode(results.Last(), 1, 3, "Bringing Up Buster") {
                                                                                                          AirDate = new DateTime(2003, 11, 16),
                                                                                                          Length = new TimeSpan(0, 22, 0)
                                                                                                      });
                return results;
            }
        }
    }

    public class TvSeries {
        private readonly List<TvEpisode> m_Episodes = new List<TvEpisode>();
        private readonly string m_Name;
        private string m_SeriesBannerPath;

        private string m_SeriesPosterPath;

        public TvSeries(string seriesName) {
            m_Name = seriesName;
        }

        public string Name {
            get { return m_Name; }
        }

        public IList<TvEpisode> Episodes {
            get { return m_Episodes; }
        }

        public string SeriesBannerPath {
            get { return m_SeriesBannerPath; }
            set { m_SeriesBannerPath = value; }
        }

        public string SeriesPosterPath {
            get { return m_SeriesPosterPath; }
            set { m_SeriesPosterPath = value; }
        }

        public bool HasPoster {
            get { return SeriesBannerPath != null; }
        }
    }

    public class TvEpisode {
        private readonly int m_EpisodeNumber;
        private readonly int m_SeasonNumber;
        private readonly TvSeries m_Series;
        private readonly string m_Title;
        private DateTimeOffset m_AirDate;
        private bool m_HasComputerVersion;

        private bool m_HasIphoneVersion;
        private bool m_HasIpodVersion;
        private TimeSpan m_Length;

        public TvEpisode(TvSeries series, int seasonNumber, int episodeNumber, string title) {
            m_Series = series;
            m_SeasonNumber = seasonNumber;
            m_EpisodeNumber = episodeNumber;
            m_Title = title;
        }

        public TvSeries Series {
            get { return m_Series; }
        }

        public int SeasonNumber {
            get { return m_SeasonNumber; }
        }

        public int EpisodeNumber {
            get { return m_EpisodeNumber; }
        }

        public string Title {
            get { return m_Title; }
        }

        public DateTimeOffset AirDate {
            get { return m_AirDate; }
            set { m_AirDate = value; }
        }

        public TimeSpan Length {
            get { return m_Length; }
            set { m_Length = value; }
        }

        public string LongName {
            get {
                if (SeasonNumber == 0)
                    return string.Format("{1:00} - {2}", EpisodeNumber, Title);
                else
                    return string.Format("{0:00}x{1:00} - {2}", SeasonNumber, EpisodeNumber, Title);
            }
        }

        public bool IsEven {
            get { return EpisodeNumber % 2 == 0; }
        }

        public bool HasComputerVersion {
            get { return m_HasComputerVersion; }
            set { m_HasComputerVersion = value; }
        }

        public bool HasIpodVersion {
            get { return m_HasIpodVersion; }
            set { m_HasIpodVersion = value; }
        }

        public bool HasIphoneVersion {
            get { return m_HasIphoneVersion; }
            set { m_HasIphoneVersion = value; }
        }
    }

    [ValueConversion(typeof (string), typeof (ImageSource))]
    public class UriImageConverter : IValueConverter {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) {
            if (value == null)
                return null;
            else {
                try {
                    return new BitmapImage(new Uri((string) value), new RequestCachePolicy(RequestCacheLevel.Default));
                } catch (WebException wex) {
                    return null;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) {
            throw new NotImplementedException();
        }

        #endregion
    }
}