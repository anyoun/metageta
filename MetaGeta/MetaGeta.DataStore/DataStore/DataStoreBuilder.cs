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

using System.ComponentModel;
using System.IO;
using System.Linq;

#endregion

namespace MetaGeta.DataStore {
    public class DataStoreBuilder : INotifyPropertyChanged, IDataErrorInfo {
        private string m_Description;
        private string m_DirectoriesToWatch;

        private string m_Extensions;
        private string m_Name;
        private IDataStoreTemplate m_Template;

        public DataStoreBuilder() {
            m_Name = "TV Shows";
            m_DirectoriesToWatch = "E:\\ipod\\";
            m_Extensions = "mp4;avi;mkv";
            m_Template = new TemplateFinder().AvailableTemplates.First();
        }

        public string Name {
            get { return m_Name; }
            set {
                if (m_Name != value) {
                    m_Name = value;
                    OnPropertyChanged("Name");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        public string Description {
            get { return m_Description; }
            set {
                if (m_Description != value) {
                    m_Description = value;
                    OnPropertyChanged("Description");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        public IDataStoreTemplate Tempate {
            get { return m_Template; }
            set {
                if (!ReferenceEquals(m_Template, value)) {
                    m_Template = value;
                    OnPropertyChanged("Template");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        public string DirectoriesToWatch {
            get { return m_DirectoriesToWatch; }
            set {
                if (m_DirectoriesToWatch != value) {
                    m_DirectoriesToWatch = value;
                    OnPropertyChanged("DirectoriesToWatch");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        public string Extensions {
            get { return m_Extensions; }
            set {
                if (m_Extensions != value) {
                    m_Extensions = value;
                    OnPropertyChanged("Extensions");
                    OnPropertyChanged("IsValid");
                }
            }
        }

        private string[] DirectoriesArray {
            get { return DirectoriesToWatch.Split(';'); }
        }

        public bool IsValid {
            get { return this["Name"] == null && this["DirectoriesToWatch"] == null && this["Extensions"] == null; }
        }

        #region IDataErrorInfo Members

        public string Error {
            get { return null; }
        }

        public string this[string propertyName] {
            get {
                switch (propertyName) {
                    case "Name":
                        return string.IsNullOrEmpty(Name) ? "Please enter a Name." : null;
                    case "DirectoriesToWatch":
                        if (DirectoriesArray.Length < 1)
                            return "Please select at least one directory.";
                        string invalidDir = DirectoriesArray.FirstOrDefault(s => !Directory.Exists(s));
                        return invalidDir == null ? null : string.Format("Can't find directory \"{0}\".", invalidDir);
                    case "Extensions":
                        if (Extensions.Length < 1)
                            return "Please select at least one extension.";
                        else
                            return null;
                        break;
                    default:
                        return null;
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}