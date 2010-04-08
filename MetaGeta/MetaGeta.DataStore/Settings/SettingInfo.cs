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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.DataStore {
    public class SettingInfo : INotifyPropertyChanged {
        private readonly SettingsAttribute m_Metadata;
        private readonly PropertyInfo m_PropertyInfo;
        private readonly object m_TargetObject;

        public SettingInfo(object targetObject, PropertyInfo propertyInfo, SettingsAttribute metadata) {
            m_TargetObject = targetObject;
            m_PropertyInfo = propertyInfo;
            m_Metadata = metadata;
        }

        public string SettingName {
            get { return m_PropertyInfo.Name; }
        }

        public SettingsAttribute Metadata {
            get { return m_Metadata; }
        }

        public object Value {
            get { return m_PropertyInfo.GetValue(m_TargetObject, null); }
            set {
                m_PropertyInfo.SetValue(m_TargetObject, value, null);
                OnPropertyChanged("IsDefault");
            }
        }

        public bool IsDefault {
            get { return Equals(Value, Metadata.DefaultValue); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public string GetValueAsString() {
            return ValueToString(Value);
        }

        public string GetDefaultValueAsString() {
            return ValueToString(Metadata.DefaultValue);
        }

        private string ValueToString(object val) {
            switch (Metadata.Type) {
                case SettingType.Directory:
                case SettingType.ShortText:
                case SettingType.File:
                case SettingType.LongText:
                    return (string) val;

                case SettingType.ExtensionList:
                case SettingType.DirectoryList:
                case SettingType.FileList:
                    return ((ReadOnlyCollection<string>) val).JoinToString(";");

                case SettingType.Int:
                    return ((int) val).ToString("D");

                case SettingType.Float:
                    return ((double) val).ToString("R");

                case SettingType.Date:
                    return ((DateTimeOffset) val).ToString("o");

                default:
                    throw new Exception(string.Format("Unknown setting type: {0}.", Metadata.Type));
            }
        }

        public void SetValueAsString(string s) {
            switch (Metadata.Type) {
                case SettingType.Directory:
                case SettingType.ShortText:
                case SettingType.File:
                case SettingType.LongText:
                    Value = s;
                    break;

                case SettingType.ExtensionList:
                case SettingType.DirectoryList:
                case SettingType.FileList:
                    Value = new ReadOnlyCollection<string>(s.Split(';'));
                    break;

                case SettingType.Int:
                    Value = int.Parse(s);
                    break;

                case SettingType.Float:
                    Value = double.Parse(s);
                    break;

                case SettingType.Date:
                    Value = DateTimeOffset.ParseExact(s, "o", null, System.Globalization.DateTimeStyles.RoundtripKind);
                    break;

                default:
                    throw new Exception(string.Format("Unknown setting type: {0}.", Metadata.Type));
            }
        }

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

namespace MetaGeta.DataStore {
    public class SettingInfoCollection : IEnumerable<SettingInfo> {
        private readonly List<SettingInfo> m_Settings;
        private readonly object m_TargetObject;

        public SettingInfoCollection(object targetObject, IEnumerable<SettingInfo> settings) {
            m_TargetObject = targetObject;
            m_Settings = new List<SettingInfo>(settings);
        }

        #region IEnumerable<SettingInfo> Members

        public IEnumerator<SettingInfo> GetEnumerator() {
            return m_Settings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }

        #endregion

        public SettingInfo GetSetting(string name) {
            return m_Settings.First(s => s.SettingName == name);
        }

        public static SettingInfoCollection GetSettings(IMGPluginBase plugin) {
            IEnumerable<SettingInfo> infos = from p in plugin.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)
                                             where p.IsDefined<SettingsAttribute>()
                                             select new SettingInfo(plugin, p, p.GetCustomAttribute<SettingsAttribute>());

            return new SettingInfoCollection(plugin, infos);
        }
    }
}