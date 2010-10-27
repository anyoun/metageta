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
using System.Data.Common;
using System.Data.SQLite;
using MetaGeta.Utilities;
using NUnit.Framework;

#endregion

namespace MetaGeta.DataStore.DataStore {
    [TestFixture]
    public class SQLiteDataTypeTest : AssertionHelper {
        #region Setup/Teardown

        [SetUp]
        public void InializeConnection() {
			NativeHelper.ExtractSqlite();
			
			Console.WriteLine(Environment.CurrentDirectory);
            m_Connection = new SQLiteConnection("Data Source=:memory:");
            m_Connection.Open();

            DbCommand cmd = m_Connection.CreateCommand();
            cmd.CommandText = "CREATE TABLE foo(bar BLOB)";
            cmd.ExecuteNonQuery();
        }

        [TearDown]
		public void CloseConnection() {
            m_Connection.Close();
        }

        #endregion

        private DbConnection m_Connection;

        private List<object> GetColumnFromTable() {
            DbCommand cmd = m_Connection.CreateCommand();
            cmd.CommandText = "SELECT * FROM foo";
            var items = new List<object>();
            using (DbDataReader rdr = cmd.ExecuteReader()) while (rdr.Read()) items.Add(rdr.GetString(0));
            return items;
        }

        [Test]
        public void String() {
            InializeConnection();

            DbCommand cmd = m_Connection.CreateCommand();
            cmd.CommandText = "INSERT INTO foo(bar) VALUES (?)";

            cmd.AddParam("foobar");

            cmd.Parameters[0].Value = "foobar";
            cmd.ExecuteNonQuery();
            cmd.Parameters[0].Value = "123";
            cmd.ExecuteNonQuery();

            Expect(GetColumnFromTable(), Is.EquivalentTo(new[] {"foobar", "123"}));
        }
    }
}