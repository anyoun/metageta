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
using System.Windows;
using System.Windows.Forms;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI {
    public partial class NewDataStoreWindow : INotifyPropertyChanged {
        private readonly DataStoreBuilder m_Arguments;

        public NewDataStoreWindow() : this(new DataStoreBuilder()) {}

        public NewDataStoreWindow(DataStoreBuilder arguments) {
            m_Arguments = arguments;
            //RaiseEvent PropertyChanged(Me, New PropertyChangedEventArgs("DataStoreBuilder"))

            // This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        public DataStoreBuilder DataStoreCreationArguments {
            get { return m_Arguments; }
        }

        public TemplateFinder TemplateFinder {
            get { return new TemplateFinder(); }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void btnOK_Click(Object sender, RoutedEventArgs e) {
            DialogResult = true;
            Close();
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e) {
            DialogResult = false;
            Close();
        }

        private void btnDirectory_Click(Object sender, RoutedEventArgs e) {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Select a folder to be monitored for files";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (tbDirectories.Text.Length != 0)
                    tbDirectories.Text += ";";
                tbDirectories.Text += fbd.SelectedPath;
            }
        }
    }
}