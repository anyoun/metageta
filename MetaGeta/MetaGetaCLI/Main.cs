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
using log4net.Config;
using MetaGeta.DataStore;
using System.Concurrency;

#endregion

namespace MetaGetaCLI {
    internal static class Program {
        public static void Main(string[] args) {
            XmlConfigurator.Configure();

			var scheduler = new NewThreadScheduler();
			var dsm = new DataStoreManager(false, scheduler);

            foreach (MGDataStore ds in dsm.DataStores) {
                Console.WriteLine("Refreshing {0}...", ds.Name);
                ds.BeginRefresh();
            }

            dsm.WaitForQueueToEmpty();

            Console.WriteLine("Done.");
            dsm.Shutdown();
        }
    }
}