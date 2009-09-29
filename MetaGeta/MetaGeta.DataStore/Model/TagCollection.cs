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
using System.Globalization;
using System.Linq;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.DataStore {
    public class MGTagCollection {
        private readonly Dictionary<string, LinkedList<MGTag>> m_Items = new Dictionary<string, LinkedList<MGTag>>();

        public MGTagCollection() { }

        public bool ContainsKey(string tagName) {
            LinkedList<MGTag> tagList = null;
            return m_Items.TryGetValue(tagName, out tagList) && tagList.Any();
        }

        private T GetValue<T>(string tagName) where T : class {
            return GetValues<T>(tagName).FirstOrDefault();
        }
        private T? GetValueNullable<T>(string tagName) where T : struct {
            var values = GetValues<T>(tagName);
            return values.Any() ? (T?)values.First() : null;
        }

        private IEnumerable<T> GetValues<T>(string tagName) {
            LinkedList<MGTag> tagList = null;
            if (!m_Items.TryGetValue(tagName, out tagList)) return new T[0];

            if (typeof(T) != typeof(object)) {
                var requestType = MGTag.GetTagType(typeof(T));
                var tagType = tagList.Agree(t => t.Type);
                if (requestType != tagType)
                    throw new Exception(string.Format("Tag {0} is of type {1}, not {2}.", tagName, tagType, requestType));
            }
            return from t in tagList select (T)t.Value;
        }

        public void Remove(string tagName) {
            m_Items.Remove(tagName);
        }

        private void SetValue(string tagName, object value) {
            Remove(tagName);
            AddValue(tagName, value);
        }
        private void AddValue(string tagName, object value) {
            var requestType = MGTag.GetTagType(value);
            if (requestType == MGTagType.NoType)
                throw new ArgumentException("Invalid type. Expected a bool, string, long, double, DateTimeOffset, TimeSpan, or byte[].", "value");

            LinkedList<MGTag> tagList = null;

            if (!m_Items.TryGetValue(tagName, out tagList)) {
                tagList = new LinkedList<MGTag>();
                m_Items.Add(tagName, tagList);
            } else {
                var tagType = tagList.Agree(t => t.Type);
                if (tagType != requestType)
                    throw new ArgumentException(string.Format("Tag {0} is of type {1}, not {2}.", tagName, tagType, requestType), "value");
            }
            tagList.AddLast(new MGTag(tagName, value, requestType));
        }

        public MGTag GetTag(string tagName) {
            LinkedList<MGTag> tagList = null;
            if (m_Items.TryGetValue(tagName, out tagList))
                return tagList.First();
            else
                return null;
        }

        #region Typed getters and setters
        public bool? GetBool(string tagName) { return GetValueNullable<bool>(tagName); }
        public string GetString(string tagName) { return GetValue<string>(tagName); }
        public int? GetInt(string tagName) { return (int?)GetValueNullable<long>(tagName); }
        public long? GetLong(string tagName) { return GetValueNullable<long>(tagName); }
        public double? GetDouble(string tagName) { return GetValueNullable<double>(tagName); }
        public DateTimeOffset? GetDateTime(string tagName) { return GetValueNullable<DateTimeOffset>(tagName); }
        public TimeSpan? GetTimeSpan(string tagName) { return GetValueNullable<TimeSpan>(tagName); }
        public byte[] GetBlob(string tagName) { return GetValue<byte[]>(tagName); }
        public object GetObject(string tagName) { return GetValue<object>(tagName); }

        public void Set(string tagName, bool value) { SetValue(tagName, value); }
        public void Set(string tagName, string value) { SetValue(tagName, value); }
        public void Set(string tagName, int value) { SetValue(tagName, (long)value); }
        public void Set(string tagName, long value) { SetValue(tagName, value); }
        public void Set(string tagName, double value) { SetValue(tagName, value); }
        public void Set(string tagName, DateTimeOffset value) { SetValue(tagName, value); }
        public void Set(string tagName, TimeSpan value) { SetValue(tagName, value); }
        public void Set(string tagName, byte[] value) { SetValue(tagName, value); }
        public void SetObject(string tagName, object value) { SetValue(tagName, value); }

        public void Add(string tagName, bool value) { AddValue(tagName, value); }
        public void Add(string tagName, string value) { AddValue(tagName, value); }
        public void Add(string tagName, int value) { AddValue(tagName, (long)value); }
        public void Add(string tagName, long value) { AddValue(tagName, value); }
        public void Add(string tagName, double value) { AddValue(tagName, value); }
        public void Add(string tagName, DateTimeOffset value) { AddValue(tagName, value); }
        public void Add(string tagName, TimeSpan value) { AddValue(tagName, value); }
        public void Add(string tagName, byte[] value) { AddValue(tagName, value); }
        #endregion

        #region Iterators
        public IEnumerable<MGTag> All {
            get {
                foreach (var pair in m_Items) {
                    foreach (var item in pair.Value) {
                        yield return item;
                    }
                }
            }
        }
        #endregion
    }
}