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
using System.Diagnostics;
using System.Reflection;
using System.Text;
using log4net;
using MetaGeta.DataStore;
using MetaGeta.Utilities;
using TagLib;
using TagLib.Mpeg4;

#endregion

namespace TranscodePlugin {
    public class Mp4TagWriterPlugin : IMGPluginBase, IMGFileActionPlugin {
        public const string c_WriteTagsAction = "WriteTags";
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MGDataStore m_DataStore;
        private string m_AtomicParsleyLocation;

        private long m_Id;

        #region "IMGPluginBase"

        public string FriendlyName {
            get { return "MPEG-4 Tag Writer Plugin"; }
        }

        public string UniqueName {
            get { return "Mp4TagWriterPlugin"; }
        }

        public Version Version {
            get { return new Version(1, 0, 0, 0); }
        }

        public long PluginID {
            get { return m_Id; }
        }

        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
            m_Id = id;
        }


        public void Shutdown() { }

        #endregion

        #region IMGFileActionPlugin Members

        public IEnumerable<string> GetActions() {
            return c_WriteTagsAction.SingleToEnumerable();
        }

        public void DoAction(string action, MGFile file, ProgressStatus progress) {
            if (action != c_WriteTagsAction)
                throw new ArgumentException();

            if (file.Tags.GetString(TVShowDataStoreTemplate.Format) != "MPEG-4") {
                progress.StatusMessage = "Not MPEG-4";
                return;
            }

            WriteMp4TvShowTags(file);
        }

        internal static void WriteMp4TvShowTags(MGFile file) {
            var f = TagLib.File.Create(file.FileName);
            var tags = new AppleTvTag(f);

            if (file.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle) != null)
                tags.SeriesTitle = file.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle);
            if (file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeID).HasValue)
                tags.EpisodeID = file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeID).Value;
            if (file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber).HasValue)
                tags.EpisodeNumber = file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber).Value;
            if (file.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber).HasValue)
                tags.SeasonNumber = file.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber).Value;

            if (file.Tags.GetString(TVShowDataStoreTemplate.EpisodeTitle) != null)
                tags.EpisodeTitle = file.Tags.GetString(TVShowDataStoreTemplate.EpisodeTitle);

            if (file.Tags.GetString(TVShowDataStoreTemplate.EpisodeDescription) != null)
                tags.EpisodeDescription = file.Tags.GetString(TVShowDataStoreTemplate.EpisodeDescription);

            if (file.Tags.GetDateTime(TVShowDataStoreTemplate.EpisodeFirstAired).HasValue)
                tags.FirstAiredDate = file.Tags.GetDateTime(TVShowDataStoreTemplate.EpisodeFirstAired).Value;

            tags.ClearEpisodeImages();
            if (false && file.Tags.GetString(TVShowDataStoreTemplate.EpisodeBanner) != null)
                tags.AddEpisodeImage(file.Tags.GetString(TVShowDataStoreTemplate.EpisodeBanner));
            if (file.Tags.GetString(TVShowDataStoreTemplate.SeriesPoster) != null)
                tags.AddEpisodeImage(file.Tags.GetString(TVShowDataStoreTemplate.SeriesPoster));

            tags.SetAsTvShow();

            f.Save();
        }

        #endregion

        #region IMGPluginBase Members

        public event PropertyChangedEventHandler SettingChanged;

        #endregion
    }
}