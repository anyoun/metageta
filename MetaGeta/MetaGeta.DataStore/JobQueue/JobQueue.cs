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
using System.Reflection;
using System.Threading;
using log4net;

#endregion

namespace MetaGeta.DataStore {
    public class JobQueue : IDisposable, INotifyPropertyChanged {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Queue<Job> m_ActionWaitingQueue = new Queue<Job>();

        private readonly List<Job> m_AllActionItems = new List<Job>();
        private readonly Thread m_ProcessActionThread;

        private readonly ManualResetEvent m_QueueThreadIsSleeping = new ManualResetEvent(false);
        private readonly ManualResetEvent m_StopProcessingActions = new ManualResetEvent(false);

        public JobQueue(bool designMode) {
            if (!designMode) {
                m_ProcessActionThread = new Thread(ProcessActionQueueThread);
                m_ProcessActionThread.Start();
            }
        }

        public IEnumerable<Job> ActionItems {
            get {
                lock (m_ActionWaitingQueue) {
                    return m_AllActionItems.ToArray();
                }
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

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
                if (disposing)
                    CloseActionQueue();

                // TODO: free your own state (unmanaged objects).
                // TODO: set large fields to null.
            }
            disposedValue = true;
        }

        #endregion

        public void EnqueueAction(IAction action, MGDataStore dataStore, MGFile file) {
            lock (m_ActionWaitingQueue) {
                var item = new Job(action, dataStore, file);
                m_ActionWaitingQueue.Enqueue(item);
                Monitor.PulseAll(m_ActionWaitingQueue);
                m_AllActionItems.Add(item);
            }
            OnActionItemsChanged();
        }

        private void CloseActionQueue() {
            log.Info("Stopping action queue...");
            lock (m_ActionWaitingQueue) {
                m_ActionWaitingQueue.Enqueue(null);
                Monitor.Pulse(m_ActionWaitingQueue);
            }
            m_StopProcessingActions.Set();
            log.Info("Joining...");
            m_ProcessActionThread.Join();
            log.Info("Joined");
        }

        private void ProcessActionQueueThread() {
            Thread.CurrentThread.Name = "ProcessActionQueue";
            while (true) {
                if (m_StopProcessingActions.WaitOne(0, false))
                    return;

                Job nextItem = null;
                lock (m_ActionWaitingQueue) {
                    while (m_ActionWaitingQueue.Count == 0) {
                        m_QueueThreadIsSleeping.Set();
                        Monitor.Wait(m_ActionWaitingQueue);
                        m_QueueThreadIsSleeping.Reset();
                    }
                    nextItem = m_ActionWaitingQueue.Dequeue();
                }
                if (nextItem == null)
                    return;

                nextItem.Status.Start();
                try {
                    log.DebugFormat("Starting action \"{0}\" for file \"{1}\"...", nextItem.Action, nextItem.File != null ? nextItem.File.FileName : "");
                    nextItem.Action.Execute(nextItem.File, nextItem.Status);
                    nextItem.Status.Done(true);
                    nextItem.Status.StatusMessage = string.Empty;
                    log.DebugFormat("Action \"{0}\" for file \"{1}\" done.", nextItem.Action, nextItem.File != null ? nextItem.File.FileName : "");
                } catch (Exception ex) {
                    nextItem.Status.Done(false);
                    nextItem.Status.StatusMessage = ex.ToString();
                    log.ErrorFormat("Action \"{0}\" for file \"{1}\" failed with exception:\\n{2}", nextItem.Action, nextItem.File != null ? nextItem.File.FileName : "", ex);
                }
            }
        }

        public void WaitForQueueToEmpty() {
            while (true) {
                lock (m_ActionWaitingQueue) {
                    //Prevents race condition were worker hasn't started yet
                    if (m_ActionWaitingQueue.Count == 0)
                        break; // TODO: might not be correct. Was : Exit While
                }
                Thread.Sleep(100);
            }
            m_QueueThreadIsSleeping.WaitOne();
        }

        private void OnActionItemsChanged() {
            OnPropertyChanged("ActionItems");
        }

        private void OnPropertyChanged(string propertyName) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}