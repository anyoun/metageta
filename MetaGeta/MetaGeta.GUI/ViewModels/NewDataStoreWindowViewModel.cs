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
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using Ninject;
using System.Windows.Forms;
using MetaGeta.GUI.Services;
using System.Windows.Input;

#endregion

namespace MetaGeta.GUI.ViewModels {
    public class NewDataStoreWindowViewModel : ViewModelBase, IDialogViewModel {
        private readonly DataStoreBuilder m_Arguments;
        private readonly RelayCommand m_NewDirectoryCommand, m_OkCommand, m_CancelCommand;

        public NewDataStoreWindowViewModel() : this(new DataStoreBuilder()) { }

        public NewDataStoreWindowViewModel(DataStoreBuilder arguments) {
            m_Arguments = arguments;

            m_NewDirectoryCommand = new RelayCommand(NewDirectoryCommand_Execute);
            m_OkCommand = new RelayCommand(delegate { }, () => IsOkEnabled);
            m_CancelCommand = new RelayCommand(delegate { }, () => true);
        }

        public DataStoreBuilder DataStoreCreationArguments { get { return m_Arguments; } }
        public TemplateFinder TemplateFinder { get { return new TemplateFinder(); } }
        public bool IsOkEnabled { get { return m_Arguments.IsValid; } }

        public ICommand NewDirectoryCommand { get { return m_NewDirectoryCommand; } }
        private void NewDirectoryCommand_Execute() {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            fbd.Description = "Select a folder to be monitored for files";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK) {
                if (m_Arguments.DirectoriesToWatch.Length != 0)
                    m_Arguments.DirectoriesToWatch += ";";
                m_Arguments.DirectoriesToWatch += fbd.SelectedPath;
            }
        }

        public ICommand OkCommand { get { return m_OkCommand; } }
        public ICommand CancelCommand { get { return m_CancelCommand; } }
    }
}