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
using System.IO;

#endregion

namespace MetaGeta.DataStore {
    public class FileInfoTaggingPlugin : IMGTaggingPlugin, IMGPluginBase {
        private MGDataStore m_DataStore;
        private long m_ID;

        #region IMGPluginBase Members

        public void Startup(MGDataStore dataStore, long id) {
            m_DataStore = dataStore;
            m_ID = id;
        }


        public void Shutdown() {}

        public string FriendlyName {
            get { return "FileInfoTaggingPlugin"; }
        }

        public string UniqueName {
            get { return "FileInfoTaggingPlugin"; }
        }

        public Version Version {
            get { return new Version(1, 0, 0, 0); }
        }

        public long PluginID {
            get { return m_ID; }
        }

        public event PropertyChangedEventHandler SettingChanged;

        #endregion

        #region IMGTaggingPlugin Members

        public void Process(MGFile file, ProgressStatus reporter) {
            var fileInfo = new FileInfo(file.FileName);
            file.SetTag("LastWriteTime", fileInfo.LastWriteTime.ToString());
            file.SetTag("Extension", fileInfo.Extension);
            file.SetTag("FileSizeBytes", fileInfo.Length.ToString());
        }

        #endregion
    }
}