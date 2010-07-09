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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using log4net;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI {
    public partial class GridView : UserControl {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private GridViewModel m_ViewModel;

        public GridView() {
            // This call is required by the Windows Form Designer.
            InitializeComponent();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) {
            base.OnPropertyChanged(e);

            if (ReferenceEquals(e.Property, DataContextProperty)) {
                if (m_ViewModel != null)
                    m_ViewModel.PropertyChanged -= ViewModelPropertyChanged;
                m_ViewModel = (GridViewModel) e.NewValue;

                if (m_ViewModel != null) {
                    m_ViewModel.PropertyChanged += ViewModelPropertyChanged;
                    CopyColumnDefinitionsToGrid();
                }
            }
        }


        private void CopyColumnDefinitionsToGrid() {
            foreach (string t in m_ViewModel.ColumnNames) {
                var column = new DataGridTextColumn();
                column.Binding = new Binding {
                                                 Converter = new MGFileConverter(),
                                                 ConverterParameter = t,
                                                 Mode = BindingMode.OneWay
                                             };
                column.Header = t;
                DataGrid1.Columns.Add(column);
            }
        }

        private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "ColumnNames")
                CopyColumnDefinitionsToGrid();
        }

        private void DataGrid1_SelectedCellsChanged(Object sender, SelectedCellsChangedEventArgs e) {
            if (m_ViewModel != null)
                m_ViewModel.SelectedFiles = (from cell in DataGrid1.SelectedCells where cell.Item is MGFile select (MGFile) cell.Item).Distinct().ToList();
        }
    }
}