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
using Ninject;
using MetaGeta.GUI.ViewModels;
using System.Windows;
using MetaGeta.DataStore;
using MetaGeta.DataStore.Database;
using Ninject.Modules;

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
                    m_Kernel = new StandardKernel(new NinjectSettings(), new DesignTimeModule());
            }
        }

        public IDialogService DialogService { get { LazyInitialize(); return m_Kernel.Get<IDialogService>(); } }
        public Window MainWindow { get { LazyInitialize(); return (Window)m_Kernel.Get<Window>(); } }
        public ViewModelBase MainWindowViewModel { get { LazyInitialize(); return m_Kernel.Get<MainWindowViewModel>(); } }
        public DataStoreManager DataStoreManager { get { LazyInitialize(); return m_Kernel.Get<DataStoreManager>(); } }
    }

    public class RunTimeModule : NinjectModule {
        public override void Load() {
            Bind<DataMapper>().To<DataMapper>().InSingletonScope();
            Bind<IJobQueue>().To<JobQueue>().InSingletonScope();
            Bind<DataStoreManager>().To<DataStoreManager>().InSingletonScope();

            Bind<Window>().To<MainWindow>().InSingletonScope();
            Bind<IDialogService>().To<DialogService>().InSingletonScope();
            Bind<MainWindowViewModel>().To<MainWindowViewModel>().InSingletonScope();
        }
    }

    public class DesignTimeModule : NinjectModule {
        public override void Load() {
            Bind<DataMapper>().To<MockDataMapper>().InSingletonScope();
            Bind<IJobQueue>().To<MockJobQueue>().InSingletonScope();
            Bind<DataStoreManager>().To<DataStoreManager>().InSingletonScope();

            Bind<Window>().ToConstant(null);
            Bind<IDialogService>().To<MockDialogService>().InSingletonScope();
            Bind<MainWindowViewModel>().To<MainWindowViewModel>().InSingletonScope();
        }
    }
}
