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
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace MetaGeta.GUI {
    public class ImportStatusViewModel : NavigationTab {
        private static readonly BitmapImage s_ViewImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/fileimport.png"));
        private readonly MGDataStore m_DataStore;

        private readonly RelayCommand m_ImportCommand;

        public ImportStatusViewModel(NavigationTabGroupBase group, IMessenger messenger, MGDataStore dataStore) : base(group, messenger) {
            m_DataStore = dataStore;
            m_DataStore.PropertyChanged += DataStorePropertyChanged;

            m_ImportCommand = new RelayCommand(Import);
        }

        public ICommand ImportCommand {
            get { return m_ImportCommand; }
        }

        public ImportStatus ImportStatus {
            get { return m_DataStore.ImportStatus; }
        }

        public override string Caption {
            get { return "Import"; }
        }

        public override ImageSource Icon {
            get { return s_ViewImage; }
        }

        private void DataStorePropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ImportStatus")
				RaisePropertyChanged("ImportStatus");
        }

        private void Import() {
            m_DataStore.BeginRefresh();
        }
    }
}