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
using System.Linq;
using MetaGeta.DataStore;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.Browser {
    public class Browser {
        private readonly FileSet m_CurrentFiles;
        private readonly List<MGTag> m_CurrentLocation = new List<MGTag>();
        private readonly List<string> m_DimensionPath;

        private MGDataStore m_DataStore;

        internal Browser(MGDataStore ds, string path) {
            m_DataStore = ds;
            m_DimensionPath = new List<string>(path.Split('/'));
            UpdateCurrent();
        }

        public bool CanPush {
            get { return m_CurrentLocation.Count == m_DimensionPath.Count; }
        }

        public bool CanPop {
            get { return m_CurrentLocation.Count != 0; }
        }

        public string CurrentLocation {
            get { return (from t in m_CurrentLocation select t.Value).JoinToString(" } "); }
        }

        private string NextTagName {
            get { return m_DimensionPath[m_CurrentLocation.Count]; }
        }

        public TagView GetCurrentView() {
            return new TagView(m_CurrentFiles.GetAllTags(NextTagName));
        }

        public FileView GetFileView() {
            return new FileView(m_CurrentFiles);
        }

        public void Pop() {
            m_CurrentLocation.RemoveAt(m_CurrentLocation.Count - 1);
            UpdateCurrent();
        }

        public void Push(MGTag tag) {
            if (m_CurrentLocation.Count >= m_DimensionPath.Count)
                throw new Exception();
            if (NextTagName != tag.Name)
                throw new Exception();

            m_CurrentLocation.Add(tag);
            UpdateCurrent();
        }


        private void UpdateCurrent() {
            //m_CurrentFiles = m_DataStore.GetAllFiles()
            //For Each tag As MGTag In m_CurrentLocation
            //    If (m_CurrentFiles Is Nothing) Then
            //        m_CurrentFiles = m_DataStore.GetFilesWhere(tag)
            //    Else
            //        m_CurrentFiles = FileSet.Intesect(m_CurrentFiles, m_DataStore.GetFilesWhere(tag))
            //    End If
            //Next
        }
    }
}