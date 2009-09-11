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
namespace MetaGeta.DataStore {
    public class TVShowDataStoreTemplate : IDataStoreTemplate {
        public const string iPhoneCompatible = "iPhoneCompatible";

        #region IDataStoreTemplate Members

        public string[] GetDimensionNames() {
            return new[] {
                             SeriesTitle,
                             EpisodeTitle,
                             EpisodeNumber,
                             SeasonNumber,
                             PartNumber,
                             SeriesID,
                             EpisodeID,
                             SeriesDescription,
                             EpisodeDescription,
                             EpisodeBanner,
                             CRC32,
                             Group,
                             AudioCodec,
                             VideoCodec,
                             Resolution,
                             PlayTime
                         };
        }

        public string[] GetColumnNames() {
            return new[] {
                             SeriesTitle,
                             EpisodeTitle,
                             EpisodeNumber,
                             SeasonNumber,
                             PartNumber,
                             CRC32,
                             Group,
                             AudioCodec,
                             VideoCodec,
                             Resolution,
                             PlayTime
                         };
        }

        public string[] GetPluginTypeNames() {
            return new[] {
                             "MetaGeta.DataStore.FileInfoTaggingPlugin, MetaGeta.DataStore",
                             "MetaGeta.TVShowPlugin.EducatedGuessImporter, TVShowPlugin",
                             "MetaGeta.DirectoryFileSourcePlugin.DirectoryFileSourcePlugin, DirectoryFileSourcePlugin",
                             "MetaGeta.MediaInfoPlugin.MediaInfoPlugin, MediaInfoPlugin",
                             "MetaGeta.TVDBPlugin.TVDBPlugin, TVDBPlugin",
                             "TranscodePlugin.TranscodePlugin, TranscodePlugin",
                             "TranscodePlugin.Mp4TagWriterPlugin, TranscodePlugin"
                         };
        }

        public string GetName() {
            return "TVShow";
        }

        #endregion

        #region "Constants"

        public const string AudioCodec = "AudioCodec";
        public const string CRC32 = "CRC32";
        public const string EpisodeBanner = "EpisodeBanner";

        public const string EpisodeDescription = "EpisodeDescription";
        public const string EpisodeFirstAired = "EpisodeFirstAired";
        public const string EpisodeID = "EpisodeID";
        public const string EpisodeNumber = "EpisodeNumber";
        public const string EpisodeTitle = "EpisodeTitle";

        public const string Format = "Format";
        public const string FrameCount = "FrameCount";
        public const string FrameRate = "FrameRate";
        public const string Group = "Group";
        public const string iPod5GCompatible = "iPod5GCompatible";
        public const string iPodClassicCompatible = "iPodClassicCompatible";
        public const string PartNumber = "PartNumber";
        public const string PlayTime = "PlayTime";
        public const string Resolution = "Resolution";
        public const string SeasonNumber = "SeasonNumber";
        public const string SeriesBanner = "SeriesBanner";
        public const string SeriesDescription = "SeriesDescription";
        public const string SeriesID = "SeriesID";
        public const string SeriesPoster = "SeriesPoster";
        public const string SeriesTitle = "SeriesTitle";
        public const string VideoCodec = "VideoCodec";

        public const string VideoCodecProfile = "VideoCodecProfile";
        public const string VideoDisplayAspectRatio = "VideoDisplayAspectRatio";
        public const string VideoHeightPx = "VideoHeightPx";
        public const string VideoWidthPx = "VideoWidthPx";

        #endregion
    }
}