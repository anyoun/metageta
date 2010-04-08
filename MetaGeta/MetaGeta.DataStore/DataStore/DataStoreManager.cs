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

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using log4net;
using MetaGeta.DataStore.Database;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.DataStore {
    public class DataStoreManager : INotifyPropertyChanged, IDataStoreOwner {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IDataMapper m_DataMapper;
        private readonly ObservableCollection<MGDataStore> m_DataStores = new ObservableCollection<MGDataStore>();

        private readonly JobQueue m_JobQueue;

        public DataStoreManager(bool designMode) {
            log.InfoFormat("DataStoreManager ctor");

            if (!designMode)
                m_DataMapper = new DataMapper("metageta.db3");
            else
                m_DataMapper = new MockDataMapper();

            m_JobQueue = new JobQueue(designMode);

            m_DataMapper.Initialize();
            m_DataStores.AddRange(m_DataMapper.GetDataStores(this));
            OnDataStoresChanged();
        }

        public JobQueue JobQueue {
            get { return m_JobQueue; }
        }

        public ObservableCollection<MGDataStore> DataStores {
            get { return m_DataStores; }
        }

        #region IDataStoreOwner Members

        public void DeleteDataStore(MGDataStore dataStore) {
            dataStore.Close();
            m_DataMapper.RemoveDataStore(dataStore);
            DataStores.Remove(dataStore);
        }

        public void EnqueueAction(IAction action, MGDataStore dataStore, MGFile file) {
            m_JobQueue.EnqueueAction(action, dataStore, file);
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void WaitForQueueToEmpty() {
            m_JobQueue.WaitForQueueToEmpty();
        }

        public void Shutdown() {
            m_JobQueue.Dispose();
            foreach (MGDataStore ds in m_DataStores)
                ds.Close();
            m_DataMapper.Close();
        }

        public MGDataStore NewDataStore(string name, IDataStoreTemplate template) {
            var data = new MGDataStore(this, m_DataMapper);
            using ((data.SuspendUpdates())) {
                data.Template = template;
                data.Name = name;
            }
            foreach (string pluginTypeName in template.GetPluginTypeNames())
                data.AddNewPlugin(pluginTypeName);
            m_DataMapper.WriteNewDataStore(data);
            m_DataStores.Add(data);
            OnDataStoresChanged();
            return data;
        }

        private void OnDataStoresChanged() {
            OnPropertyChanged("DataStores");
        }

        private void OnPropertyChanged(string name) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}