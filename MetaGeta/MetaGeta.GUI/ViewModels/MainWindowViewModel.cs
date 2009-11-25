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
using Ninject.Core;
using MetaGeta.GUI.Services;

#endregion

namespace MetaGeta.GUI.ViewModels {
    public class MainWindowViewModel : ViewModelBase {
        private static readonly BitmapImage s_ConfigureImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/configure.png"));
        private readonly DataStoreManager m_DataStoreManager;

        private readonly NamedNavigationTabGroup m_MetaGetaTabGroup;
        private readonly ObservableCollection<NavigationTabGroupBase> m_TabGroups = new ObservableCollection<NavigationTabGroupBase>();

        [Inject]
        public MainWindowViewModel(DataStoreManager dataStoreManager) {
            m_MetaGetaTabGroup = new NamedNavigationTabGroup("MetaGeta");
            m_MetaGetaTabGroup.Children.Add(new NullViewModel("Settings", s_ConfigureImage));
            m_TabGroups.Add(m_MetaGetaTabGroup);

            m_DataStoreManager = dataStoreManager;
            m_DataStoreManager.DataStores.CollectionChanged += DataStoresChanged;
            m_MetaGetaTabGroup.Children.Add(new JobQueueViewModel(dataStoreManager));
            AddTabGroups(dataStoreManager.DataStores);

            m_NewDataStoreCommand = new RelayCommand(NewDataStoreCommand_Execute);
        }

        [Inject]
        public IDialogService DialogService { get; set; }

        public ObservableCollection<NavigationTabGroupBase> Tabs {
            get { return m_TabGroups; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void DataStoresChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Reset:
                    RemoveTabGroupForDataStore(e.OldItems.Cast<MGDataStore>());
                    AddTabGroups(e.NewItems.Cast<MGDataStore>());

                    break;
                case NotifyCollectionChangedAction.Add:
                    AddTabGroups(e.NewItems.Cast<MGDataStore>());

                    break;
                case NotifyCollectionChangedAction.Move:
                    RemoveTabGroupForDataStore(e.OldItems.Cast<MGDataStore>());
                    AddTabGroups(e.NewItems.Cast<MGDataStore>());

                    break;
                case NotifyCollectionChangedAction.Remove:
                    RemoveTabGroupForDataStore(e.OldItems.Cast<MGDataStore>());

                    break;
                case NotifyCollectionChangedAction.Replace:
                    RemoveTabGroupForDataStore(e.OldItems.Cast<MGDataStore>());
                    AddTabGroups(e.NewItems.Cast<MGDataStore>());

                    break;
                default:
                    throw new Exception();
            }
        }

        private void RemoveTabGroupForDataStore(IEnumerable<MGDataStore> dataStores) {
            foreach (MGDataStore ds in dataStores)
                m_TabGroups.Remove(m_TabGroups.First(tab => tab is DataStoreNavigationTabGroup && ReferenceEquals(((DataStoreNavigationTabGroup) tab).DataStore, ds)));
        }

        private void AddTabGroups(IEnumerable<MGDataStore> dataStores) {
            foreach (MGDataStore ds in dataStores)
                m_TabGroups.Add(CreateTabGroup(ds));
        }

        private NavigationTabGroupBase CreateTabGroup(MGDataStore dataStore) {
            var @group = new DataStoreNavigationTabGroup(dataStore);

            @group.Children.Add(new DataStoreConfigurationViewModel(dataStore));
            @group.Children.Add(new GridViewModel(dataStore));
            @group.Children.Add(new ImportStatusViewModel(dataStore));
            @group.Children.Add(new TvShowViewModel(dataStore));

            return @group;
        }

        private void OnTabsChanged() {
            OnPropertyChanged("Tabs");
        }

        #region Actions

        private readonly RelayCommand m_NewDataStoreCommand;

        public RelayCommand NewDataStoreCommand { get { return m_NewDataStoreCommand; } }
        private void NewDataStoreCommand_Execute() {
            var vm = new NewDataStoreWindowViewModel();
            bool? dr = DialogService.ShowDialog(vm, DialogType.Modal, DialogButtons.OkCancel);
            if (dr.HasValue && dr.Value) {
                DataStoreBuilder args = vm.DataStoreCreationArguments;
                MGDataStore ds = m_DataStoreManager.NewDataStore(args.Name, args.Tempate);
                ds.SetCreationArguments(args);
            }
        }

        #endregion
    }
}