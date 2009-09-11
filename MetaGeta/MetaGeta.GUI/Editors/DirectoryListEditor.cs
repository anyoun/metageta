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
using System.Windows.Forms;
using System.Windows.Interop;
using IWin32Window=System.Windows.Forms.IWin32Window;

#endregion

namespace MetaGeta.GUI.Editors {
    public class DirectoryListEditor : ItemListEditor {
        protected override string CreateItem() {
            var fbd = new FolderBrowserDialog();
            fbd.ShowNewFolderButton = true;
            var owner = new Win32Window(((HwndSource) PresentationSource.FromVisual(this)).Handle);
            if (fbd.ShowDialog(owner) != DialogResult.OK)
                return null;
            return fbd.SelectedPath;
        }

        #region Nested type: Win32Window

        private class Win32Window : IWin32Window {
            private readonly IntPtr m_Handle;

            public Win32Window(IntPtr handle) {
                m_Handle = handle;
            }

            #region IWin32Window Members

            public IntPtr Handle {
                get { return m_Handle; }
            }

            #endregion
        }

        #endregion
    }
}