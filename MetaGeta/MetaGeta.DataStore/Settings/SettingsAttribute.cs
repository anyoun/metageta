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
using System.Linq;
using System.Reflection;

#endregion

namespace MetaGeta.DataStore {
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class SettingsAttribute : Attribute {
        private readonly string m_Category;
        private readonly object m_DefaultValue;
        private readonly string m_FriendlyName;

        private readonly SettingType m_Type;

        public SettingsAttribute(string friendlyName, object defaultValue, SettingType type, string category) {
            m_FriendlyName = friendlyName;
            m_Type = type;
            m_Category = category;

            if (defaultValue.GetType().IsArray) {
                MethodInfo mi = typeof(Array).GetMethods().Where(x => x.Name == "AsReadOnly").Single();
                m_DefaultValue = mi.MakeGenericMethod(defaultValue.GetType().GetElementType()).Invoke(null, new[] { defaultValue });
            } else
                m_DefaultValue = defaultValue;
        }

        public string FriendlyName {
            get { return m_FriendlyName; }
        }

        public object DefaultValue {
            get { return m_DefaultValue; }
        }

        public SettingType Type {
            get { return m_Type; }
        }

        public string Category {
            get { return m_Category; }
        }
    }
}

namespace MetaGeta.DataStore {
    public enum SettingType {
        ShortText,
        LongText,
        Int,
        Float,
        Directory,
        DirectoryList,
        File,
        FileList,
        ExtensionList,
        Date
    }
}