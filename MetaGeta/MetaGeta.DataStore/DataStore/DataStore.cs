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

#endregion

namespace MetaGeta.DataStore {
    [Serializable()]
    public class MGDataStore : INotifyPropertyChanged {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly List<IMGPluginBase> m_AllPlugins = new List<IMGPluginBase>();
        private readonly IDataMapper m_DataMapper;
        private readonly Dictionary<string, IMGFileActionPlugin> m_FileActionDictionary = new Dictionary<string, IMGFileActionPlugin>();
        private readonly IDataStoreOwner m_Owner;
        private readonly ManualResetEvent m_ShutdownEvent = new ManualResetEvent(false);
        private string m_Description = "";
        private long m_ID = -1;
        private string m_Name;
        private IDataStoreTemplate m_Template;

        internal MGDataStore(IDataStoreOwner owner, IDataMapper dataMapper) {
            m_ImportThread = new Thread(ImportThread);
            m_Owner = owner;
            m_DataMapper = dataMapper;

            m_ImportThread.IsBackground = true;
            m_ImportThread.Start();
        }

        public IList<MGFile> Files {
            get { return m_DataMapper.GetFiles(this); }
        }

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

        private bool AreUpdatesSuspended {
            get { return m_SuspendCount != 0; }
        }

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
            m_ShutdownEvent.Set();
            m_ImportThread.Join();
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

        public void DoAction(MGFile file, string action) {
            m_Owner.EnqueueAction(action, this, file);
        }

        internal IMGFileActionPlugin LookupAction(string action) {
            return m_FileActionDictionary[action];
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
            return new DataStoreBuilder {
                                            Name = Name,
                                            Description = Description,
                                            Tempate = Template,
                                            DirectoriesToWatch = GetPluginSetting((IMGPluginBase) FileSourcePlugins.Single(), "DirectoriesToWatch"),
                                            Extensions = GetPluginSetting((IMGPluginBase) FileSourcePlugins.Single(), "Extensions")
                                        };
        }

        public void SetCreationArguments(DataStoreBuilder args) {
            Name = args.Name;
            Description = args.Description;
            SetPluginSetting((IMGPluginBase) FileSourcePlugins.Single(), "DirectoriesToWatch", args.DirectoriesToWatch);
            SetPluginSetting((IMGPluginBase) FileSourcePlugins.Single(), "Extensions", args.Extensions);
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
            var plugin = (IMGPluginBase) Activator.CreateInstance(t);
            m_AllPlugins.Add(plugin);
            //Datamapper will call StartupPlugin() after IDs have been assigned
        }

        internal void StartupPlugin(IMGPluginBase plugin, long id, bool firstRun) {
            if (!Plugins.Contains(plugin))
                throw new Exception("Can't find plugin in StartupPlugin().");

            log.InfoFormat("StartupPlugin(): {0}", plugin.GetType().FullName);

            plugin.Startup(this, id);

            foreach (SettingInfo setting in SettingInfoCollection.GetSettings(plugin)) {
                if (firstRun) {
                    setting.Value = setting.Metadata.DefaultValue;
                    SetPluginSetting(plugin, setting.SettingName, setting.GetDefaultValueAsString());
                } else
                    setting.SetValueAsString(GetPluginSetting(plugin, setting.SettingName));
            }
            plugin.SettingChanged += Plugin_SettingChanged;

            if (plugin is IMGFileActionPlugin)
                SetUpAction((IMGFileActionPlugin) plugin);
        }

        private void SetUpAction(IMGFileActionPlugin ap) {
            foreach (string s in ap.GetActions())
                m_FileActionDictionary.Add(s, ap);
        }

        #endregion

        #region "Importing"

        private readonly AutoResetEvent m_ImportNow = new AutoResetEvent(false);
        private readonly Thread m_ImportThread;
        private ReadOnlyCollection<MGFile> m_ImportQueueCache;
        private ImportStatus m_LastImportStatus = new ImportStatus();

        private object m_LastImportStatusLock = new object();

        public ImportStatus ImportStatus {
            get {
                lock (m_LastImportStatusLock) {
                    return m_LastImportStatus;
                }
            }
        }

        private IEnumerable<MGFile> ImportQueue {
            get {
                if (m_ImportQueueCache == null || m_ImportQueueCache.Count == 0)
                    m_ImportQueueCache = new ReadOnlyCollection<MGFile>(Files.Where(f => !bool.Parse(f.GetTag("ImportComplete").Coalesce("False"))).ToArray());
                return m_ImportQueueCache;
            }
        }

        /// <summary>
        /// Starts an asynchronous refresh and import from all FileSourcePlugins.
        /// </summary>
        /// <remarks></remarks>
        public void BeginRefresh() {
            m_ImportNow.Set();
        }

        private void SetImportStatus(string message, int progressPct) {
            lock (m_LastImportStatusLock) {
                if (progressPct == -1)
                    m_LastImportStatus = new ImportStatus(message);
                else
                    m_LastImportStatus = new ImportStatus(message, progressPct);
            }
            OnPropertyChanged("ImportStatus");
        }

        public void ImportThread() {
            while (true) {
                int wakeupHandle = WaitHandle.WaitAny(new WaitHandle[] {
                                                                           m_ImportNow,
                                                                           m_ShutdownEvent
                                                                       });
                if (wakeupHandle == 1)
                    return;

                SetImportStatus("Listing files...", 0);

                //Get list of files
                Uri[] newPaths = (from fs in FileSourcePlugins
                                  from path in fs.GetFilesToAdd()
                                  select path).ToArray();
                MGFile[] files = (newPaths.Select(p => new MGFile(this))).ToArray();
                m_DataMapper.WriteNewFiles(files, this);

                foreach (Tuple<MGFile, Uri, int> fileAndPath in files.IndexInnerJoin(newPaths)) {
                    MGFile newfile = fileAndPath.First;
                    Uri filePath = fileAndPath.Second;
                    int index = fileAndPath.Third;
                    SetImportStatus(string.Format("Importing \"{0}\"", filePath.LocalPath), (int) (index * 100.0) / files.Length);
                    ImportNewFile(newfile, filePath);
                }

                SetImportStatus("Import complete.", 100);

                OnFilesChanged();
            }
        }

        public MGFile AddNewFile(Uri path) {
            var file = new MGFile(this);
            m_DataMapper.WriteNewFiles(file.SingleToEnumerable(), this);
            ImportNewFile(file, path);
            OnFilesChanged();
            return file;
        }

        private void ImportNewFile(MGFile newfile, Uri filePath) {
            newfile.SetTag(MGFile.FileNameKey, filePath.AbsoluteUri);
            newfile.SetTag("ImportComplete", false.ToString());

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
            var plugin = (IMGPluginBase) sender;
            string value = SettingInfoCollection.GetSettings(plugin).GetSetting(e.PropertyName).GetValueAsString();
            SetPluginSetting(plugin, e.PropertyName, value);
        }

        public string GetPluginSetting(IMGPluginBase plugin, string settingName) {
            return m_DataMapper.GetPluginSetting(this, plugin.PluginID, settingName);
        }

        public void SetPluginSetting(IMGPluginBase plugin, string settingName, string settingValue) {
            m_DataMapper.WritePluginSetting(this, plugin.PluginID, settingName, settingValue);
        }

        #endregion
    }
}

namespace MetaGeta.DataStore {
    public class ImportStatus {
        private readonly bool m_IsImporting;

        private readonly bool m_IsIndeterminate;
        private readonly int m_ProgressPct;
        private readonly string m_StatusMessage;

        public ImportStatus(string statusMessage, int progressPct) {
            m_StatusMessage = statusMessage;
            m_ProgressPct = progressPct;
            m_IsImporting = true;
            m_IsIndeterminate = false;
        }

        public ImportStatus() : this(string.Empty, 0) {
            m_IsImporting = false;
        }

        public ImportStatus(string statusMessage) : this(statusMessage, 0) {
            m_IsIndeterminate = true;
        }

        public string StatusMessage {
            get { return m_StatusMessage; }
        }

        public int ProgressPct {
            get { return m_ProgressPct; }
        }

        public bool IsImporting {
            get { return m_IsImporting; }
        }

        public bool IsIndeterminate {
            get { return m_IsIndeterminate; }
        }
    }
}