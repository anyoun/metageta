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
using NUnit.Framework.SyntaxHelpers;

#endregion

namespace MetaGeta.TVShowPlugin {
    [TestFixture]
    public class TvShowPluginUnitTests : AssertionHelper {
        private void Expect(string fileName,
                            string seriesTitle,
                            string seasonNumber,
                            string episodeNumber,
                            string episodeTitle) {
            var f = new MGFile();
            f.SetTag(MGFile.FileNameKey, new Uri("C:\\" + fileName + ".avi").AbsoluteUri);
            var edu = new EducatedGuessImporter();
            edu.Process(f, new ProgressStatus());
            if (seriesTitle != null)
                Expect(f.GetTag(TVShowDataStoreTemplate.SeriesTitle), Is.EqualTo(seriesTitle));
            if (seasonNumber != null)
                Expect(f.GetTag(TVShowDataStoreTemplate.SeasonNumber), Is.EqualTo(seasonNumber));
            if (episodeNumber != null)
                Expect(f.GetTag(TVShowDataStoreTemplate.EpisodeNumber), Is.EqualTo(episodeNumber));
            if (episodeTitle != null)
                Expect(f.GetTag(TVShowDataStoreTemplate.EpisodeTitle), Is.EqualTo(episodeTitle));
        }

        [Test]
        public void BasicFileNameParsing() {
            Expect("Series Name - 1", "Series Name", "1", "1", "");
            Expect("Series Name 1", "Series Name", "1", "1", "");
            Expect("Series Name 2x1", "Series Name", "2", "1", "");
            Expect("Series Name s2e1", "Series Name", "2", "1", "");
            Expect("Series Name 201", "Series Name", "2", "1", "");
            Expect("Series Name 1 v3", "Series Name", "1", "1", "");
            Expect("Series.Name.4.3", "Series Name", "4", "3", "");
            Expect("Series.Name.5.37.This.Is.The.Title", "Series Name", "5", "37", "This Is The Title");
        }
    }
}