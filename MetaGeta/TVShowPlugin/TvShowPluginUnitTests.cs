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
using MetaGeta.DataStore;
using NUnit.Framework;

#endregion

namespace MetaGeta.TVShowPlugin {
    [TestFixture]
    public class TvShowPluginUnitTests : AssertionHelper {
        private void Expect(string fileName,
                            string seriesTitle,
                            int? seasonNumber,
                            int? episodeNumber,
                            string episodeTitle) {
            var f = new MGFile();
            f.Tags.Set(MGFile.FileNameKey, new Uri("C:\\" + fileName + ".avi").AbsoluteUri);
            var edu = new EducatedGuessImporter();
            edu.Process(f, new ProgressStatus());

            try {
                if (seriesTitle != null)
                    Expect(f.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle), Is.EqualTo(seriesTitle));
                if (seasonNumber != null)
                    Expect(f.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber), Is.EqualTo(seasonNumber));
                if (episodeNumber != null)
                    Expect(f.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber), Is.EqualTo(episodeNumber));
                if (episodeTitle != null)
                    Expect(f.Tags.GetString(TVShowDataStoreTemplate.EpisodeTitle), Is.EqualTo(episodeTitle));
            } catch {
                foreach (MGTag tag in f.Tags.All) {
                    Console.WriteLine("{0}: \"{1}\" ({2})", tag.Name, tag.Value, tag.Type);
                }
                throw;
            }
        }

        [Test]
        public void BasicFileNameParsing() {
            Expect("Series Name - 1", "Series Name", 1, 1, null);
            Expect("Series Name 1", "Series Name", 1, 1, null);
            Expect("Series Name 2x1", "Series Name", 2, 1, null);
            Expect("Series Name s2e1", "Series Name", 2, 1, null);
            Expect("Series Name 201", "Series Name", 2, 1, null);
            Expect("Series Name 1 v3", "Series Name", 1, 1, null);
            Expect("Series.Name.4.3", "Series Name", 4, 3, null);
            Expect("Series.Name.5.37.This.Is.The.Title", "Series Name", 5, 37, "This Is The Title");

            Expect("Series.Name.S04E12.720p.HDTV.x264-GROUP", "Series Name", 4, 12, null);
            Expect("Series.Name.S04E12.720p.HDTV.X264-GROUP", "Series Name", 4, 12, null);

            Expect("Series.Name.oav", "Series Name", null, null, null);
            Expect("Battlestar Galactica-s1e2", "Battlestar Galactica (2003)", 1, 2, null);

            //TODO: Fix these cases
            Expect("Series.Name.h264.something.else", "Series Name", null, null, "something else");
            Expect("Series Name [1x02] something else", "Series Name", 1, 2, "something else");
            Expect("30 Something AB S01E04 XviD-QWER", "30 Something AB", 1, 4, null);
            Expect("[A-Group]_Some_Name_-_07v2_[1280x720_H264_AAC_5.1][12345678]", "Some Name", 1, 7, null);
            Expect("[A-Group]_Some_Name_-_07v2_[1280x720_H264_AAC_5.1][12345678]", "Some Name", 1, 7, null);
            Expect("[QWERT-ASDF]_A_Name_-_Sub_Name_-_04_[12345678]", "A Name - Sub Name", 1, 4, null);
            Expect("[Groupname]_Series_Name_-_08[Eng-Rus][1280x720][12345678]", "Series Name", 1, 8, null);
            Expect("[Groupname]_Series_Name_-_09(1280x720)[ABCDEF78]", "Series Name", 1, 9, null);
            Expect("Battlestar Galactica-s1e1.mp4.iphone", "Battlestar Galactica (2003)", 1, 1, null);
        }
    }
}