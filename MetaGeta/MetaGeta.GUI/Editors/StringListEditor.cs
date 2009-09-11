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

using System.Windows;

#endregion

namespace MetaGeta.GUI.Editors {
    public class StringListEditor : ItemListEditor {
        private string m_DialogCaption;

        private string m_DialogPrompt;

        public string DialogCaption {
            get { return m_DialogCaption; }
            set { m_DialogCaption = value; }
        }

        public string DialogPrompt {
            get { return m_DialogPrompt; }
            set { m_DialogPrompt = value; }
        }

        protected override string CreateItem() {
            var prompt = new StringInputPrompt(DialogCaption, DialogPrompt);
            prompt.Owner = Window.GetWindow(this);
            bool? result = prompt.ShowDialog();
            if (result.HasValue && result.Value)
                return prompt.EditingString;
            else
                return null;
        }
    }
}