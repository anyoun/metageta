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

using System.Runtime.InteropServices;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using MetaGeta.Utilities;
using System.ComponentModel;

#endregion

namespace MetaGeta.Utilities {
	public class NativeHelper {
		public static void ExtractMediaInfo() {
			if (Environment.Is64BitProcess) {
				LoadUnmanagedDll(@"lib-x64\MediaInfo.dll");
			} else {
				LoadUnmanagedDll(@"lib-x86\MediaInfo.dll");
			}
		}
		public static void ExtractSqlite() {
			if (Environment.Is64BitProcess) {
				LoadUnmanagedDll(@"lib-x64\sqlite3.dll");
			} else {
				LoadUnmanagedDll(@"lib-x86\sqlite3.dll");
			}
		}

		[DllImport("kernel32", SetLastError = true)]
		static extern IntPtr LoadLibrary(string lpFileName);

		private static void LoadUnmanagedDll(string path) {
			if (!File.Exists(path))
				throw new Exception(string.Format("Can't find dll \"{0}\".", Path.GetFullPath(path)));
			IntPtr ptr = LoadLibrary(path);
			if (ptr == IntPtr.Zero)
				throw new Exception(string.Format("Couldn't load library \"{0}\".", path),
									new Win32Exception());
		}
	}
}
