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
using System.Windows;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI {
    public partial class GlobalSettingsEditor : INotifyPropertyChanged {
        private readonly IList<GlobalSetting> m_Settings;

        private double m_MaxNameColumnWidth = 0;

        public GlobalSettingsEditor(IList<GlobalSetting> setting) : base() {
            InitializeComponent();

            m_Settings = setting;
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("SettingsList"));
        }

        public IList<GlobalSetting> SettingsList {
            get { return m_Settings; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void btnOK_Click(Object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }
    }
}