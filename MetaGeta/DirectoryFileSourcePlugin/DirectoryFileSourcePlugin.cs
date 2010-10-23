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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Reflection;
using log4net;
using MetaGeta.DataStore;
using MetaGeta.Utilities;
using System.Threading;

#endregion

namespace MetaGeta.DirectoryFileSourcePlugin {
	public class DirectoryFileSourcePlugin : IMGFileSourcePlugin, IMGPluginBase {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private MGDataStore m_DataStore;

		private ReadOnlyCollection<string> m_DirectoriesToWatch;
		private ReadOnlyCollection<string> m_Extensions;
		private long m_ID;

		private readonly Subject<FileAddedEventArgs> m_FileAddedSubject = new Subject<FileAddedEventArgs>();
		private readonly Subject<FileModifiedEventArgs> m_FileModifiedSubject = new Subject<FileModifiedEventArgs>();
		private readonly Subject<FileMovedEventArgs> m_FileMovedSubject = new Subject<FileMovedEventArgs>();
		private readonly Subject<FileDeletedEventArgs> m_FileDeleteSubject = new Subject<FileDeletedEventArgs>();

		public long ID { get { return m_ID; } }

		[Settings("Directories To Watch", new string[0], SettingType.DirectoryList, "Locations")]
		public ReadOnlyCollection<string> DirectoriesToWatch {
			get { return m_DirectoriesToWatch; }
			set {
				if (!ReferenceEquals(value, m_DirectoriesToWatch)) {
					m_DirectoriesToWatch = value;
					if (SettingChanged != null)
						SettingChanged(this, new PropertyChangedEventArgs("DirectoriesToWatch"));
				}
			}
		}

		[Settings("Extensions To Watch", new string[0], SettingType.ExtensionList, "Locations")]
		public ReadOnlyCollection<string> Extensions {
			get { return m_Extensions; }
			set {
				if (!ReferenceEquals(value, m_Extensions)) {
					m_Extensions = value;
					if (SettingChanged != null)
						SettingChanged(this, new PropertyChangedEventArgs("Extensions"));
				}
			}
		}

		#region IMGFileSourcePlugin Members

		public IObservable<FileAddedEventArgs> FileAddedEvent { get { return m_FileAddedSubject; } }
		public IObservable<FileModifiedEventArgs> FileModifiedEvent { get { return m_FileModifiedSubject; } }
		public IObservable<FileMovedEventArgs> FileMovedEvent { get { return m_FileMovedSubject; } }
		public IObservable<FileDeletedEventArgs> FileDeleteEvent { get { return m_FileDeleteSubject; } }

		public void Refresh(CancellationToken cancel) {
			var fileNameToFileDict = new Dictionary<string, MGFile>();
			foreach (var t in m_DataStore.GetAllTagOnFiles(MGFile.FileNameKey))
				fileNameToFileDict[new Uri((string)t.Item1.Value).LocalPath] = t.Item2;

			var newFiles = new List<Uri>();

			foreach (FileInfo fi in GetAllFilesInWatchedDirectories()) {
				if (fileNameToFileDict.Remove(fi.FullName)) {
					//Already exits, remove it from the dictionary
					log.DebugFormat("File \"{0}\" already exists.", fi.FullName);
				} else {
					//New files
					log.DebugFormat("New files: \"{0}\".", fi.FullName);
					m_FileAddedSubject.OnNext(new FileAddedEventArgs(new Uri(fi.FullName)));
				}
				if (cancel.IsCancellationRequested)
					return;
			}

			var deletedFiles = new List<Uri>();
			foreach (var item in fileNameToFileDict) {
				m_FileDeleteSubject.OnNext(new FileDeletedEventArgs(new Uri(item.Key)));
				deletedFiles.Add(new Uri(item.Key));
			}
		}

		#endregion

		#region IMGPluginBase Members

		public void Startup(MGDataStore dataStore, long id) {
			m_DataStore = dataStore;
			m_ID = id;
		}
		public void Shutdown() { }

		long IMGPluginBase.PluginID { get { return ID; } }
		public string FriendlyName { get { return "Directory File Source Plugin"; } }
		public string UniqueName { get { return "DirectoryFileSourcePlugin"; } }
		public Version Version { get { return new Version(1, 0, 0, 0); } }

		public event PropertyChangedEventHandler SettingChanged;

		#endregion

		private IEnumerable<FileInfo> GetAllFilesInWatchedDirectories() {
			var extensionsLookup = new HashSet<string>(Extensions);
			foreach (string d in DirectoriesToWatch) {
				DirectoryInfo di = null;
				try {
					di = new DirectoryInfo(d);
				} catch (ArgumentException) { } catch (PathTooLongException) { }
				if (di != null && di.Exists) {
					foreach (var fileInfo in di.EnumerateFiles("*", SearchOption.AllDirectories)) {
						var ext = Path.GetExtension(fileInfo.FullName);
						if(extensionsLookup.Contains(ext) || extensionsLookup.Contains(ext.TrimStart('.')))
							yield return fileInfo;
					}
				}
			}
		}
	}
}