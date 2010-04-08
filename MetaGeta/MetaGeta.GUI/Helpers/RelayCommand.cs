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
using System.Windows.Input;

#endregion

namespace MetaGeta.GUI {
    public class RelayCommand<T> : ICommand {
        private readonly Predicate<T> m_CanExecute;
        private readonly Action<T> m_Execute;
        private readonly Func<T> m_GetParameter;

        public RelayCommand(Action<T> execute) : this(execute, null) { }
        public RelayCommand(Action<T> execute, Predicate<T> canExecute) : this(execute, canExecute, null) { }
        public RelayCommand(Action<T> execute, Predicate<T> canExecute, Func<T> getParameter) {
            m_Execute = execute;
            m_CanExecute = canExecute;
            m_GetParameter = getParameter;
        }

        #region ICommand Members

        public bool CanExecute(object parameter) {
            parameter = m_GetParameter == null ? parameter : m_GetParameter();
            return m_CanExecute == null ? true : m_CanExecute((T) parameter);
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) {
            parameter = m_GetParameter == null ? parameter : m_GetParameter();
            m_Execute((T) parameter);
        }

        #endregion
    }
}

namespace MetaGeta.GUI {
    public class RelayCommand : ICommand {
        private readonly Func<bool> m_CanExecute;
        private readonly Action m_Execute;

        public RelayCommand(Action execute) : this(execute, null) { }

        public RelayCommand(Action execute, Func<bool> canExecute) {
            m_Execute = execute;
            m_CanExecute = canExecute;
        }

        #region ICommand Members

        public bool CanExecute(object parameter) {
            return m_CanExecute == null ? true : m_CanExecute();
        }

        public event EventHandler CanExecuteChanged {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter) {
            m_Execute();
        }

        #endregion
    }
}