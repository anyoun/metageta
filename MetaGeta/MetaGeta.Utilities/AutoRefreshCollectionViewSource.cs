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
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

#endregion

namespace MetaGeta.Utilities {
    public class AutoRefreshCollectionViewSource : CollectionViewSource {
        protected override void OnSourceChanged(object oldSource, object newSource) {
            if (oldSource != null)
                RemoveEventHandlers(oldSource);
            if (newSource != null)
                AddEventHandlers(newSource);

            base.OnSourceChanged(oldSource, newSource);
        }

        private void AddEventHandlers(object source) {
            if (source is INotifyCollectionChanged)
                ((INotifyCollectionChanged) source).CollectionChanged += CollectionChanged;

            foreach (object item in (IEnumerable) source)
                AddEventHandler(item);
        }

        private void RemoveEventHandlers(object source) {
            if (source is INotifyCollectionChanged)
                ((INotifyCollectionChanged) source).CollectionChanged -= CollectionChanged;

            foreach (object item in (IEnumerable) source)
                RemoveEventHandler(item);
        }

        private void AddEventHandler(object item) {
            if (item is INotifyPropertyChanged)
                ((INotifyPropertyChanged) item).PropertyChanged += PropertyChanged;
        }

        private void RemoveEventHandler(object item) {
            if (item is INotifyPropertyChanged)
                ((INotifyPropertyChanged) item).PropertyChanged -= PropertyChanged;
        }

        private void CollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (object item in e.NewItems)
                        AddEventHandler(item);


                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (object item in e.OldItems)
                        RemoveEventHandler(item);


                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (object item in e.OldItems)
                        RemoveEventHandler(item);

                    foreach (object item in e.NewItems)
                        AddEventHandler(item);


                    break;
                case NotifyCollectionChangedAction.Move:
                    break;
                case NotifyCollectionChangedAction.Reset:
                    View.Refresh();

                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private void PropertyChanged(object sender, PropertyChangedEventArgs e) {
            if ((from s in SortDescriptions where s.PropertyName == e.PropertyName select s).Any())
                View.Refresh();
            else if ((from g in GroupDescriptions
                      where g is PropertyGroupDescription
                      let pg = (PropertyGroupDescription) g
                      where pg.PropertyName == e.PropertyName
                      select pg).Any())
                View.Refresh();
        }
    }
}