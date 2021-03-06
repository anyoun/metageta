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
using System.Linq;
using System.Reflection;
using System.Text;

#endregion

namespace MetaGeta.Utilities {
	public static class LinqExtensions {
		public static string JoinToCsv(this IEnumerable<string> list) {
			var bdr = new StringBuilder();
			bool firstItem = true;

			foreach (string i in list) {
				if (firstItem)
					firstItem = false;
				else
					bdr.Append(",");
				string quoted = i.Replace("\"", "\"\"");
				if (quoted.Contains(",")) {
					bdr.Append("\"");
					bdr.Append(quoted);
					bdr.Append("\"");
				} else
					bdr.Append(quoted);
			}

			return bdr.ToString();
		}

		public static string JoinToString(this IEnumerable<string> list, string seperator) {
			var bdr = new StringBuilder();
			bool firstItem = true;

			foreach (string i in list) {
				if (firstItem)
					firstItem = false;
				else
					bdr.Append(seperator);
				bdr.Append(i);
			}

			return bdr.ToString();
		}

		public static string JoinToString<T>(this IEnumerable<T> list) {
			return list.JoinToString(string.Empty);
		}
		public static string JoinToString<T>(this IEnumerable<T> list, Func<T, string> selector) {
			return list.JoinToString(selector, string.Empty);
		}
		public static string JoinToString<T>(this IEnumerable<T> list, string separator) {
			return list.JoinToString(o => o.ToString(), separator);
		}
		public static string JoinToString<T>(this IEnumerable<T> list, Func<T, string> selector, string separator) {
			return list.Select(selector).JoinToString(separator);
		}

		public static string JoinToCsv<T>(this IEnumerable<T> list, Func<T, string> selector) {
			return list.Select(selector).JoinToCsv();
		}

		public static void ForEach<T>(this IEnumerable<T> list, Action<T> action) {
			foreach (T item in list)
				action(item);
		}

		public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items) {
			foreach (T item in items)
				collection.Add(item);
		}

		public static IEnumerable<T> SingleToEnumerable<T>(this T item) {
			return new[] { item };
		}
		public static IEnumerable<T> Cons<T>(this T item) {
			return new[] { item };
		}
		public static IEnumerable<T> Cons<T>(this IEnumerable<T> list, T item) {
			return list.Concat(new[] { item });
		}
		public static IEnumerable<T> Cons<T>(this T item, IEnumerable<T> list) {
			return new[] { item }.Concat(list);
		}

		public static bool IsDefined<T>(this MemberInfo info) where T : Attribute {
			return info.IsDefined<T>(false);
		}

		public static bool IsDefined<T>(this MemberInfo info, bool inherit) where T : Attribute {
			return info.IsDefined(typeof(T), inherit);
		}

		public static T GetCustomAttribute<T>(this MemberInfo info) where T : Attribute {
			return info.GetCustomAttribute<T>(false);
		}

		public static T GetCustomAttribute<T>(this MemberInfo info, bool inherit) where T : Attribute {
			return info.GetCustomAttributes(typeof(T), inherit).Cast<T>().Single();
		}

		public static IEnumerable<Tuple<T, U, int>> IndexInnerJoin<T, U>(this IEnumerable<T> left, IEnumerable<U> right) {
			IEnumerator<T> l = left.GetEnumerator();
			IEnumerator<U> r = right.GetEnumerator();
			var results = new List<Tuple<T, U, int>>();
			int i = 0;
			while (l.MoveNext() & r.MoveNext()) {
				results.Add(new Tuple<T, U, int>(l.Current, r.Current, i));
				i += 1;
			}
			return results;
		}

		public static TResult Agree<T, TResult>(this IEnumerable<T> collection, Func<T, TResult> f) {
			TResult[] results = collection.Select(f).ToArray();
			if (results.Length == 0)
				return default(TResult);
			TResult firstItem = results.First();
			foreach (TResult r in results) {
				if (!firstItem.Equals(r))
					return default(TResult);
			}
			return firstItem;
		}

		public static T Coalesce<T>(this IEnumerable<T> collection) where T : class {
			foreach (T item in collection) {
				if (item != null)
					return item;
			}
			return null;
		}

		public static T Coalesce<T>(this IEnumerable<T?> collection) where T : struct {
			foreach (T? item in collection) {
				if (item.HasValue)
					return item.Value;
			}
			return default(T);
		}

		public static T Coalesce<T>(this T item, params T[] rest) where T : class {
			return item ?? Coalesce((IEnumerable<T>)rest);
		}

		public static int IndexOfMax<T>(this IList<T> items, Func<T, int> resultSelector) {
			int indexOfMax = -1;
			int largestMetric = 0;

			for (int i = 0; i < items.Count; i++) {
				int metric = resultSelector(items[i]);
				if (metric > largestMetric) {
					largestMetric = metric;
					indexOfMax = i;
				}
			}

			return indexOfMax;
		}

		public static T MaxItem<T>(this IEnumerable<T> items, Func<T, int> resultSelector) {
			T maxItem = default(T);
			int largestMetric = 0;

			foreach (T item in items) {
				int metric = resultSelector(item);
				if (metric > largestMetric) {
					largestMetric = metric;
					maxItem = item;
				}
			}

			return maxItem;
		}

		public static IEnumerable<T> SkipBothEndsWhile<T>(this IEnumerable<T> items, Predicate<T> condition) {
			var arr = items.ToArray();
			int startIndex = 0, endIndex = 0;
			for (int i = 0; i < arr.Length; i++) {
				startIndex = 0;
				if (!condition(arr[i]))
					break;
			}
			for (int i = arr.Length - 1; i >= 0; i--) {
				endIndex = i;
				if (!condition(arr[i]))
					break;
			}
			for (int i = startIndex; i <= endIndex; i++) {
				yield return arr[i];
			}
		}

		public static IEnumerable<T> Except<T>(this IEnumerable<T> items, T item) {
			return items.Except(item.Cons());
		}
	}
}