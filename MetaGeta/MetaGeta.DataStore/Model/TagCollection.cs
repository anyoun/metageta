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
    public class MGTagCollection : IEnumerable<MGTag> {
        private readonly Dictionary<string, MGTag> m_Items = new Dictionary<string, MGTag>();

        public MGTagCollection() {}

        public MGTagCollection(IEnumerable<MGTag> tags) {
            foreach (MGTag tag in tags)
                m_Items.Add(tag.Name, tag);
        }

        public MGTag this[string tagName] {
            get {
                MGTag tag = null;
                if (m_Items.TryGetValue(tagName, out tag))
                    return tag;
                else
                    return null;
            }
        }

        #region IEnumerable<MGTag> Members

        public IEnumerator<MGTag> GetEnumerator() {
            return m_Items.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator1();
        }

        #endregion

        public string GetValue(string tagName) {
            MGTag tag = null;
            if (m_Items.TryGetValue(tagName, out tag))
                return tag.Value;
            else
                return null;
        }

        public void SetValue(string tagName, string value) {
            if (value == null)
                m_Items.Remove(tagName);
            else
                m_Items[tagName] = new MGTag(tagName, value);
        }

        public IEnumerator GetEnumerator1() {
            return GetEnumerator();
        }
    }
}