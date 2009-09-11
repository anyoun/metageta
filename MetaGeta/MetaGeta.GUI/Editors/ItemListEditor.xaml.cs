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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using MetaGeta.Utilities;

#endregion

namespace MetaGeta.GUI.Editors {
    public abstract partial class ItemListEditor {
        private static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof (ReadOnlyCollection<string>), typeof (ItemListEditor));

        public ItemListEditor() {
            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
        }


        public ReadOnlyCollection<string> Items {
            get { return (ReadOnlyCollection<string>) GetValue(ItemsProperty); }
            set { SetValue(ItemsProperty, value); }
        }

        public DataTemplate ItemTemplate {
            get { return (DataTemplate) GetValue(ItemsControl.ItemTemplateProperty); }
            set { SetValue(ItemsControl.ItemTemplateProperty, value); }
        }

        protected abstract string CreateItem();

        private void btnAdd_Click(Object sender, RoutedEventArgs e) {
            string item = CreateItem();
            if (item != null)
                Items = new ReadOnlyCollection<string>(Items.Union(item.SingleToEnumerable()).ToArray());
        }

        private void btnRemove_Click(Object sender, RoutedEventArgs e) {
            Items = new ReadOnlyCollection<string>(Items.Where(s => !ReferenceEquals(s, ListBox1.SelectedItem)).ToArray());
        }
    }
}