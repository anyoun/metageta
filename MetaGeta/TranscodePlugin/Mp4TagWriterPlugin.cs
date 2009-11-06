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

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.SeriesTitle))
                WriteAtomicParsleyTag(file.FileName, "TVShowName", file.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle));
            progress.ProgressPct = 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.SeriesTitle))
                WriteAtomicParsleyTag(file.FileName, "album", file.Tags.GetString(TVShowDataStoreTemplate.SeriesTitle));
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeFirstAired))
                WriteAtomicParsleyTag(file.FileName, "year", file.Tags.GetDateTime(TVShowDataStoreTemplate.EpisodeFirstAired).Value.ToUniversalTime().ToString("u"));
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.SeasonNumber))
                WriteAtomicParsleyTag(file.FileName, "TVSeasonNum", file.Tags.GetInt(TVShowDataStoreTemplate.SeasonNumber).ToString());
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeID))
                WriteAtomicParsleyTag(file.FileName, "TVEpisode", file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeID).ToString());
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeNumber))
                WriteAtomicParsleyTag(file.FileName, "TVEpisodeNum", file.Tags.GetInt(TVShowDataStoreTemplate.EpisodeNumber).ToString());
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeDescription))
                WriteAtomicParsleyTag(file.FileName, "description", file.Tags.GetString(TVShowDataStoreTemplate.EpisodeDescription));

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeBanner))
                WriteAtomicParsleyTag(file.FileName, "artwork", file.Tags.GetString(TVShowDataStoreTemplate.EpisodeBanner));
            progress.ProgressPct += 0.1;

            WriteAtomicParsleyTag(file.FileName, "stik", "TV Show");
            progress.ProgressPct += 0.1;

            WriteAtomicParsleyTag(file.FileName, "genre", "TV Shows");
            progress.ProgressPct += 0.1;

            if (file.Tags.ContainsKey(TVShowDataStoreTemplate.EpisodeTitle))
                WriteAtomicParsleyTag(file.FileName, "title", file.Tags.GetString(TVShowDataStoreTemplate.EpisodeTitle));
            progress.ProgressPct += 0.1;
        }

        #endregion

        #region IMGPluginBase Members

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        private void WriteAtomicParsleyTag(string path, string apTagName, string value) {
            if (value == null) {
                log.WarnFormat("Can't write tag \"{0}\" for file \"{1}\" since the value is null.", apTagName, path);
                return;
            }

            var p = new Process();
            p.StartInfo = new ProcessStartInfo(AtomicParsleyLocation);
            var sb = new StringBuilder();

            sb.AppendFormat(" \"{0}\" ", path);
            sb.AppendFormat(" --{0} \"{1}\" ", apTagName, CapLength(value).Replace("\"", "\"\""));
            sb.Append(" --overWrite ");
            p.StartInfo.Arguments = sb.ToString();
            log.DebugFormat("{0} {1}", p.StartInfo.FileName, p.StartInfo.Arguments);
            p.StartInfo.RedirectStandardError = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            p.OutputDataReceived += OutputHandler;
            p.ErrorDataReceived += OutputHandler;
            p.Start();
            p.BeginErrorReadLine();
            p.BeginOutputReadLine();
            p.WaitForExit();
            if (p.ExitCode != 0)
                throw new Exception(string.Format("AtomicParsley failed with exit code: {0}", p.ExitCode));
        }

        private string CapLength(string s) {
            return s.Substring(0, Math.Min(255, s.Length));
        }


        private void OutputHandler(object sender, DataReceivedEventArgs e) {
            log.Debug(e.Data);
        }

        [Settings("AtomicParsley Location", "AtomicParsley.exe", SettingType.File, "Programs")]
        public string AtomicParsleyLocation {
            get { return m_AtomicParsleyLocation; }
            set {
                if (value != m_AtomicParsleyLocation) {
                    m_AtomicParsleyLocation = value;
                    if (SettingChanged != null)
                        SettingChanged(this, new PropertyChangedEventArgs("AtomicParsleyLocation"));
                }
            }
        }
    }
}