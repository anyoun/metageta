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

#endregion

namespace MetaGeta.DataStore {
    public interface IDataStoreTemplate {
        string[] GetDimensionNames();
        string[] GetColumnNames();
        string[] GetPluginTypeNames();
        string GetName();
    }
}

namespace MetaGeta.DataStore {
    public class TemplateFinder {
        private static Dictionary<string, Type> s_CachedTemplates;

        public IEnumerable<IDataStoreTemplate> AvailableTemplates {
            get { return new IDataStoreTemplate[] {new TVShowDataStoreTemplate()}; }
        }

        public static IDataStoreTemplate GetTemplateByName(string name) {
            if (s_CachedTemplates == null) {
                //Hard-coded for now, Unity later?
                s_CachedTemplates = new Dictionary<string, Type>();
                s_CachedTemplates.Add("TVShow", typeof (TVShowDataStoreTemplate));
            }
            object o = Activator.CreateInstance(s_CachedTemplates[name]);
            return (IDataStoreTemplate) o;
        }
    }
}