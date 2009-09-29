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
using NUnit.Framework;

#endregion

namespace MetaGeta.DataStore.Model {
    [TestFixture]
    public class TagCollectionTest : AssertionHelper {
        [Test]
        public void Basic() {
            var tags = new MGTagCollection();
            tags.Add("foo", 123);
            tags.Add("foo", 456);
            tags.Add("bar", "asdf");
            tags.Add("bar", "qwer");
            Assert.That(() => tags.Add("foo", "789"), Throws.TypeOf<ArgumentException>());
            Assert.That(() => tags.Add("bar", 123), Throws.TypeOf<ArgumentException>());
            tags.Add("foo", 987);
            tags.Add("bar", "poiu");
        }
    }
}