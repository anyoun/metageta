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
using GalaSoft.MvvmLight;

#endregion

namespace MetaGeta.GUI {
	public abstract class NavigationTabGroupBase : ViewModelBase {
		public abstract string Caption { get; }
		public abstract ImageSource Icon { get; }
	}

	public class NamedNavigationTabGroup : NavigationTabGroupBase {
		private static readonly BitmapImage s_MetaGetaImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/MetaGeta_Image.png"));
		private readonly string m_Caption;

		public NamedNavigationTabGroup(string caption) {
			m_Caption = caption;
		}

		public override string Caption { get { return m_Caption; } }
		public override ImageSource Icon { get { return s_MetaGetaImage; } }
	}

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
				RaisePropertyChanged("Caption");
		}
	}
}