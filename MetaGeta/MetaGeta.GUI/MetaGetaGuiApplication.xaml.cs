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
                ExtractGzipResourceToFile("MetaGeta.GUI.Resources.MediaInfo.dll.x64.gz", "MediaInfo.dll");
                ExtractGzipResourceToFile("MetaGeta.GUI.Resources.sqlite3.dll.x64.gz", "sqlite3.dll");
            } else {
                log.Debug("MetaGeta is running in 32-bit mode.");
                ExtractGzipResourceToFile("MetaGeta.GUI.Resources.MediaInfo.dll.x86.gz", "MediaInfo.dll");
                ExtractGzipResourceToFile("MetaGeta.GUI.Resources.sqlite3.dll.x86.gz", "sqlite3.dll");
            }
        }

        private static bool Is64BitProcess {
            get { return IntPtr.Size == 8; }
        }

        private static void ExtractGzipResourceToFile(string resourceName, string fileName) {
            if (File.Exists(fileName)) {
                log.DebugFormat("Not extracting {0} since it already exists.", fileName);
                return;
            }

            log.DebugFormat("Extracting {0}...", fileName);

            using (var memoryStream = Assembly.GetEntryAssembly().GetManifestResourceStream(resourceName)) {
                if (memoryStream == null) {
                    log.FatalFormat("Couldn't file resource {0}.", resourceName);
                    log.DebugFormat("Resource list: {1}", Assembly.GetEntryAssembly().GetManifestResourceNames().JoinToCsv());
                }
                using (var decompressedStream = new GZipStream(memoryStream, CompressionMode.Decompress))
                using (var fs = File.Open(fileName, FileMode.CreateNew, FileAccess.Write))
                    CopyStream(decompressedStream, fs);
            }

            log.Debug("OK");
        }

        private static void CopyStream(Stream input, Stream output) {
            byte[] buffer = new byte[0x1000];
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, read);
        }

    }
}