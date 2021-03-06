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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MetaGeta.DataStore;
using GalaSoft.MvvmLight.Messaging;

#endregion

namespace MetaGeta.GUI {
	public class JobQueueViewModel : NavigationTab {
		private static readonly BitmapImage s_ConfigureImage = new BitmapImage(new Uri("pack://application:,,,/MetaGeta.GUI;component/Resources/text_block.png"));
		private readonly DataStoreManager m_DataStoreManager;

		public JobQueueViewModel(NavigationTabGroupBase group, IMessenger messenger, DataStoreManager dataStoreManager)
			: base(group, messenger) {
			m_DataStoreManager = dataStoreManager;
			m_DataStoreManager.JobQueue.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(JobQueue_PropertyChanged);
		}

		void JobQueue_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) {
			if (e.PropertyName == "NotDoneCount") {
				RaisePropertyChanged("NotDoneCount");
			} else if (e.PropertyName == "CurrentItem") {
				RaisePropertyChanged("CurrentProgress");
			}
		}

		public JobQueue JobQueue { get { return m_DataStoreManager.JobQueue; } }
		public override string Caption { get { return "Jobs"; } }
		public override ImageSource Icon { get { return s_ConfigureImage; } }

		public int NotDoneCount { get { return JobQueue.NotDoneCount; } }
		public double CurrentProgress { get { return JobQueue.CurrentItem == null ? 1 : JobQueue.CurrentItem.Status.ProgressPct; } }
	}
}