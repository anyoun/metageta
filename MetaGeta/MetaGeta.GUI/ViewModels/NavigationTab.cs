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

#endregion

namespace MetaGeta.GUI {
    public abstract class NavigationTab : INotifyPropertyChanged {
        //Private m_Icon As ImageSource
        //Private m_Caption As String
        //Private m_Parent As NavigationTabGroupBase

        //Public Property Icon() As ImageSource
        //    Get
        //        Return m_Icon
        //    End Get
        //    Set(ByVal value As ImageSource)
        //        If m_Icon IsNot value Then
        //            m_Icon = value
        //            OnPropertyChanged("Icon")
        //        End If
        //    End Set
        //End Property

        //Public Property Caption() As String
        //    Get
        //        Return m_Caption
        //    End Get
        //    Set(ByVal value As String)
        //        If m_Caption IsNot value Then
        //            m_Caption = value
        //            OnPropertyChanged("Caption")
        //        End If
        //    End Set
        //End Property

        //Public Property Parent() As NavigationTabGroupBase
        //    Get
        //        Return m_Parent
        //    End Get
        //    Set(ByVal value As NavigationTabGroupBase)
        //        If m_Parent IsNot value Then
        //            m_Parent = value
        //            OnPropertyChanged("Parent")
        //        End If
        //    End Set
        //End Property
        public abstract ImageSource Icon { get; }
        public abstract string Caption { get; }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        protected void OnPropertyChanged(string name) {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(name));
        }
    }
}