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
namespace MetaGeta.DataStore {
    public class Job {
        private readonly string m_Action;
        private readonly MGDataStore m_DataStore;
        private readonly MGFile m_File;

        private readonly ProgressStatus m_Status = new ProgressStatus();

        public Job(string action, MGDataStore dataStore, MGFile file) {
            m_Action = action;
            m_DataStore = dataStore;
            m_File = file;
        }

        public string Action {
            get { return m_Action; }
        }

        public MGDataStore DataStore {
            get { return m_DataStore; }
        }

        public MGFile File {
            get { return m_File; }
        }

        public ProgressStatus Status {
            get { return m_Status; }
        }
    }
}