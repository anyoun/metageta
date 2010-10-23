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
using System.Collections.Generic;
using System.Threading;

#endregion

namespace MetaGeta.DataStore {
	public interface IMGFileSourcePlugin {
		void Refresh(CancellationToken cancel);
		IObservable<FileAddedEventArgs> FileAddedEvent { get; }
		IObservable<FileModifiedEventArgs> FileModifiedEvent { get; }
		IObservable<FileMovedEventArgs> FileMovedEvent { get; }
		IObservable<FileDeletedEventArgs> FileDeleteEvent { get; }
	}

	public class FileAddedEventArgs : EventArgs {
		private readonly Uri m_FilePath;
		public FileAddedEventArgs(Uri filePath) {
			m_FilePath = filePath;
		}
		public Uri FilePath { get { return m_FilePath; } }
	}
	public class FileModifiedEventArgs : EventArgs {
		private readonly Uri m_FilePath;
		public FileModifiedEventArgs(Uri filePath) {
			m_FilePath = filePath;
		}
		public Uri FilePath { get { return m_FilePath; } }
	}
	public class FileMovedEventArgs : EventArgs {
		private readonly Uri m_OldFilePath, m_NewFilePath;
		public FileMovedEventArgs(Uri oldFilePath, Uri newFilePath) {
			m_OldFilePath = oldFilePath;
			m_NewFilePath = newFilePath;
		}
		public Uri OldFilePath { get { return m_OldFilePath; } }
		public Uri NewFilePath { get { return m_NewFilePath; } }
	}
	public class FileDeletedEventArgs : EventArgs {
		private readonly Uri m_FilePath;
		public FileDeletedEventArgs(Uri filePath) {
			m_FilePath = filePath;
		}
		public Uri FilePath { get { return m_FilePath; } }
	}
}