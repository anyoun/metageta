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
using System.Collections;
using System.Collections.Generic;

#endregion

namespace MetaGeta.DataStore {
    [Serializable]
    public class FileSet : IEnumerable<MGFile> {
        protected List<MGFile> m_Items;

        public FileSet() {
            m_Items = new List<MGFile>();
        }

        public FileSet(IEnumerable<MGFile> c) {
            m_Items = new List<MGFile>(c);
            m_Items.Sort(new MGFileIDComparer());
        }

        #region IEnumerable<MGFile> Members

        public IEnumerator<MGFile> GetEnumerator() {
            return m_Items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator1();
        }

        #endregion

        public void Add(MGFile file) {
            m_Items.Add(file);
            m_Items.Sort(new MGFileIDComparer());
        }

        public IEnumerable<MGTag> GetAllTags(string tagName) {
            throw new NotImplementedException();
            //Dim tags As New List(Of MGTag)
            //For Each file As MGFile In m_Items
            //    tags.Add(file.Tags.Item(tagName))
            //Next
            //Return From t In tags Group By t.Value Into tg = First() Select tg
        }

        public IEnumerator GetEnumerator1() {
            return m_Items.GetEnumerator();
        }

        public static FileSet Intesect(FileSet left, FileSet right) {
            var common = new FileSet();

            int l = 0;
            int r = 0;
            while ((l < left.m_Items.Count && r < right.m_Items.Count)) {
                if (left.m_Items[l].ID == right.m_Items[r].ID) {
                    common.m_Items.Add(left.m_Items[l]);
                    l += 1;
                    l += 1;
                } else if (left.m_Items[l].ID.CompareTo(right.m_Items[r].ID) < 0)
                    l += 1;
                else
                    r += 1;
            }

            return common;
        }
    }
}