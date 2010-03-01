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
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using TranscodePlugin;
using System.Linq;

#endregion

namespace MetaGeta.GUI {
    public class GridViewModel : NavigationTab {
        private static readonly BitmapImage s_ViewImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/view_detailed.png"));
        private readonly MGDataStore m_DataStore;

        private IList<MGFile> m_SelectedFiles;

        public GridViewModel(MGDataStore dataStore) {
            m_DataStore = dataStore;
            m_DataStore.PropertyChanged += DataStorePropertyChanged;

            m_ConvertToIphoneCommand = new RelayCommand<IList<MGFile>>(ConvertToIphone, IsNonEmpty, () => SelectedFiles);
            m_WriteMp4TagsCommand = new RelayCommand<IList<MGFile>>(WriteMp4Tags, IsNonEmpty, () => SelectedFiles);
            m_RemoveFileCommand = new RelayCommand<IList<MGFile>>(RemoveFile, IsNonEmpty, () => SelectedFiles);
        }

        public string[] ColumnNames {
            get { return m_DataStore.Template.GetColumnNames(); }
        }

        public IList<MGFile> Files {
            get { return m_DataStore.Files; }
        }

        public IList<MGFile> SelectedFiles {
            get { return m_SelectedFiles; }
            set {
                if (!ReferenceEquals(m_SelectedFiles, value)) {
                    m_SelectedFiles = value;
                    OnPropertyChanged("SelectedFiles");
                    OnPropertyChanged("SelectedFile");
                }
            }
        }

        public MGFile SelectedFile { get { return m_SelectedFiles.Count != 1 ? null : m_SelectedFiles.First(); } }

        public override string Caption {
            get { return "Grid"; }
        }

        public override ImageSource Icon {
            get { return s_ViewImage; }
        }

        #region "Commands"

        private readonly RelayCommand<IList<MGFile>> m_ConvertToIphoneCommand;
        private readonly RelayCommand<IList<MGFile>> m_RemoveFileCommand;

        private readonly RelayCommand<IList<MGFile>> m_WriteMp4TagsCommand;

        #region "Properties"

        public ICommand ConvertToIphoneCommand {
            get { return m_ConvertToIphoneCommand; }
        }

        public ICommand WriteMp4TagsCommand {
            get { return m_WriteMp4TagsCommand; }
        }

        public ICommand RemoveFileCommand {
            get { return m_RemoveFileCommand; }
        }

        #endregion

        #region "Execute"

        public void ConvertToIphone(IList<MGFile> param) {
            foreach (MGFile f in param)
                m_DataStore.DoAction(f, TranscodePlugin.TranscodePlugin.ConvertActionName);
        }

        public void WriteMp4Tags(IList<MGFile> param) {
            foreach (MGFile f in param)
                m_DataStore.DoAction(f, Mp4TagWriterPlugin.c_WriteTagsAction);
        }

        public void RemoveFile(IList<MGFile> param) {
            m_DataStore.RemoveFiles(param);
        }

        public void ShowProperties(IList<MGFile> param) {
            //Dim win As New FilePropertiesView(param.First())
            //PresentationTraceSources.SetTraceLevel(win, PresentationTraceLevel.High)
            //win.Owner = Window.GetWindow(Me)
            //win.ShowDialog()
        }

        #endregion

        #region "Command helper functions"

        private static bool IsNonEmpty(IList<MGFile> files) {
            return files != null && files.Count > 0;
        }

        private static bool IsExactlyOne(IList<MGFile> files) {
            return files != null && files.Count == 1;
        }

        #endregion

        #endregion

        private void DataStorePropertyChanged(object sender, PropertyChangedEventArgs e) {
            if (e.PropertyName == "Files")
                OnPropertyChanged("Files");
        }
    }
}