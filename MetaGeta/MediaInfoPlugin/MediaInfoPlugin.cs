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
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using log4net;
using MediaInfoLib;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.MediaInfoPlugin {
    public class MediaInfoPlugin : IMGTaggingPlugin, IMGPluginBase {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private MGDataStore m_DataStore;

        private long m_ID;
        private MediaInfoWrapper m_MediaInfo;

        public long ID {
            get { return m_ID; }
        }

        #region IMGPluginBase Members

        long IMGPluginBase.PluginID {
            get { return ID; }
        }

        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
            m_MediaInfo = new MediaInfoWrapper();
        }

        public string FriendlyName {
            get { return "MediaInfo Plugin"; }
        }

        public string UniqueName {
            get { return "MediaInfoPlugin"; }
        }

        public Version Version {
            get { return new Version(1, 0, 0, 0); }
        }


        public void Shutdown() {
            m_DataStore = null;
            m_MediaInfo = null;
        }

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        #region IMGTaggingPlugin Members

        public void Process(MGFile file, ProgressStatus reporter) {
            FileMetadata fileInfo = m_MediaInfo.ReadFile(file.FileName);

            file.SetTag(TVShowDataStoreTemplate.Format, fileInfo.Format);

            if (fileInfo.AudioStreams.Any()) {
                AudioStreamInfo audio = fileInfo.AudioStreams.First();
                file.SetTag(TVShowDataStoreTemplate.AudioCodec, audio.CodecString);
            }

            if (fileInfo.VideoStreams.Any()) {
                VideoStreamInfo video = fileInfo.VideoStreams.First();
                file.SetTag(TVShowDataStoreTemplate.VideoCodec, video.CodecString);
                file.SetTag(TVShowDataStoreTemplate.VideoCodecProfile, video.CodecProfile);
                file.SetTag(TVShowDataStoreTemplate.Resolution, string.Format("{0}x{1}", video.WidthPx, video.HeightPx));
                file.SetTag(TVShowDataStoreTemplate.VideoWidthPx, video.WidthPx.ToString());
                file.SetTag(TVShowDataStoreTemplate.VideoHeightPx, video.HeightPx.ToString());
                file.SetTag(TVShowDataStoreTemplate.PlayTime, video.PlayTime.ToString());
                file.SetTag(TVShowDataStoreTemplate.VideoDisplayAspectRatio, video.DisplayAspectRatio.ToString());
                file.SetTag(TVShowDataStoreTemplate.FrameCount, video.FrameCount.ToString());
                file.SetTag(TVShowDataStoreTemplate.FrameRate, video.FrameRate.ToString());
            }

            file.SetTag(TVShowDataStoreTemplate.iPod5GCompatible, fileInfo.IsCompatible(DeviceType.iPod5G).ToString());
            file.SetTag(TVShowDataStoreTemplate.iPodClassicCompatible, fileInfo.IsCompatible(DeviceType.iPodClassic).ToString());
            file.SetTag(TVShowDataStoreTemplate.iPhoneCompatible, fileInfo.IsCompatible(DeviceType.iPhone).ToString());
        }

        #endregion
    }
}