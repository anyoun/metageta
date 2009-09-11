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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

#endregion

namespace MetaGeta.DataStore {
    public class FileProgress : INotifyPropertyChanged, IProgressReportCallback<MGFile> {
        private readonly object m_Lock = new object();
        private readonly string m_ProcedureName;
        private IList<MGFile> m_AllFiles;
        private int m_CurrentIndex;
        private double m_CurrentItemPercent;

        public FileProgress(string procedureName) {
            m_ProcedureName = procedureName;

            m_AllFiles = null;
            m_CurrentIndex = -1;

            OnCompletedItemsChanged();
            OnCurrentItemChanged();
            OnIsDoneChanged();
            OnPercentDoneChanged();
        }

        public IEnumerable<MGFile> AllItems {
            get {
                lock (m_Lock) {
                    return m_AllFiles;
                }
            }
        }

        public IEnumerable CompletedItems {
            get {
                lock (m_Lock) {
                    return m_AllFiles.Take(m_CurrentIndex);
                }
            }
        }

        public double TotalPercentDone {
            get {
                lock (m_Lock) {
                    if (m_CurrentIndex == m_AllFiles.Count)
                        return 1;
                    else
                        return (double) m_CurrentIndex / m_AllFiles.Count;
                }
            }
        }

        public bool IsDone {
            get {
                lock (m_Lock) {
                    return m_CurrentIndex == m_AllFiles.Count;
                }
            }
        }

        public object CurrentItem {
            get {
                lock (m_Lock) {
                    if (m_CurrentIndex == m_AllFiles.Count)
                        return null;
                    else
                        return m_AllFiles[m_CurrentIndex];
                }
            }
        }

        public double CurrentItemPercentDone {
            get {
                lock (m_Lock) {
                    if (m_CurrentIndex == m_AllFiles.Count)
                        return 1;
                    else
                        return m_CurrentItemPercent;
                }
            }
        }

        public string ProcedureName {
            get { return m_ProcedureName; }
        }

        #region "IProgressReporter"

        public void SetCurrentItem(int index) {
            bool done = false;
            lock (m_Lock) {
                m_CurrentIndex = index;
                if (m_CurrentIndex == m_AllFiles.Count)
                    done = true;
            }
            OnCurrentItemChanged();
            OnCompletedItemsChanged();
            OnPercentDoneChanged();
            if (done)
                OnIsDoneChanged();
        }

        public void SetItemProgress(double percent) {
            lock (m_Lock) {
                m_CurrentItemPercent = percent;
            }
            OnPercentDoneChanged();
        }

        public void SetItems(IEnumerable<MGFile> items) {
            bool done = false;
            lock (m_Lock) {
                m_AllFiles = items.ToList();
                if (m_AllFiles.Count == 0)
                    done = true;
            }
            OnCurrentItemChanged();
            OnCompletedItemsChanged();
            if (done)
                OnIsDoneChanged();
        }

        #endregion

        #region "Property changes"

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnCompletedItemsChanged() {
            OnPropertyChanged("CompletedItems");
        }

        private void OnIsDoneChanged() {
            OnPropertyChanged("IsDone");
        }

        private void OnCurrentItemChanged() {
            OnPropertyChanged("CurrentItem");
        }

        private void OnPercentDoneChanged() {
            OnPropertyChanged("PercentDone");
        }

        private void OnPropertyChanged(string name) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }

        #endregion
    }
}

namespace MetaGeta.DataStore {
    public class ProgressHelper : IEnumerable<MGFile> {
        private readonly List<MGFile> m_Files;

        private readonly IProgressReportCallback<MGFile> m_Reporter;

        public ProgressHelper(IProgressReportCallback<MGFile> reporter, IEnumerable<MGFile> files) {
            m_Reporter = reporter;
            m_Files = new List<MGFile>(files);
        }

        #region IEnumerable<MGFile> Members

        public IEnumerator<MGFile> GetEnumerator() {
            return new ProgressHelperEnumerator(m_Reporter, m_Files);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator1();
        }

        #endregion

        public IEnumerator GetEnumerator1() {
            return GetEnumerator();
        }

        #region Nested type: ProgressHelperEnumerator

        private class ProgressHelperEnumerator : IEnumerator<MGFile> {
            private readonly List<MGFile> m_Files;

            private readonly IProgressReportCallback<MGFile> m_Reporter;
            private int m_CurrentIndex;

            public ProgressHelperEnumerator(IProgressReportCallback<MGFile> reporter, List<MGFile> files) {
                m_Reporter = reporter;
                m_Files = files;
                m_CurrentIndex = -1;

                reporter.SetItems(m_Files);
                reporter.SetCurrentItem(-1);
            }

            #region IEnumerator<MGFile> Members

            public MGFile Current {
                get { return m_Files[m_CurrentIndex]; }
            }

            object IEnumerator.Current {
                get { return Current; }
            }

            public bool MoveNext() {
                m_CurrentIndex += 1;
                m_Reporter.SetCurrentItem(m_CurrentIndex);
                return m_CurrentIndex < m_Files.Count;
            }

            public void Reset() {
                m_CurrentIndex = -1;
            }

            #endregion

            #region " IDisposable Support "

            // To detect redundant calls
            private bool disposedValue = false;

            // IDisposable

            // This code added by Visual Basic to correctly implement the disposable pattern.
            public void Dispose() {
                // Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing) {
                if (!disposedValue) {
                    if (disposing) {
                        // TODO: free other state (managed objects).
                    }

                    // TODO: free your own state (unmanaged objects).
                    // TODO: set large fields to null.
                }
                disposedValue = true;
            }

            #endregion
        }

        #endregion
    }
}