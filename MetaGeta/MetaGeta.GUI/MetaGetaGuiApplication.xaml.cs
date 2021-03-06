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

		static MetaGetaGuiApplication() {
			GalaSoft.MvvmLight.Threading.DispatcherHelper.Initialize();
		}

		protected override void OnStartup(StartupEventArgs e) {
			base.OnStartup(e);
			XmlConfigurator.Configure();
			NativeHelper.ExtractSqlite();
			NativeHelper.ExtractMediaInfo();
		}
	}
}