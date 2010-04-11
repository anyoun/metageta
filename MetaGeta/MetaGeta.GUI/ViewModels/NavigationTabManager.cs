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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using System.Windows.Data;
using System.Windows.Input;
using GalaSoft.MvvmLight.Command;

#endregion

namespace MetaGeta.GUI {
	public class NavigationTabManager : ViewModelBase {
		private static readonly BitmapImage s_ConfigureImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"));
		private readonly DataStoreManager m_DataStoreManager;

		private readonly NamedNavigationTabGroup m_MetaGetaTabGroup;
		private readonly ObservableCollection<NavigationTab> m_TabGroups = new ObservableCollection<NavigationTab>();
		private readonly ListCollectionView m_TabView;

		private NavigationTab m_SelectedTab;

		private readonly RelayCommand m_AddDataStoreCommand;
		private readonly RelayCommand<MGDataStore> m_RemoveDataStoreCommand;

		public NavigationTabManager(DataStoreManager dataStoreManager) {
			MessengerInstance = new Messenger();

			m_MetaGetaTabGroup = new NamedNavigationTabGroup("MetaGeta");
			m_TabGroups.Add(new NullViewModel(m_MetaGetaTabGroup, MessengerInstance, "Settings", s_ConfigureImage));

			m_DataStoreManager = dataStoreManager;
			m_DataStoreManager.DataStores.CollectionChanged += DataStoresChanged;
			m_TabGroups.Add(new JobQueueViewModel(m_MetaGetaTabGroup, MessengerInstance, m_DataStoreManager));
			AddTabs(dataStoreManager.DataStores);

			m_TabView = new ListCollectionView(m_TabGroups);
			m_TabView.GroupDescriptions.Add(new PropertyGroupDescription("Group"));

			m_AddDataStoreCommand = new RelayCommand(AddDataStoreCommand_Execute);
			m_RemoveDataStoreCommand = new RelayCommand<MGDataStore>(RemoveDataStoreCommand_Execute, RemoveDataStoreCommand_CanExecute);
		}

		public override void Cleanup() {
			base.Cleanup();

			m_DataStoreManager.Shutdown();
		}

		public CollectionView Tabs { get { return m_TabView; } }
		public NavigationTab SelectedTab {
			get { return m_SelectedTab; }
			set {
				if (m_SelectedTab != value) {
					m_SelectedTab = value;
					RaisePropertyChanged("SelectedTab");
					RaisePropertyChanged("SelectedTabIsDataStore");
				}
			}
		}
		public bool SelectedTabIsDataStore { get { return m_SelectedTab != null && m_SelectedTab.Group is DataStoreNavigationTabGroup; } }

		private void DataStoresChanged(object sender, NotifyCollectionChangedEventArgs e) {
			switch (e.Action) {
				case NotifyCollectionChangedAction.Reset:
					RemoveTabs(e.OldItems.Cast<MGDataStore>());
					AddTabs(e.NewItems.Cast<MGDataStore>());
					break;

				case NotifyCollectionChangedAction.Add:
					AddTabs(e.NewItems.Cast<MGDataStore>());
					break;

				case NotifyCollectionChangedAction.Move:
					RemoveTabs(e.OldItems.Cast<MGDataStore>());
					AddTabs(e.NewItems.Cast<MGDataStore>());
					break;

				case NotifyCollectionChangedAction.Remove:
					RemoveTabs(e.OldItems.Cast<MGDataStore>());
					break;

				case NotifyCollectionChangedAction.Replace:
					RemoveTabs(e.OldItems.Cast<MGDataStore>());
					AddTabs(e.NewItems.Cast<MGDataStore>());
					break;

				default:
					throw new Exception();
			}
			m_TabView.Refresh();
			RaisePropertyChanged("Tabs");
		}

		private void RemoveTabs(IEnumerable<MGDataStore> dataStores) {
			foreach (MGDataStore ds in dataStores)
				foreach (var tab in m_TabGroups.Where(tab => tab.Group is DataStoreNavigationTabGroup && ReferenceEquals(((DataStoreNavigationTabGroup)tab.Group).DataStore, ds)).ToArray())
					m_TabGroups.Remove(tab);
		}
		private void AddTabs(IEnumerable<MGDataStore> dataStores) {
			foreach (MGDataStore ds in dataStores)
				CreateTabGroup(ds);
		}

		private void CreateTabGroup(MGDataStore dataStore) {
			var grp = new DataStoreNavigationTabGroup(dataStore);

			m_TabGroups.Add(new GridViewModel(grp, MessengerInstance, dataStore));
			m_TabGroups.Add(new ImportStatusViewModel(grp, MessengerInstance, dataStore));
			m_TabGroups.Add(new TvShowViewModel(grp, MessengerInstance, dataStore));
		}

		public ICommand AddDataStoreCommand { get { return m_AddDataStoreCommand; } }
		public ICommand RemoveDataStoreCommand { get { return m_RemoveDataStoreCommand; } }

		private void AddDataStoreCommand_Execute() {
			m_DataStoreManager.NewDataStore("New Library", new TVShowDataStoreTemplate());
		}
		private void RemoveDataStoreCommand_Execute(MGDataStore ds) {
			ds.Delete();
		}
		private bool RemoveDataStoreCommand_CanExecute(MGDataStore ds) { return ds != null; }
	}

	public class DesignTimeNavigationTabManager : NavigationTabManager {
		public DesignTimeNavigationTabManager()
			: base(new DataStoreManager(true)) {
			//SelectedTab = Tabs.Cast<NavigationTab>().First(tab => tab.Group is DataStoreNavigationTabGroup);
			SelectedTab = Tabs.Cast<NavigationTab>().First(tab => tab is JobQueueViewModel);
		}
	}
}