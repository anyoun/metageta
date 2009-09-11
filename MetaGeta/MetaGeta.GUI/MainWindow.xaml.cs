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

#endregion

namespace MetaGeta.GUI {
    public partial class MainWindow {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private DataStoreManager m_DataStoreManager;
        private NavigationTabManager m_Navigation;

        public MainWindow() {
            m_DataStoreManager = new DataStoreManager(false);
            m_Navigation = new NavigationTabManager(m_DataStoreManager);

            InitializeComponent();

            PresentationTraceSources.SetTraceLevel(this, PresentationTraceLevel.High);

            DataContext = m_Navigation;
        }

        private void MainWindow_Closing(object sender, EventArgs e) {
            m_DataStoreManager.Shutdown();
        }

        private void btnNew_Click(Object sender, RoutedEventArgs e) {
            var win = new NewDataStoreWindow();
            PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High);
            win.Owner = this;
            bool? dr = win.ShowDialog();
            if (dr.HasValue && dr.Value) {
                DataStoreBuilder args = win.DataStoreCreationArguments;
                MGDataStore ds = m_DataStoreManager.NewDataStore(args.Name, args.Tempate);
                ds.SetCreationArguments(args);
            }
        }

        private void lbDataStores_PreviewMouseDown(Object sender, MouseButtonEventArgs e) {
            var originalItem = (DependencyObject) e.OriginalSource;
            DependencyObject nextTreeViewItem = lbDataStores;
            DependencyObject lastTreeViewItem = lbDataStores;
            while (nextTreeViewItem != null && nextTreeViewItem is ItemsControl) {
                lastTreeViewItem = nextTreeViewItem;
                nextTreeViewItem = (ItemsControl) ItemsControl.ContainerFromElement((ItemsControl) lastTreeViewItem, originalItem);
            }

            if (lastTreeViewItem != null) {
                if (((FrameworkElement) lastTreeViewItem).DataContext is NavigationTabGroupBase)
                    e.Handled = true;
                else
                    e.Handled = false;
            }
        }
    }
}