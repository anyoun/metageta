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
using System.Data.SQLite;
using System.Linq;
using MetaGeta.Utilities;
using NUnit.Framework;

#endregion

namespace MetaGeta.DataStore.Database {
    [TestFixture]
    public class TagCollectionPersistenceTest : AssertionHelper {
		[TestFixtureSetUp]
		public void Setup() {
			NativeHelper.ExtractSqlite();
		}

        private MGTagCollection CreateTags() {
            var tags = new MGTagCollection();
            tags.Add("foo", 123);
            tags.Add("bar", "qwer");
            tags.Add("foo", 987);
            tags.Add("zxcv", new byte[] {1, 2, 3, 4});
            tags.Add("bar", "987");
            tags.Add("bar", "True");

            tags.Add("baz", DateTimeOffset.Now);
            tags.Add("baz2", DateTimeOffset.MinValue);
            tags.Add("baz3", DateTimeOffset.MaxValue);
            tags.Add("baz4", new DateTimeOffset(1300, 4, 14, 2, 5, 48, TimeSpan.Zero));
            tags.Add("baz5", new DateTimeOffset(2135, 6, 1, 17, 0, 10, TimeSpan.Zero));
            tags.Add("blee", 123.456);
            tags.Add("bar", "poiu");
            tags.Add("foo", 456);
            tags.Add("asdf", false);
            tags.Add("asdf", true);
            tags.Add("qwerty", TimeSpan.FromSeconds(10));
            tags.Add("bar", "asdf");
            return tags;
        }

        [Test]
        public void SqliteTest() {
            MGTagCollection originalTags = CreateTags();

            var bdr = new SQLiteConnectionStringBuilder();
            bdr.DataSource = ":memory:";
            using (var conn = new SQLiteConnection(bdr.ConnectionString)) {
                conn.Open();

                SQLiteCommand cmd = conn.CreateCommand();
                cmd.CommandText = "CREATE TABLE foo(Key integer, Name varchar, Value NONE, Type varchar)";
                cmd.ExecuteNonQuery();

                cmd = conn.CreateCommand();
                cmd.CommandText = "INSERT INTO foo(Key, Name, Value, Type) VALUES (@Key, @Name, @Value, @Type)";
                var persister = new TagCollectionPersister {
                                                               KeyColumn = "Key",
                                                               TagNameColumn = "Name",
                                                               TagValueColumn = "Value",
                                                               TagTypeColumn = "Type",
                                                           };
                cmd.SetParam("@Key", 1);
                persister.WriteTags(originalTags, cmd);

                cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT Key, Name, Value, Type FROM foo";
                using (SQLiteDataReader rdr = cmd.ExecuteReader()) {
                    Dictionary<long, MGTagCollection> tagDictionary = persister.ReadTags<long>(rdr);
                    Expect(tagDictionary.Count, Is.EqualTo(1));
                    Expect(tagDictionary.Values.First().All.ToArray(),
                           Is.EquivalentTo(originalTags.All.ToArray()).Using<MGTag>(EqualityComparer<MGTag>.Default));
                }
            }
        }
    }
}