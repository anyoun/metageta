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
using System.Windows;

#endregion

namespace MetaGeta.GUI.Editors {
    public partial class StringInputPrompt {
        private readonly string m_LabelText;

        private string m_EditingString;

        public StringInputPrompt(string caption, string prompt) {
            m_LabelText = prompt;

            InitializeComponent();

            Title = caption;
        }

        public string LabelText {
            get { return m_LabelText; }
        }

        public string EditingString {
            get { return m_EditingString; }
            set { m_EditingString = value; }
        }

        private void btnOK_Click(Object sender, RoutedEventArgs e) {
            DialogResult = true;
        }

        private void btnCancel_Click(Object sender, RoutedEventArgs e) {
            DialogResult = false;
        }
    }
}