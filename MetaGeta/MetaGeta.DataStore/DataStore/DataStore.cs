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
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using log4net;
using MetaGeta.DataStore.Database;
using MetaGeta.Utilities;
using System.Concurrency;
using System.Threading.Tasks;

#endregion

namespace MetaGeta.DataStore {
	[Serializable()]
	public class MGDataStore : INotifyPropertyChanged {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		private readonly List<IMGPluginBase> m_AllPlugins = new List<IMGPluginBase>();
		private readonly IDataMapper m_DataMapper;
		private readonly IDataStoreOwner m_Owner;
		private readonly CancellationTokenSource m_CancelTaskSource = new CancellationTokenSource();
		private string m_Description = "";
		private long m_ID = -1;
		private string m_Name;
		private IDataStoreTemplate m_Template;

		internal MGDataStore(IDataStoreOwner owner, IDataMapper dataMapper) {
			m_Owner = owner;
			m_DataMapper = dataMapper;

			ImportStatus = new ImportStatus(string.Format("{0} files total.", m_DataMapper.GetFileCount(this)), 0);
		}

		public IList<MGFile> Files { get { return m_DataMapper.GetFiles(this); } }

		public string Name {
			get { return m_Name; }
			set {
				if (value != m_Name) {
					m_Name = value;
					if (!AreUpdatesSuspended) {
						m_DataMapper.WriteDataStore(this);
						OnNameChanged();
					}
				}
			}
		}

		public string Description {
			get { return m_Description; }
			set {
				if (value != m_Description) {
					m_Description = value;
					if (!AreUpdatesSuspended) {
						m_DataMapper.WriteDataStore(this);
						OnDescriptionChanged();
					}
				}
			}
		}

		public IDataStoreTemplate Template {
			get { return m_Template; }
			set {
				if (!ReferenceEquals(value, m_Template)) {
					m_Template = value;
					if (!AreUpdatesSuspended) {
						m_DataMapper.WriteDataStore(this);
						OnTemplateChanged();
					}
				}
			}
		}

		public long ID {
			get { return m_ID; }
			set {
				if (value != m_ID)
					m_ID = value;
			}
		}

		#region "Suspending Updates"

		private int m_SuspendCount = 0;

		private bool AreUpdatesSuspended { get { return m_SuspendCount != 0; } }

		public IDisposable SuspendUpdates() {
			m_SuspendCount += 1;
			return new SuspendUpdatesToken(this);
		}

		private class SuspendUpdatesToken : IDisposable {
			private readonly MGDataStore m_DataStore;

			// To detect redundant calls
			private bool disposedValue = false;

			public SuspendUpdatesToken(MGDataStore dataStore) {
				m_DataStore = dataStore;
			}

			// IDisposable
			protected virtual void Dispose(bool disposing) {
				if (!disposedValue) {
					if (disposing)
						m_DataStore.m_SuspendCount -= 1;

					// TODO: free your own state (unmanaged objects).
					// TODO: set large fields to null.
				}
				disposedValue = true;
			}

			#region " IDisposable Support "

			// This code added by Visual Basic to correctly implement the disposable pattern.
			public void Dispose() {
				// Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
				Dispose(true);
				GC.SuppressFinalize(this);
			}

			#endregion
		}

		#endregion

		public void Close() {
			m_CancelTaskSource.Cancel();
			foreach (IMGPluginBase plugin in m_AllPlugins)
				plugin.Shutdown();
		}

		public void Delete() {
			m_Owner.DeleteDataStore(this);
		}

		public IList<Tuple<MGTag, MGFile>> GetAllTagOnFiles(string tagName) {
			return m_DataMapper.GetTagOnAllFiles(this, tagName);
		}

		public void RemoveFiles(IEnumerable<MGFile> files) {
			m_DataMapper.RemoveFiles(files, this);
			OnFilesChanged();
		}

		public override string ToString() {
			return "MGDataStore: " + Name;
		}

		public string ToCsv() {
			var sb = new StringBuilder();
			sb.AppendLine();
			foreach (MGFile f in Files)
				sb.AppendLine();
			return sb.ToString();
		}

		#region "Action Plugins"

		public IEnumerable<IAction> GetActions() {
			return m_AllPlugins.OfType<IMGFileActionPlugin>().SelectMany(p => p.GetActions()).ToArray();
		}

		public void DoAction(MGFile file, IAction action) {
			m_Owner.EnqueueAction(action, this, file);
		}

		#endregion

		#region "Property Changed"

		public event PropertyChangedEventHandler PropertyChanged;

		private void OnNameChanged() {
			OnPropertyChanged("Name");
		}
		private void OnDescriptionChanged() {
			OnPropertyChanged("Description");
		}
		private void OnTemplateChanged() {
			OnPropertyChanged("Template");
		}
		private void OnFilesChanged() {
			OnPropertyChanged("Files");
		}
		private void OnPropertyChanged(string propertyName) {
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		#endregion

		#region "DataStoreCreationArguments"

		public DataStoreBuilder GetCreationArguments() {
			var fileSourceSettings = SettingInfoCollection.GetSettings((IMGPluginBase)FileSourcePlugins.Single());
			return new DataStoreBuilder {
				Name = Name,
				Description = Description,
				Tempate = Template,
				DirectoriesToWatch = fileSourceSettings.GetSetting("DirectoriesToWatch").GetValueAsString(),
				Extensions = fileSourceSettings.GetSetting("Extensions").GetValueAsString()
			};
		}

		public void SetCreationArguments(DataStoreBuilder args) {
			Name = args.Name;
			Description = args.Description;
			var fileSourceSettings = SettingInfoCollection.GetSettings((IMGPluginBase)FileSourcePlugins.Single());
			fileSourceSettings.GetSetting("DirectoriesToWatch").SetValueAsString(args.DirectoriesToWatch);
			fileSourceSettings.GetSetting("Extensions").SetValueAsString(args.Extensions);
		}

		#endregion

		#region "Images"

		public string GetImageDirectory() {
			string imagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images");
			if (!Directory.Exists(imagePath))
				Directory.CreateDirectory(imagePath);
			return imagePath;
		}

		#endregion

		#region "Plugins"

		public IEnumerable<IMGTaggingPlugin> TaggingPlugins {
			get { return m_AllPlugins.Where(p => p is IMGTaggingPlugin).Cast<IMGTaggingPlugin>(); }
		}

		public IEnumerable<IMGFileSourcePlugin> FileSourcePlugins {
			get { return m_AllPlugins.Where(p => p is IMGFileSourcePlugin).Cast<IMGFileSourcePlugin>(); }
		}

		public IEnumerable<IMGFileActionPlugin> FileActionPlugins {
			get { return m_AllPlugins.Where(p => p is IMGFileActionPlugin).Cast<IMGFileActionPlugin>(); }
		}

		public IEnumerable<IMGPluginBase> Plugins {
			get { return m_AllPlugins; }
		}

		internal void AddExistingPlugin(IMGPluginBase plugin, long id) {
			m_AllPlugins.Add(plugin);
			StartupPlugin(plugin, id, false);
		}

		internal void AddNewPlugin(string pluginTypeName) {
			Type t = Type.GetType(pluginTypeName);
			if (t == null)
				throw new Exception(string.Format("Can't find type \"{0}\".", pluginTypeName));
			var plugin = (IMGPluginBase)Activator.CreateInstance(t);
			m_AllPlugins.Add(plugin);
			//Datamapper will call StartupPlugin() after IDs have been assigned
		}

		internal void StartupPlugin(IMGPluginBase plugin, long id, bool firstRun) {
			if (!Plugins.Contains(plugin))
				throw new Exception("Can't find plugin in StartupPlugin().");

			log.InfoFormat("StartupPlugin(): {0}", plugin.GetType().FullName);

			if (plugin is IMGFileSourcePlugin) {
				var fsp = (IMGFileSourcePlugin)plugin;
				var bufferTime = TimeSpan.FromMilliseconds(100);
				fsp.FileAddedEvent.BufferWithTime(bufferTime).Where(l => l.Count > 0).Subscribe(l => OnFileAdded(fsp, l));
				fsp.FileModifiedEvent.BufferWithTime(bufferTime).Where(l => l.Count > 0).Subscribe(l => OnFileModified(fsp, l));
				fsp.FileMovedEvent.BufferWithTime(bufferTime).Where(l => l.Count > 0).Subscribe(l => OnFileMoved(fsp, l));
				fsp.FileDeleteEvent.BufferWithTime(bufferTime).Where(l => l.Count > 0).Subscribe(l => OnFileDeleted(fsp, l));
			}

			plugin.Startup(this, id);

			foreach (SettingInfo setting in SettingInfoCollection.GetSettings(plugin)) {
				if (firstRun) {
					setting.Value = setting.Metadata.DefaultValue;
					m_DataMapper.WritePluginSetting(this, plugin.PluginID, setting.SettingName, setting.GetDefaultValueAsString());
				} else
					setting.SetValueAsString(m_DataMapper.GetPluginSetting(this, plugin.PluginID, setting.SettingName));
			}
			plugin.SettingChanged += Plugin_SettingChanged;
		}

		#endregion

		#region "Importing"

		private ImportStatus m_LastImportStatus = ImportStatus.None;
		private object m_LastImportStatusLock = new object();

		public ImportStatus ImportStatus {
			get {
				lock (m_LastImportStatusLock) {
					return m_LastImportStatus;
				}
			}
			private set {
				lock (m_LastImportStatusLock) {
					m_LastImportStatus = value;
					OnPropertyChanged("ImportStatus");
				}
			}
		}


		/// <summary>
		/// Starts an asynchronous refresh and import from all FileSourcePlugins.
		/// </summary>
		/// <remarks></remarks>
		public void BeginRefresh() {
			ImportStatus = new ImportStatus("Listing files...", 0);
			FileSourcePlugins.AsParallel().WithCancellation(m_CancelTaskSource.Token).ForEach(fs => fs.Refresh(m_CancelTaskSource.Token));
		}

		private void OnFileAdded(IMGFileSourcePlugin pluing, IList<FileAddedEventArgs> events) {
			var files = events.Select(e => new MGFile(this)).ToArray();
			m_DataMapper.WriteNewFiles(files, this);
			var filePaths = events.Select(e => e.FilePath).ToArray();
			for (int i = 0; i < filePaths.Length; i++) {
				ImportStatus = new ImportStatus(string.Format("Importing \"{0}\"", filePaths[i].LocalPath), filePaths.Length - 1);
				ImportNewFile(files[i], filePaths[i]);
			}
			ImportStatus = new ImportStatus("Import complete.", 0);
			m_Owner.MainThreadScheduler.Schedule(OnFilesChanged);
		}
		private void OnFileModified(IMGFileSourcePlugin pluing, IList<FileModifiedEventArgs> events) {

		}
		private void OnFileMoved(IMGFileSourcePlugin pluing, IList<FileMovedEventArgs> events) {

		}
		private void OnFileDeleted(IMGFileSourcePlugin pluing, IList<FileDeletedEventArgs> events) {
			var removedFiles = events.ToLookup(e => e.FilePath);
			m_DataMapper.RemoveFiles(Files.Where(f => removedFiles.Contains(f.FileNameUri)), this);
			ImportStatus = new ImportStatus("", 0);
			m_Owner.MainThreadScheduler.Schedule(OnFilesChanged);
		}

		public MGFile AddNewFile(Uri path) {
			var file = new MGFile(this);
			m_DataMapper.WriteNewFiles(file.SingleToEnumerable(), this);
			ImportNewFile(file, path);
			m_Owner.MainThreadScheduler.Schedule(OnFilesChanged);
			return file;
		}

		private void ImportNewFile(MGFile newfile, Uri filePath) {
			newfile.Tags.Set(MGFile.FileNameKey, filePath.AbsoluteUri);

			log.DebugFormat("Importing file: {0}....", filePath);

			foreach (IMGTaggingPlugin plugin in TaggingPlugins) {
				log.DebugFormat("Importing {0} with {1}....", newfile.FileName, plugin.GetType().FullName);
				try {
					plugin.Process(newfile, new ProgressStatus());
				} catch (Exception ex) {
					log.Warn(string.Format("Importing \"{0}\" with {1} failed.", filePath, plugin.GetType().FullName), ex);
				}
				m_DataMapper.WriteFile(newfile);
			}
		}

		#endregion

		#region "Settings"

		public void Plugin_SettingChanged(object sender, PropertyChangedEventArgs e) {
			var plugin = (IMGPluginBase)sender;
			string value = SettingInfoCollection.GetSettings(plugin).GetSetting(e.PropertyName).GetValueAsString();
			m_DataMapper.WritePluginSetting(this, plugin.PluginID, e.PropertyName, value);
		}

		#endregion
	}

	public class ImportStatus {
		private readonly int m_FilesRemaining;
		private readonly string m_StatusMessage;

		public ImportStatus(string statusMessage, int filesRemaining) {
			m_StatusMessage = statusMessage;
			m_FilesRemaining = filesRemaining;
		}

		public string StatusMessage { get { return m_StatusMessage; } }
		public int FilesRemaining { get { return m_FilesRemaining; } }

		public static readonly ImportStatus None = new ImportStatus(string.Empty, 0);
	}
}