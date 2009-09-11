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

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.Browser {
    public class FileView : IEnumerable<MGFile> {
        private readonly IEnumerable<MGFile> m_Files;

        internal FileView(IEnumerable<MGFile> files) {
            m_Files = files.OrderBy(f => f.FileName);
        }

        #region IEnumerable<MGFile> Members

        public IEnumerator<MGFile> GetEnumerator() {
            return m_Files.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator1();
        }

        #endregion

        public IEnumerator GetEnumerator1() {
            return GetEnumerator();
        }
    }
}