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
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using log4net;
using MetaGeta.DataStore;
using GalaSoft.MvvmLight.Messaging;
using MetaGeta.GUI.Services;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

#endregion

namespace MetaGeta.GUI {
	public partial class MainWindow {
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private DataStoreManager m_DataStoreManager;
		private NavigationTabManager m_Navigation;
		private readonly Messenger m_Messenger;

		public MainWindow() {
			m_DataStoreManager = new DataStoreManager(false);

			m_Messenger = new Messenger();
			m_Messenger.Register<DirectoryPromptMessage>(this, ProcessDirectoryPrompt);
			m_Messenger.Register<InputPromptMessage>(this, ProcessInputPrompt);

			m_Navigation = new NavigationTabManager(m_Messenger, m_DataStoreManager);

			InitializeComponent();

			DataContext = m_Navigation;
		}

		private void ProcessDirectoryPrompt(DirectoryPromptMessage message) {
			var cfd = new CommonOpenFileDialog();
			cfd.IsFolderPicker = true;
			cfd.EnsurePathExists = true;
			cfd.EnsureValidNames = true;
			cfd.Multiselect = false;
			cfd.Title = message.Title;
			if (cfd.ShowDialog() == CommonFileDialogResult.OK) {
				message.Callback(cfd.FileName);
			} else {
				message.Callback(null);
			}
		}
		private void ProcessInputPrompt(InputPromptMessage message) {
			var prompt = new InputDialog();
			string text = string.Empty;
			if (prompt.ShowDialog(message.Title, message.Message, ref text) ?? false)
				message.Callback(text);
			else
				message.Callback(null);
		}

		private void MainWindow_Closing(object sender, EventArgs e) {
			m_Navigation.Cleanup();
		}
	}
}