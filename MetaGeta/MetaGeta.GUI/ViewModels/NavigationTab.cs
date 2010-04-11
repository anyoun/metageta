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
using System.Windows.Media;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Messaging;
using MetaGeta.DataStore;

#endregion

namespace MetaGeta.GUI {
	public abstract class NavigationTab : ViewModelBase {
		private readonly NavigationTabGroupBase m_Group;
		protected NavigationTab(NavigationTabGroupBase group, IMessenger messenger)
			: base(messenger) {
			m_Group = group;
		}
		public NavigationTabGroupBase Group { get { return m_Group; } }
		public abstract ImageSource Icon { get; }
		public abstract string Caption { get; }
	}

	//public class TabBadge : ViewModelBase {
	//    private readonly int m_Count;
	//    private readonly ProgressStatus m_Status;

	//    public TabBadge(int count, ProgressStatus status) {
	//        m_Count = count;
	//        m_Status = status;
	//    }

	//    public int Count { get { return m_Count; } }
	//    public double Progress { get { return m_Status.ProgressPct; } }
	//    public bool IsRunning { get { return m_Status.IsRunning; } }
	//}
}