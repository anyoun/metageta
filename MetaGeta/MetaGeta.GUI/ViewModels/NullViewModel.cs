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

using System.Windows.Media;

#endregion

namespace MetaGeta.GUI {
    public class NullViewModel : NavigationTab {
        private readonly string m_Caption;

        private readonly ImageSource m_Icon;

        public NullViewModel(string caption, ImageSource icon) {
            m_Caption = caption;
            m_Icon = icon;
        }

        public override string Caption {
            get { return m_Caption; }
        }

        public override ImageSource Icon {
            get { return m_Icon; }
        }
    }
}