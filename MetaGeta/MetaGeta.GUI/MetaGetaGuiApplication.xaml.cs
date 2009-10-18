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
using System.Windows;
using log4net.Config;
using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.GUI {
    public partial class MetaGetaGuiApplication : System.Windows.Application {
        // Application-level events, such as Startup, Exit, and DispatcherUnhandledException
        // can be handled in this file.

        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected override void OnStartup(StartupEventArgs e) {
            base.OnStartup(e);
            XmlConfigurator.Configure();
            ExtractUnmanagedDlls();
        }

        private static void ExtractUnmanagedDlls() {
            if (Is64BitProcess) {
                log.Debug("MetaGeta is running in 64-bit mode.");
                LoadUnmanagedDll(@"lib-x64\MediaInfo.dll");
                LoadUnmanagedDll(@"lib-x64\sqlite3.dll");
            } else {
                log.Debug("MetaGeta is running in 32-bit mode.");
                LoadUnmanagedDll(@"lib-x86\MediaInfo.dll");
                LoadUnmanagedDll(@"lib-x86\sqlite3.dll");
            }
        }

        private static bool Is64BitProcess {
            get { return IntPtr.Size == 8; }
        }


        [DllImport("kernel32", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        private static void LoadUnmanagedDll(string path) {
            IntPtr ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
                throw new Exception(string.Format("Couldn't load library \"{0}\".", path),
                                    Marshal.GetExceptionForHR(Marshal.GetLastWin32Error()));
        }
    }
}