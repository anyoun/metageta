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

using System.Collections.Generic;
using MetaGeta.Utilities;
using System;

#endregion

namespace MetaGeta.DataStore.Database {
    public class MockDataMapper : IDataMapper {
        #region IDataMapper Members

        public void Initialize() {}


        public void Close() {}

        public IList<Tuple<MGTag, MGFile>> GetTagOnAllFiles(MGDataStore dataStore, string tagName) {
            return new List<Tuple<MGTag, MGFile>>();
        }

        public IEnumerable<MGDataStore> GetDataStores(IDataStoreOwner owner) {
            return new[] {new MGDataStore(owner, this) {Name = "Sample DataStore"}};
        }

        public IList<MGFile> GetFiles(MGDataStore dataStore) {
            return new MGFile[0];
        }

        public string GetGlobalSetting(string settingName) {
            return null;
        }

        public string GetPluginSetting(MGDataStore dataStore, long pluginID, string settingName) {
            return string.Empty;
        }

        public IList<GlobalSetting> ReadGlobalSettings() {
            return new GlobalSetting[0];
        }


        public void RemoveDataStore(MGDataStore datastore) {}


        public void RemoveFiles(IEnumerable<MGFile> files, MGDataStore store) {}


        public void WriteDataStore(MGDataStore dataStore) {}


        public void WriteGlobalSetting(string settingName, string settingValue) {}


        public void WriteNewDataStore(MGDataStore dataStore) {}


        public void WriteNewFiles(IEnumerable<MGFile> files, MGDataStore dataStore) {}


        public void WritePluginSetting(MGDataStore dataStore, long pluginID, string settingName, string settingValue) {}


        public void WriteFile(MGFile file) {}

        #endregion
    }
}