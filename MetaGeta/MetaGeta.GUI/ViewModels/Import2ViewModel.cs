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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Media;
using System.Collections.ObjectModel;
using MetaGeta.GUI.Services;
using MetaGeta.Utilities;

namespace MetaGeta.GUI {
	public class Import2ViewModel : NavigationTab {
		private static readonly BitmapImage s_ViewImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/fileimport.png"));

		private readonly MGDataStore m_DataStore;

		private readonly SettingInfoCollection m_Settings;

		private readonly RelayCommand m_ImportCommand, m_AddDirectoryCommand, m_AddExtensionCommand;
		private readonly RelayCommand<string> m_RemoveDirectoryCommand, m_RemoveExtensionCommand;

		public Import2ViewModel(NavigationTabGroupBase group, IMessenger messenger, MGDataStore dataStore)
			: base(group, messenger) {
			m_DataStore = dataStore;

			if (m_DataStore != null) {
				m_DataStore.PropertyChanged += m_DataStore_PropertyChanged;
				m_Settings = SettingInfoCollection.GetSettings((IMGPluginBase)m_DataStore.FileSourcePlugins.Single());
			}

			m_ImportCommand = new RelayCommand(ImportCommand_Execute);
			m_AddDirectoryCommand = new RelayCommand(AddDirectoryCommand_Execute);
			m_AddExtensionCommand = new RelayCommand(AddExtensionCommand_Execute);
			m_RemoveDirectoryCommand = new RelayCommand<string>(RemoveDirectoryCommand_Execute, s => s != null);
			m_RemoveExtensionCommand = new RelayCommand<string>(RemoveExtensionCommand_Execute, s => s != null);
		}

		void m_DataStore_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "ImportStatus")
				RaisePropertyChanged("ImportStatus");
		}

		public virtual ImportStatus ImportStatus { get { return m_DataStore.ImportStatus; } }
		public override string Caption { get { return "Import2"; } }
		public override ImageSource Icon { get { return s_ViewImage; } }

		public virtual IList<string> Directories {
			get {
				return (ReadOnlyCollection<string>)m_Settings.GetSetting("DirectoriesToWatch").Value;
			}
			set {
				m_Settings.GetSetting("DirectoriesToWatch").Value = new ReadOnlyCollection<string>(value);
				RaisePropertyChanged("Directories");
			}
		}
		public virtual IList<string> Extensions {
			get {
				return (ReadOnlyCollection<string>)m_Settings.GetSetting("Extensions").Value;
			}
			set {
				m_Settings.GetSetting("Extensions").Value = new ReadOnlyCollection<string>(value);
				RaisePropertyChanged("Extensions");
			}
		}

		private void DataStorePropertyChanged(object sender, PropertyChangedEventArgs e) {
			if (e.PropertyName == "ImportStatus")
				RaisePropertyChanged("ImportStatus");
		}

		public ICommand ImportCommand { get { return m_ImportCommand; } }
		private void ImportCommand_Execute() {
			m_DataStore.BeginRefresh();
		}

		public ICommand AddDirectoryCommand { get { return m_AddDirectoryCommand; } }
		private void AddDirectoryCommand_Execute() {
			MessengerInstance.Send(new DirectoryPromptMessage("Add Directory", "message", dir => {
				if (dir != null)
					Directories = Directories.Cons(dir).ToArray();
			}));
		}

		public ICommand RemoveDirectoryCommand { get { return m_RemoveDirectoryCommand; } }
		private void RemoveDirectoryCommand_Execute(string directory) {
			Directories = Directories.Except(directory).ToArray();
		}

		public ICommand AddExtensionCommand { get { return m_AddExtensionCommand; } }
		private void AddExtensionCommand_Execute() {
			MessengerInstance.Send(new InputPromptMessage("Add Extension", "Enter a new file extension:", ext => {
				if (ext != null)
					Extensions = Extensions.Cons(ext).ToArray();
			}));
		}

		public ICommand RemoveExtensionCommand { get { return m_RemoveExtensionCommand; } }
		private void RemoveExtensionCommand_Execute(string extension) {
			Extensions = Extensions.Except(extension).ToArray();
		}
	}

	public class DesignTimeImport2ViewModel : Import2ViewModel {
		public DesignTimeImport2ViewModel()
			: base(null, null, null) { }
		public override IList<string> Directories {
			get { return new[] { @"c:\foo\bar\", @"c:\foo\", @"e:\bar\" }; }
			set { }
		}
		public override IList<string> Extensions {
			get { return new[] { "mp4", "avi", "mkv", "ogm" }; }
			set { }
		}

		public override ImportStatus ImportStatus { get { return new ImportStatus("Import status message", 30); } }
	}
}
