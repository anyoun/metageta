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

#endregion

namespace MetaGeta.DataStore {
    [Serializable]
    public class MGTag : IEquatable<MGTag> {
        private readonly string m_Name;

        private readonly string m_Value;

        public MGTag(string name, string value) {
            m_Name = name;
            m_Value = value;
        }

        public string Name {
            get { return m_Name; }
        }

        public string Value {
            get { return m_Value; }
        }

        public bool IsSet {
            get { return m_Value != null; }
        }

        #region IEquatable<MGTag> Members

        public bool Equals(MGTag other) {
            return other.Name == Name && other.Value == Value;
        }

        #endregion
    }
}