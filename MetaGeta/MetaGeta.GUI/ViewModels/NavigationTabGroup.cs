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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using Ninject;
using Ninject.Modules;

#endregion

namespace MetaGeta.GUI {
    public abstract class NavigationTabGroupBase : NavigationTab {
        private readonly IKernel m_Kernel;

        private readonly ObservableCollection<NavigationTab> m_Children = new ObservableCollection<NavigationTab>();

        private NavigationTab m_SelectedChild;

        public NavigationTabGroupBase() {
            m_Kernel = new StandardKernel(CreateModule());
        }

        public ObservableCollection<NavigationTab> Children {
            get { return m_Children; }
        }

        public NavigationTab SelectedChild {
            get { return m_SelectedChild; }
            set {
                if (!ReferenceEquals(m_SelectedChild, value)) {
                    m_SelectedChild = value;
                    OnPropertyChanged("SelectedChild");
                }
            }
        }

        protected abstract INinjectModule CreateModule();
    }
}

namespace MetaGeta.GUI {
    public class NamedNavigationTabGroup : NavigationTabGroupBase {
        private static readonly BitmapImage s_MetaGetaImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/MetaGeta_Image.png"));
        private readonly string m_Caption;

        public NamedNavigationTabGroup(string caption) {
            m_Caption = caption;
        }

        public override string Caption { get { return m_Caption; } }
        public override ImageSource Icon { get { return s_MetaGetaImage; } }

        protected override INinjectModule CreateModule() { return null; }
    }
}

namespace MetaGeta.GUI {
    public class DataStoreNavigationTabGroup : NavigationTabGroupBase {
        private static readonly BitmapImage s_DatabaseImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/db.png"));
        private readonly MGDataStore m_DataStore;

        public DataStoreNavigationTabGroup(MGDataStore dataStore) {
            m_DataStore = dataStore;
            m_DataStore.PropertyChanged += DataStore_PropertyChanged;
        }

        public MGDataStore DataStore { get { return m_DataStore; } }

        public override string Caption { get { return m_DataStore.Name; } }

        public override ImageSource Icon { get { return s_DatabaseImage; } }

        private void DataStore_PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Name")
                OnPropertyChanged("Caption");
        }

        protected override INinjectModule CreateModule() { return new DataStoreNavigationModule(); }

        private class DataStoreNavigationModule : NinjectModule {
            public override void Load() {
                Bind<NavigationTab>().To<
                Bind<DataMapper>().To<DataMapper>().InSingletonScope();
                Bind<IJobQueue>().To<JobQueue>().InSingletonScope();
                Bind<DataStoreManager>().To<DataStoreManager>().InSingletonScope();

                Bind<Window>().To<MainWindow>().InSingletonScope();
                Bind<IDialogService>().To<DialogService>().InSingletonScope();
                Bind<MainWindowViewModel>().To<MainWindowViewModel>().InSingletonScope();
            }
        }
    }
}