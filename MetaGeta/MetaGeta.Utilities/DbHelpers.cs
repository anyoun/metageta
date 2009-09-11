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

using System.Data;
using System.Data.Common;
using System;

#endregion

namespace MetaGeta.Utilities {
    public static class DbHelpers {
        public static void AddParam(this DbCommand cmd, object value) {
            DbParameter param = cmd.CreateParameter();
            param.Value = value;
            cmd.Parameters.Add(param);
        }

        public static T Get<T>(this IDataReader rdr, int index) where T : class {
            return rdr.IsDBNull(index) ? null : (T)rdr[index];
        }
    }
}