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

#endregion

namespace MetaGeta.DataStore {
    [Serializable]
    public class MGFile : IEquatable<MGFile> {
        private readonly MGDataStore m_DataStore;

        private readonly MGTagCollection m_Tags = new MGTagCollection();
        private long m_ID = -1;

        internal MGFile(MGDataStore datastore) {
            m_DataStore = datastore;
        }

        internal MGFile(long id, MGDataStore datastore) {
            m_DataStore = datastore;
            m_ID = id;
        }

        public MGFile() : this(null) {}

        public long ID {
            get { return m_ID; }
            set {
                if (m_ID != value)
                    m_ID = value;
            }
        }

        public string FileName {
            get {
                string fn = GetTag(FileNameKey);
                if (fn == null)
                    throw new Exception("Can't find filename for file.");
                return new Uri(fn).LocalPath;
            }
        }

        public MGTagCollection Tags {
            get { return m_Tags; }
        }

        #region "Constants"

        public static string FileNameKey {
            get { return "FileName"; }
        }

        public static string TimeStampKey {
            get { return "TimeStamp"; }
        }

        #endregion

        #region IEquatable<MGFile> Members

        public bool Equals(MGFile other) {
            return other.ID == ID;
        }

        #endregion

        public string GetTag(string tagName) {
            return m_Tags.GetValue(tagName);
        }

        public void SetTag(string tagName, string tagValue) {
            m_Tags.SetValue(tagName, tagValue);
        }
    }
}

namespace MetaGeta.DataStore {
    public class MGFileIDComparer : IComparer<MGFile> {
        #region IComparer<MGFile> Members

        public int Compare(MGFile x, MGFile y) {
            return x.ID.CompareTo(y.ID);
        }

        #endregion
    }
}

namespace MetaGeta.DataStore {
    public class MGFileEventArgs : EventArgs {
        private readonly MGFile m_File;

        public MGFileEventArgs(MGFile f) {
            m_File = f;
        }

        public MGFile File {
            get { return m_File; }
        }
    }
}