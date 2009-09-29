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
using System.Data;
using System.Data.Common;
using System.Text;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.DataStore.Database {
    public class TagCollectionPersister {
        public string KeyColumn { get; set; }
        public string TagNameColumn { get; set; }
        public string TagValueColumn { get; set; }
        public string TagTypeColumn { get; set; }

        public Dictionary<T, MGTagCollection> ReadTags<T>(DbDataReader rdr) where T : IEquatable<T> {
            int tagValueColumnIndex = rdr.GetOrdinal(TagValueColumn);
            var items = new Dictionary<T, MGTagCollection>();
            while (rdr.Read()) {
                var key = (T) rdr[KeyColumn];
                MGTagCollection tags;
                if (!items.TryGetValue(key, out tags)) {
                    tags = new MGTagCollection();
                    items.Add(key, tags);
                }
                var name = (string) rdr[TagNameColumn];
                var type = (MGTagType) Enum.Parse(typeof (MGTagType), (string) rdr[TagTypeColumn]);

                long valueSize = rdr.GetBytes(tagValueColumnIndex, 0, null, 0, 0);
                var value = new byte[valueSize];
                long bytesRead = rdr.GetBytes(tagValueColumnIndex, 0, value, 0, (int) valueSize);
                if (bytesRead != valueSize)
                    throw new Exception(string.Format("Wasn't able to read all bytes from a DbDataReader. Expected {0} but read {1}.", valueSize, bytesRead));

                switch (type) {
                    case MGTagType.Text:
                        tags.Add(name, new UTF8Encoding().GetString(value));
                        break;
                    case MGTagType.Integer:
                        tags.Add(name, BitConverter.ToInt64(value, 0));
                        break;
                    case MGTagType.Real:
                        tags.Add(name, BitConverter.ToDouble(value, 0));
                        break;
                    case MGTagType.DateTime:
                        tags.Add(name, DateTimeOffset.FromFileTime(BitConverter.ToInt64(value, 0)));
                        break;
                    case MGTagType.TimeSpan:
                        tags.Add(name, TimeSpan.FromTicks(BitConverter.ToInt64(value, 0)));
                        break;
                    case MGTagType.Blob:
                        tags.Add(name, value);
                        break;
                    case MGTagType.Boolean:
                        tags.Add(name, BitConverter.ToBoolean(value, 0));
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return items;
        }

        public void WriteTags(MGTagCollection tags, DbCommand cmd) {
            cmd.AddParam(TagNameColumn, DbType.String);
            cmd.AddParam(TagTypeColumn, DbType.String);
            cmd.AddParam(TagValueColumn, DbType.Object);

            foreach (MGTag tag in tags.All) {
                cmd.Parameters[TagNameColumn].Value = tag.Name;
                cmd.Parameters[TagTypeColumn].Value = tag.Type.ToString();

                byte[] value = null;

                switch (tag.Type) {
                    case MGTagType.Text:
                        value = new UTF8Encoding().GetBytes((string) tag.Value);
                        break;

                    case MGTagType.Integer:
                        value = BitConverter.GetBytes((long) tag.Value);
                        break;

                    case MGTagType.Real:
                        value = BitConverter.GetBytes((double) tag.Value);
                        break;

                    case MGTagType.DateTime:
                        value = BitConverter.GetBytes(((DateTimeOffset) tag.Value).ToFileTime());
                        break;

                    case MGTagType.Boolean:
                        value = BitConverter.GetBytes((bool) tag.Value);
                        break;

                    case MGTagType.TimeSpan:
                        value = BitConverter.GetBytes(((TimeSpan) tag.Value).Ticks);
                        break;

                    case MGTagType.Blob:
                        value = (byte[]) tag.Value;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }
                cmd.Parameters[TagValueColumn].Value = value;
                cmd.Parameters[TagValueColumn].DbType = DbType.Binary;

                cmd.ExecuteNonQuery();
            }
        }
    }
}