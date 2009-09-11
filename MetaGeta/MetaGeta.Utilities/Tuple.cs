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

#endregion

namespace MetaGeta.Utilities {
    public class Tuple<TFirst, TSecond> : IEquatable<Tuple<TFirst, TSecond>> {
        private readonly TFirst m_First;

        private readonly TSecond m_Second;

        public Tuple(TFirst first, TSecond second) {
            m_First = first;
            m_Second = second;
        }

        public TFirst First {
            get { return m_First; }
        }

        public TSecond Second {
            get { return m_Second; }
        }

        #region IEquatable<Tuple<TFirst,TSecond>> Members

        bool IEquatable<Tuple<TFirst, TSecond>>.Equals(Tuple<TFirst, TSecond> other) {
            return Equals1(other);
        }

        #endregion

        public override bool Equals(object obj) {
            return (obj) is Tuple<TFirst, TSecond> && Equals1((Tuple<TFirst, TSecond>) obj);
        }

        public override int GetHashCode() {
            return First.GetHashCode() ^ Second.GetHashCode();
        }

        public override string ToString() {
            return string.Format("First = {0}, Second = {1}", First, Second);
        }

        public bool Equals1(Tuple<TFirst, TSecond> other) {
            return EqualityComparer<TFirst>.Default.Equals(First, other.First) && EqualityComparer<TSecond>.Default.Equals(Second, other.Second);
        }
    }
}

namespace MetaGeta.Utilities {
    public class Tuple<T1, T2, T3> : IEquatable<Tuple<T1, T2, T3>> {
        private readonly T1 m_First;
        private readonly T2 m_Second;

        private readonly T3 m_Third;

        public Tuple(T1 first, T2 second, T3 third) {
            m_First = first;
            m_Second = second;
            m_Third = third;
        }

        public T1 First {
            get { return m_First; }
        }

        public T2 Second {
            get { return m_Second; }
        }

        public T3 Third {
            get { return m_Third; }
        }

        #region IEquatable<Tuple<T1,T2,T3>> Members

        bool IEquatable<Tuple<T1, T2, T3>>.Equals(Tuple<T1, T2, T3> other) {
            return Equals1(other);
        }

        #endregion

        public override bool Equals(object obj) {
            return (obj) is Tuple<T1, T2, T3> && Equals1((Tuple<T1, T2, T3>) obj);
        }

        public override int GetHashCode() {
            return First.GetHashCode() ^ Second.GetHashCode() ^ Third.GetHashCode();
        }

        public override string ToString() {
            return string.Format("First = {0}, Second = {1}, Third = {2}", First, Second, Third);
        }

        public bool Equals1(Tuple<T1, T2, T3> other) {
            return EqualityComparer<T1>.Default.Equals(First, other.First) && EqualityComparer<T2>.Default.Equals(Second, other.Second) &&
                   EqualityComparer<T3>.Default.Equals(Third, other.Third);
        }
    }
}