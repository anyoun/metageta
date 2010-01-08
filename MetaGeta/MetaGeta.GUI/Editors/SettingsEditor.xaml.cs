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
using System.Windows;
using System.Windows.Controls;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI.Editors {
    public partial class SettingsEditor {
        public static DependencyProperty SettingsProperty = DependencyProperty.Register("Settings", typeof(SettingInfoCollection), typeof(SettingsEditor));

        public SettingsEditor()
            : base() {
            InitializeComponent();

            // Insert code required on object creation below this point.
        }

        [Category("Content")]
        public SettingInfoCollection Settings {
            get { return (SettingInfoCollection) GetValue(SettingsProperty); }
            set { SetValue(SettingsProperty, value); }
        }
    }
}

namespace MetaGeta.GUI {
    public class EditorTemplateSelector : DataTemplateSelector {
        private DataTemplate m_DirectoryListTemplate;
        private DataTemplate m_ExtensionListTemplate;

        private DataTemplate m_FileListTemplate;
        private DataTemplate m_StringTemplate;

        public DataTemplate StringTemplate {
            get { return m_StringTemplate; }
            set { m_StringTemplate = value; }
        }

        public DataTemplate ExtensionListTemplate {
            get { return m_ExtensionListTemplate; }
            set { m_ExtensionListTemplate = value; }
        }

        public DataTemplate DirectoryListTemplate {
            get { return m_DirectoryListTemplate; }
            set { m_DirectoryListTemplate = value; }
        }

        public DataTemplate FileListTemplate {
            get { return m_FileListTemplate; }
            set { m_FileListTemplate = value; }
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container) {
            switch (((SettingInfo) item).Metadata.Type) {
                case SettingType.Directory:
                case SettingType.ShortText:
                case SettingType.File:
                case SettingType.LongText:
                    return StringTemplate;

                case SettingType.ExtensionList:
                    return ExtensionListTemplate;

                case SettingType.DirectoryList:
                    return DirectoryListTemplate;

                case SettingType.FileList:
                    return FileListTemplate;

                case SettingType.Int:
                    return StringTemplate;

                case SettingType.Float:
                    return StringTemplate;

                case SettingType.Date:
                    return StringTemplate;

                default:
                    throw new Exception(string.Format("Unknown setting type: {0}.", ((SettingInfo) item).Metadata.Type));
            }
        }
    }
}