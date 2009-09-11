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
using System.ComponentModel;
using System.Windows;

#endregion

namespace MetaGeta.GUI {
    public class DesignModeDataHelper {
        public static readonly DependencyProperty DesignTimeDataContextProperty = DependencyProperty.RegisterAttached("DesignTimeDataContext", typeof (Type),
                                                                                                                      typeof (DesignModeDataHelper),
                                                                                                                      new PropertyMetadata(OnDesignTimeDataContextChanged));

        private static void OnDesignTimeDataContextChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e) {
            SetDesignTimeDataContext(obj, e.NewValue);
        }

        public static void SetDesignTimeDataContext(DependencyObject obj, object value) {
            var element = obj as FrameworkElement;

            if (element != null && DesignerProperties.GetIsInDesignMode(element)) {
                var t = (Type) value;
                element.DataContext = Activator.CreateInstance(t);
            }
        }

        public static object GetDesignTimeDataContext(DependencyObject obj) {
            var element = obj as FrameworkElement;

            if (element != null && DesignerProperties.GetIsInDesignMode(element))
                return element.DataContext.GetType();
            else
                return null;
        }
    }
}