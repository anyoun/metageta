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
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI {
    public class DataStoreConfigurationViewModel : NavigationTab {
        private static readonly BitmapImage s_RunImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/run.png"));
        private readonly MGDataStore m_DataStore;
        private readonly RelayCommand m_DeleteCommand;

        private IMGPluginBase m_SelectedPlugin;

        public DataStoreConfigurationViewModel(MGDataStore dataStore) {
            m_DataStore = dataStore;
            m_DataStore.PropertyChanged += DataStore_PropertyChanged;

            m_DeleteCommand = new RelayCommand(Delete);
        }


        public ICommand DeleteCommand {
            get { return m_DeleteCommand; }
        }

        public string Name {
            get { return m_DataStore.Name; }
            set { m_DataStore.Name = value; }
        }

        public string Description {
            get { return m_DataStore.Description; }
            set { m_DataStore.Description = value; }
        }

        public IEnumerable<IMGPluginBase> Plugins {
            get { return m_DataStore.Plugins; }
        }

        public IMGPluginBase SelectedPlugin {
            get { return m_SelectedPlugin; }
            set {
                if (!ReferenceEquals(m_SelectedPlugin, value)) {
                    m_SelectedPlugin = value;
                    OnPropertyChanged("SelectedPlugin");
                    OnPropertyChanged("SelectedPluginSettings");
                }
            }
        }

        public SettingInfoCollection SelectedPluginSettings {
            get {
                if (SelectedPlugin == null)
                    return null;
                else
                    return SettingInfoCollection.GetSettings(SelectedPlugin);
            }
        }

        public override string Caption {
            get { return "Configuration"; }
        }

        public override ImageSource Icon {
            get { return s_RunImage; }
        }

        private void Delete() {
            m_DataStore.Delete();
        }

        private void DataStore_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Name")
                OnPropertyChanged("Name");
            else if (e.PropertyName == "Description")
                OnPropertyChanged("Description");
        }
    }
}