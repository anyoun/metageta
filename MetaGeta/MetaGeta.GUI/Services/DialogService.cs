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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Ninject.Core;
using MetaGeta.GUI.ViewModels;
using MetaGeta.GUI.Windows;

namespace MetaGeta.GUI.Services {
    internal class DialogService : IDialogService {
        private readonly Window m_ParentWindow;

        [Inject]
        public DialogService(Window parentWindow) {
            m_ParentWindow = parentWindow;
        }

        public bool? ShowDialog(IDialogViewModel viewModel, DialogType type, DialogButtons buttons) {
            var window = new DialogBox();
            window.Owner = m_ParentWindow;
            window.DataContext = viewModel;
            window.SetDialogButtons(buttons);

            if (type == DialogType.Modal) {
                return window.ShowDialog();
            } else {
                window.Show();
                return window.DialogResult;
            }
        }
    }

    internal class MockDialogService : IDialogService {
        [Inject]
        public MockDialogService(Window parentWindow) { }

        public bool? ShowDialog(IDialogViewModel viewModel, DialogType type, DialogButtons buttons) {
            return true;
        }
    }
}
