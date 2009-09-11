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

#endregion

namespace MetaGeta.DataStore {
    public class ProgressStatus : INotifyPropertyChanged {
        private double m_ProgressPct;
        private DateTime m_StartTime;

        private ProgressStatusState m_State = ProgressStatusState.NotStarted;
        private string m_StatusMessage;

        public double ProgressPct {
            get { return m_ProgressPct; }
            set {
                if (value != m_ProgressPct) {
                    m_ProgressPct = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("ProgressPct"));
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("TimeLeft"));
                }
            }
        }

        public TimeSpan TimeLeft {
            get { return Rate == 0 ? TimeSpan.Zero : new TimeSpan(0, 0, (int) ((1 - ProgressPct) / Rate)); }
        }

        public double Rate {
            get { return ProgressPct / (DateTime.Now - m_StartTime).TotalSeconds; }
        }

        public string StatusMessage {
            get { return m_StatusMessage; }
            set {
                if (value != m_StatusMessage) {
                    m_StatusMessage = value;
                    if (PropertyChanged != null)
                        PropertyChanged(this, new PropertyChangedEventArgs("StatusMessage"));
                }
            }
        }

        public string State {
            get { return m_State.ToString(); }
        }

        public bool IsRunning {
            get { return m_State == ProgressStatusState.Running; }
        }

        public bool IsDone {
            get { return m_State == ProgressStatusState.Done || m_State == ProgressStatusState.Done; }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        public void Start() {
            m_StartTime = DateTime.Now;
            m_State = ProgressStatusState.Running;
            OnStateChanged();
        }

        public void Done(bool succeeded) {
            m_State = succeeded ? ProgressStatusState.Done : ProgressStatusState.Failed;
            ProgressPct = 1;
            OnStateChanged();
        }

        private void OnStateChanged() {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("State"));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("IsRunning"));
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs("IsDone"));
        }
    }
}

namespace MetaGeta.DataStore {
    public enum ProgressStatusState {
        NotStarted,
        Running,
        Failed,
        Done
    }
}