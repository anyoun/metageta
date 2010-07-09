using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight.Messaging;

namespace MetaGeta.GUI.Services {
	public class InputPromptMessage : MessageBase {
		private readonly string m_Title, m_Message;
		private readonly Action<string> m_Callback;
		public InputPromptMessage(string title, string message, Action<string> callback) {
			m_Title = title;
			m_Message = message;
			m_Callback = callback;
		}
		public string Message { get { return m_Message; } }
		public string Title { get { return m_Title; } }
		public Action<string> Callback { get { return m_Callback; } }
	}
}
