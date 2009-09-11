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
using System.IO;

#endregion

namespace MetaGeta.DataStore {
    public class GlobalSetting : INotifyPropertyChanged, IDataErrorInfo {
        private readonly string m_DefaultValue;
        private readonly string m_Name;

        private readonly GlobalSettingType m_Type;

        private string m_Value;

        public GlobalSetting(string name, string value, string defaultValue, GlobalSettingType type) {
            m_Name = name;

            m_DefaultValue = defaultValue;
            m_Type = type;

            if (value == m_DefaultValue)
                m_Value = null;
            else
                m_Value = value;
        }

        public string Name {
            get { return m_Name; }
        }

        public string DefaultValue {
            get { return m_DefaultValue; }
        }

        public GlobalSettingType Type {
            get { return m_Type; }
        }

        public string Value {
            get {
                if (m_Value == null)
                    return m_DefaultValue;
                else
                    return m_Value;
            }
            set {
                if (value != m_Value) {
                    if (value == m_DefaultValue)
                        m_Value = null;
                    else
                        m_Value = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("Value"));
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("IsDefault"));
                }
            }
        }

        public string ValueOrNull {
            get { return m_Value; }
        }

        public bool IsDefault {
            get { return m_Value == null; }
        }

        public bool IsValid {
            get { return this["Value"] == null; }
        }

        #region IDataErrorInfo Members

        public string Error {
            get { return null; }
        }

        public string this[string propertyName] {
            get {
                if (propertyName != "Value")
                    return null;

                switch (m_Type) {
                    case GlobalSettingType.Number:
                        double v = 0;
                        if (double.TryParse(Value, out v))
                            return null;
                        else
                            return string.Format("\"{0}\" is not a number.", Value);
                        break;
                    case GlobalSettingType.Directory:
                        return Directory.Exists(Environment.ExpandEnvironmentVariables(Value)) ? null : string.Format("Can't find directory \"{0}\".", Value);
                    case GlobalSettingType.File:
                        return File.Exists(Environment.ExpandEnvironmentVariables(Value)) ? null : string.Format("Can't find file \"{0}\".", Value);
                    case GlobalSettingType.LongText:
                        return null;
                    case GlobalSettingType.ShortText:
                        return null;
                    default:
                        throw new Exception();
                }
            }
        }

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion
    }
}

namespace MetaGeta.DataStore {
    public enum GlobalSettingType {
        ShortText,
        LongText,
        Number,
        Directory,
        File
    }
}