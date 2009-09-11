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

namespace MetaGeta.Browser
{
    public class TagView : IEnumerable<MGTag>
    {
        private readonly IEnumerable<MGTag> m_Tags;

        internal TagView(IEnumerable<MGTag> tags)
        {
            m_Tags = tags.OrderBy(t => t.Value);
        }

        #region IEnumerable<MGTag> Members

        public IEnumerator<MGTag> GetEnumerator()
        {
            return m_Tags.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator1();
        }

        #endregion

        public IEnumerator GetEnumerator1()
        {
            return GetEnumerator();
        }
    }
}