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

#endregion

namespace MetaGeta.DataStore.Database {
    public interface IDataMapper {
        void Initialize();
        void Close();
        void WriteNewDataStore(MGDataStore dataStore);
        void WriteNewFiles(IEnumerable<MGFile> files, MGDataStore dataStore);
        IEnumerable<MGDataStore> GetDataStores(IDataStoreOwner owner);
        IList<MGFile> GetFiles(MGDataStore dataStore);
        IList<Tuple<MGTag, MGFile>> GetTagOnAllFiles(MGDataStore dataStore, string tagName);
        void WriteFile(MGFile file);
        void WriteDataStore(MGDataStore dataStore);
        void RemoveDataStore(MGDataStore datastore);
        void RemoveFiles(IEnumerable<MGFile> files, MGDataStore store);
        string GetPluginSetting(MGDataStore dataStore, long pluginID, string settingName);
        void WritePluginSetting(MGDataStore dataStore, long pluginID, string settingName, string settingValue);
        string GetGlobalSetting(string settingName);
        void WriteGlobalSetting(string settingName, string settingValue);
        IList<GlobalSetting> ReadGlobalSettings();
    }
}