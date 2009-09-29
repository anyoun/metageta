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
using System.Linq;

#endregion

namespace MetaGeta.DataStore {
    public class MGTag : IEquatable<MGTag> {
        private readonly string m_Name;
        private readonly object m_Value;
        private readonly MGTagType m_Type;

        public MGTag(string name, object value, MGTagType type) {
            m_Name = name;
            m_Value = value;
            m_Type = type;
        }

        public string Name {
            get { return m_Name; }
        }

        public object Value {
            get { return m_Value; }
        }

        public MGTagType Type {
            get { return m_Type; }
        }

        #region IEquatable<MGTag> Members

        public bool Equals(MGTag other) {
            if (other.Name != Name || other.Type != Type)
                return false;

            if (object.ReferenceEquals(other.Value, Value))
                return true;

            if (object.ReferenceEquals(other.Value, null) || object.ReferenceEquals(Value, null))
                return false;

            switch (m_Type) {
                case MGTagType.Text:
                    return string.Equals((string)other.Value, (string)Value, StringComparison.InvariantCulture);

                case MGTagType.Integer:
                    return (long)other.Value == (long)Value;

                case MGTagType.Real:
                    return (double)other.Value == (double)Value;

                case MGTagType.DateTime:
                    return (DateTimeOffset)other.Value == (DateTimeOffset)Value;

                case MGTagType.TimeSpan:
                    return (TimeSpan)other.Value == (TimeSpan)Value;

                case MGTagType.Blob:
                    return ((byte[])other.Value).SequenceEqual((byte[])Value);

                case MGTagType.Boolean:
                    return (bool)other.Value == (bool)Value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public override int GetHashCode() {
            unchecked {
                int result = (m_Name != null ? m_Name.GetHashCode() : 0);
                result = (result * 397) ^ (m_Value != null ? m_Value.GetHashCode() : 0);
                result = (result * 397) ^ m_Type.GetHashCode();
                return result;
            }
        }

        #endregion

        public override string ToString() {
            return string.Format("Name: {0}, Value: {1}, Type: {2}", m_Name, m_Value, m_Type);
        }

        public static MGTagType GetTagType(object value) {
            return GetTagType(value.GetType());
        }
        public static MGTagType GetTagType(Type t) {
            if (t == typeof(string)) return MGTagType.Text;
            if (t == typeof(int)) return MGTagType.Integer;
            if (t == typeof(long)) return MGTagType.Integer;
            if (t == typeof(double)) return MGTagType.Real;
            if (t == typeof(DateTimeOffset)) return MGTagType.DateTime;
            if (t == typeof(TimeSpan)) return MGTagType.TimeSpan;
            if (t == typeof(byte[])) return MGTagType.Blob;
            if (t == typeof(bool)) return MGTagType.Boolean;
            return MGTagType.NoType;
        }
    }

    public enum MGTagType {
        NoType = 0,
        Text = 1,
        Integer = 2,
        Real = 3,
        DateTime = 4,
        TimeSpan = 5,
        Blob = 6,
        Boolean = 7,
    }
}