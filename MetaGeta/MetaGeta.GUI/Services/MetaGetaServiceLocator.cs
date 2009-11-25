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
using Ninject.Core;
using MetaGeta.GUI.ViewModels;
using System.Windows;
using MetaGeta.DataStore;
using MetaGeta.DataStore.Database;
using Ninject.Core.Behavior;

namespace MetaGeta.GUI.Services {
    public class MetaGetaServiceLocator {
        private static IKernel m_Kernel;
        private static object m_KernelLock = new object();

        public static void InitializeRunTime() {
            lock (m_KernelLock) {
                if (m_Kernel != null)
                    throw new InvalidOperationException("Kernel has already been initialized.");
                m_Kernel = new StandardKernel(new RunTimeModule());
            }
        }

        private static void LazyInitialize() {
            lock (m_KernelLock) {
                if (m_Kernel == null)
                    m_Kernel = new StandardKernel(new DesignTimeModule());
            }
        }

        public IDialogService DialogService { get { LazyInitialize(); return m_Kernel.Get<IDialogService>(); } }
        public Window MainWindow { get { LazyInitialize(); return (Window)m_Kernel.Get<Window>(); } }
        public ViewModelBase MainWindowViewModel { get { LazyInitialize(); return m_Kernel.Get<MainWindowViewModel>(); } }
        public DataStoreManager DataStoreManager { get { LazyInitialize(); return m_Kernel.Get<DataStoreManager>(); } }
    }

    public class RunTimeModule : StandardModule {
        public override void Load() {
            Bind<DataMapper>().To<DataMapper>().Using<SingletonBehavior>();
            Bind<IJobQueue>().To<JobQueue>().Using<SingletonBehavior>();
            Bind<DataStoreManager>().To<DataStoreManager>().Using<SingletonBehavior>();

            Bind<Window>().To<MainWindow>().Using<SingletonBehavior>();
            Bind<IDialogService>().To<DialogService>().Using<SingletonBehavior>();
            Bind<MainWindowViewModel>().To<MainWindowViewModel>().Using<SingletonBehavior>();
        }
    }

    public class DesignTimeModule : StandardModule {
        public override void Load() {
            Bind<DataMapper>().To<MockDataMapper>().Using<SingletonBehavior>();
            Bind<IJobQueue>().To<MockJobQueue>().Using<SingletonBehavior>();
            Bind<DataStoreManager>().To<DataStoreManager>().Using<SingletonBehavior>();

            Bind<Window>().ToConstant<MainWindow>(null);
            Bind<IDialogService>().To<MockDialogService>().Using<SingletonBehavior>();
            Bind<MainWindowViewModel>().To<MainWindowViewModel>().Using<SingletonBehavior>();
        }
    }
}
