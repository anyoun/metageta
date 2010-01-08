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
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using log4net;
using MetaGeta.DataStore;
using MetaGeta.Utilities;
using TvdbLib;
using TvdbLib.Cache;
using TvdbLib.Data;

#endregion

namespace MetaGeta.TVDBPlugin {
    public class TVDBPlugin : IMGTaggingPlugin, IMGPluginBase {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<string, int?> m_SeriesNameDictionary = new Dictionary<string, int?>();
        private MGDataStore m_DataStore;

        private long m_ID;
        private TvdbHandler m_tvdbHandler = null;
        private DateTimeOffset m_LastUpdateTime;


        #region IMGPluginBase Members

        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
        }

        private void LazyInitialization() {
            //Initialize once per instance here, not Startup() to avoid slowing down application startup.
            if (m_tvdbHandler != null)
                return;

            m_tvdbHandler = new TvdbHandler(new XmlCacheProvider("C:\\temp\\tvdbcache"), "BC8024C516DFDA3B");
            m_tvdbHandler.InitCache();

            if (DateTimeOffset.Now - m_LastUpdateTime > TimeSpan.FromDays(1)) {
                log.InfoFormat("Updating all series since it's been {0} since the last update.", DateTimeOffset.Now - m_LastUpdateTime);
                m_tvdbHandler.UpdateAllSeries(TvdbHandler.Interval.automatic, true);
                LastUpdateTime = DateTimeOffset.Now;
                log.DebugFormat("Update OK.");
            } else {
                log.InfoFormat("Not updating since it's only been {0} since the last update.", DateTimeOffset.Now - m_LastUpdateTime);
            }
        }

        public void Shutdown() {
            if (m_tvdbHandler != null)
                m_tvdbHandler.CloseCache();
        }

        public long ID { get { return m_ID; } }
        long IMGPluginBase.PluginID { get { return ID; } }
        public string FriendlyName { get { return "The TVDB Plugin"; } }
        public string UniqueName { get { return "TVDBPlugin"; } }
        public Version Version { get { return new Version(1, 0, 0, 0); } }

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        #region IMGTaggingPlugin Members

        public void Process(MGFile file, ProgressStatus reporter) {
            LazyInitialization();

            //prompt user for series name lookups??
            int? seriesID = GetSeriesID(file);
            if (seriesID == null)
                return;
            TvdbSeries series = GetSeries(seriesID.Value);

            file.Tags.Set(TVShowDataStoreTemplate.SeriesDescription, series.Overview);

            file.Tags.Set(TVShowDataStoreTemplate.SeriesBanner, LoadBannerToPath(series.SeriesBanners.FirstOrDefault()));
            file.Tags.Set(TVShowDataStoreTemplate.SeriesPoster, LoadBannerToPath(series.PosterBanners.FirstOrDefault()));

            TvdbEpisode exactEpisode = GetEpisode(series, file);

            if (exactEpisode != null) {
                file.Tags.Set(TVShowDataStoreTemplate.EpisodeTitle, exactEpisode.EpisodeName);
                file.Tags.Set(TVShowDataStoreTemplate.EpisodeDescription, exactEpisode.Overview);
                file.Tags.Set(TVShowDataStoreTemplate.EpisodeID, exactEpisode.Id);
                file.Tags.Set(TVShowDataStoreTemplate.EpisodeFirstAired, exactEpisode.FirstAired);

                file.Tags.Set(TVShowDataStoreTemplate.EpisodeBanner, LoadBannerToPath(exactEpisode.Banner));
            }
        }

        #endregion

        private const string c_DefaultDate = "0001-01-01T00:00:00.0000000+00:00";
        [Settings("Last Update Time", c_DefaultDate, SettingType.ShortText, "Updates")]
        public string LastUpdateTimeString {
            get { return m_LastUpdateTime.ToString("o"); }
            set {
                if (value != LastUpdateTimeString) {
                    m_LastUpdateTime = DateTimeOffset.ParseExact(value ?? c_DefaultDate, "o", null, System.Globalization.DateTimeStyles.RoundtripKind);
                    if (SettingChanged != null) {
                        SettingChanged(this, new PropertyChangedEventArgs("LastUpdateTimeString"));
                        SettingChanged(this, new PropertyChangedEventArgs("LastUpdateTime"));
                    }
                }
            }
        }

        public DateTimeOffset LastUpdateTime {
            get { return m_LastUpdateTime; }
            set {
                if (value != m_LastUpdateTime) {
                    m_LastUpdateTime = value;
                    if (SettingChanged != null) {
                        SettingChanged(this, new PropertyChangedEventArgs("LastUpdateTimeString"));
                        SettingChanged(this, new PropertyChangedEventArgs("LastUpdateTime"));
                    }
                }
            }
        }

        private string LoadBannerToPath(TvdbBanner banner) {
            if (banner == null)
                return null;

            string imageLocalPath = Path.Combine(m_DataStore.GetImageDirectory(), banner.Id.ToString());
            imageLocalPath = Path.ChangeExtension(imageLocalPath, "jpg");
            if (!File.Exists(imageLocalPath)) {
                log.DebugFormat("Downloading banner \"{0}\" to \"{1}\"", banner.BannerUri.AbsoluteUri, imageLocalPath);
                using (var wc = new WebClient()) {
                    try {
                        wc.DownloadFile(banner.BannerUri, imageLocalPath);
                    } catch (Exception ex) {
                        log.Warn("Loading banner failed.", ex);
                        return null;
                    }
                    log.DebugFormat("OK");
                }
            }

            return imageLocalPath;
        }

        private int? GetSeriesID(MGFile file) {
            if (file.Tags.GetInt(TVShowDataStoreTemplate.SeriesID).HasValue)
                return file.Tags.GetInt(TVShowDataStoreTemplate.SeriesID);

            string seriesName = file.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle);

            if (seriesName == null)
                return null;

            int? seriesID = default(int?);

            if (!m_SeriesNameDictionary.TryGetValue(seriesName, out seriesID)) {
                List<TvdbSearchResult> searchResult = m_tvdbHandler.SearchSeries(seriesName);
                if (searchResult.Count == 0) {
                    log.DebugFormat("Couldn't find series: \"{0}\".", seriesName);
                    seriesID = null;
                } else if (searchResult.Count == 1) {
                    log.DebugFormat("Found series: \"{0}\" -> \"{1}\".", seriesName, searchResult.Single().SeriesName);
                    seriesID = searchResult.Single().Id;
                } else {
                    log.DebugFormat("Multiple results for \"{0}\": {1}", seriesName, searchResult.Select(s => s.SeriesName).JoinToString(", "));
                    TvdbSearchResult exactMatch = searchResult.FirstOrDefault(s => string.Equals(s.SeriesName, seriesName, StringComparison.CurrentCultureIgnoreCase));
                    if (((exactMatch != null))) {
                        seriesID = exactMatch.Id;
                        log.Debug("Found an exact match.");
                    } else {
                        seriesID = null;
                        log.Debug("No exact match.");
                    }
                }
                m_SeriesNameDictionary.Add(seriesName, seriesID);
            }
            if (seriesID.HasValue) {
                file.Tags.Set(TVShowDataStoreTemplate.SeriesID, seriesID.Value);
                return seriesID.Value;
            } else
                return null;
        }

        private TvdbSeries GetSeries(int seriesID) {
            log.DebugFormat("Getting series {0}. IsCached: {1}", seriesID, m_tvdbHandler.IsCached(seriesID, TvdbLanguage.DefaultLanguage, true, false, false));
            TvdbSeries s = m_tvdbHandler.GetSeries(seriesID, TvdbLanguage.DefaultLanguage, true, false, false, true);
            log.DebugFormat("{0} is {1}", seriesID, s);
            return s;
        }

        private TvdbEpisode GetEpisode(TvdbSeries series, MGFile file) {
            int? seasonNumber = file.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber);
            int? episodeNumber = file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber);

            if (!seasonNumber.HasValue || !episodeNumber.HasValue) {
                log.DebugFormat("Missing season or episode number for \"{0}\".", file.FileName);
                return null;
            }

            TvdbEpisode ep = null;
            try {
                ep = series.Episodes.Find(episode => episode.EpisodeNumber == episodeNumber && episode.SeasonNumber == seasonNumber);
            } catch (Exception ex) {
                log.WarnFormat("TVDB exception", ex);
            }

            if ((ep != null))
                log.DebugFormat("Found episode: {0} - s{1}e{2} - {3}.", series.SeriesName, seasonNumber, episodeNumber, ep.EpisodeName);
            else
                log.DebugFormat("Couldn't find episode: {0} - s{1}e{2}.", series.SeriesName, seasonNumber, episodeNumber);
            return ep;
        }
    }
}