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

            file.Tags.Set(TVShowDataStoreTemplate.Format, fileInfo.Format);

            if (fileInfo.AudioStreams.Any()) {
                AudioStreamInfo audio = fileInfo.AudioStreams.First();
                file.Tags.Set(TVShowDataStoreTemplate.AudioCodec, audio.CodecString);
            }

            if (fileInfo.VideoStreams.Any()) {
                VideoStreamInfo video = fileInfo.VideoStreams.First();
                file.Tags.Set(TVShowDataStoreTemplate.VideoCodec, video.CodecString);
                file.Tags.Set(TVShowDataStoreTemplate.VideoCodecProfile, video.CodecProfile);
                file.Tags.Set(TVShowDataStoreTemplate.Resolution, string.Format("{0}x{1}", video.WidthPx, video.HeightPx));
                file.Tags.Set(TVShowDataStoreTemplate.VideoWidthPx, video.WidthPx);
                file.Tags.Set(TVShowDataStoreTemplate.VideoHeightPx, video.HeightPx);
                file.Tags.Set(TVShowDataStoreTemplate.PlayTime, video.PlayTime);
                file.Tags.Set(TVShowDataStoreTemplate.VideoDisplayAspectRatio, video.DisplayAspectRatio);
                file.Tags.Set(TVShowDataStoreTemplate.FrameCount, video.FrameCount);
                file.Tags.Set(TVShowDataStoreTemplate.FrameRate, video.FrameRate);
            }

            file.Tags.Set(TVShowDataStoreTemplate.iPod5GCompatible, fileInfo.IsCompatible(DeviceType.iPod5G));
            file.Tags.Set(TVShowDataStoreTemplate.iPodClassicCompatible, fileInfo.IsCompatible(DeviceType.iPodClassic));
            file.Tags.Set(TVShowDataStoreTemplate.iPhoneCompatible, fileInfo.IsCompatible(DeviceType.iPhone));
        }

        #endregion
    }
}