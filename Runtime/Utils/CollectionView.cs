/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Achioto.Gamespace_PCG.Runtime.Utils
{
    public class CollectionViewChangedEventArgs<T> : EventArgs
    {
        public readonly IEnumerable<T> RemovedItems;
        public readonly IEnumerable<T> AddedItems;
        public readonly bool IsReset;
        public CollectionViewChangedEventArgs(IEnumerable<T> removed, IEnumerable<T> added, bool isReset = false)
        {
            RemovedItems = removed;
            AddedItems = added;
            IsReset = isReset;
        }
    }
    public delegate void ViewChangedDelegate<T>(object sender, CollectionViewChangedEventArgs<T> args);

    /// <summary>
    /// Delegate describing a filtering operation
    /// </summary>
    /// <param name="relation">The Relation to evaluate</param>
    /// <returns>True if Item passes filter. Else False.</returns>
    public delegate bool CollectionViewFilterDelegate<T>(T item);
    public interface IReadOnlyCollectionView<T> : IEnumerable<T>, IEnumerable, IDisposable
    {
        event ViewChangedDelegate<T> ViewChanged;
        IEnumerable<T> FilteredItems { get; }
    }

    public class CollectionView<T> : IReadOnlyCollectionView<T>
    {
        public CollectionView()
        {
            _viewSource = new List<T>();
        }

        private event ViewChangedDelegate<T> _viewChanged;
        public event ViewChangedDelegate<T> ViewChanged
        {
            add 
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("CollectionView<>"); 
                _viewChanged += value; 
            }
            remove 
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("CollectionView<>");
                _viewChanged -= value; 
            }
        }

        /// <summary>
        /// The current Filter of this View.
        /// </summary>
        private event Predicate<T> _filter;
        public event Predicate<T> Filter
        {
            add 
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("CollectionView<>");
                _filter += value; 
            }
            remove
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("CollectionView<>");
                _filter -= value; 
            }
        }

        /// <summary>
        /// Reapplies filter on the ViewSource.
        /// </summary>
        /// <returns>Filtered items</returns>
        private IEnumerable<T> ApplyFilter()
        {
            // Select elements that passes all filters.
            if (_filter == null || _viewSource==null)
                return new List<T>();
            return _viewSource.Where(item =>
                _filter.GetInvocationList().Cast<Predicate<T>>()
                .All(filter => filter(item)));
        }

        /// <summary>
        /// View Cache.
        /// </summary>
        private List<T> _filteredItems = new();
        public IEnumerable<T> FilteredItems => _isDisposed ? throw new ObjectDisposedException("CollectionView<>") : _filteredItems;


        /// <summary>
        /// Handle changes on the ViewSource.
        /// Propagates changes in the ViewSource to the View by directly removing or filtering/adding changed items.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ViewSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            bool changed = false;
            bool isReset = false;
            // Reset => Clear items from view.
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                _filteredItems.Clear();
                changed = true;
                isReset = true;
            }
            List<T> removed = new List<T>();
            if (e.OldItems != null)
            {
                foreach (T p in e.OldItems)
                {
                    if (_filteredItems.Remove(p))
                    {
                        removed.Add(p);
                        changed = true;
                    }
                }
            }
            List<T> added = new List<T>();
            if (e.NewItems != null)
            {
                foreach (T p in e.NewItems)
                {
                    // Filter and add items
                    if (_filter == null || _filter.GetInvocationList().Cast<Predicate<T>>()
                        .All(filter => filter(p)))
                    {
                        _filteredItems.Add(p);
                        added.Add(p);
                        changed = true;
                    }
                }
            }
            // Raise event if View changed
            if (changed)
                _viewChanged?.Invoke(this, new CollectionViewChangedEventArgs<T>(removed, added, isReset));

        }

        private IEnumerable<T> _viewSource;
        /// <summary>
        /// Source item collection. Does call refresh on set.
        /// If provided with a INotifyCollectionChanged collection will automatically update View when collection changed.
        /// </summary>
        public IEnumerable<T> ViewSource
        {
            get => _isDisposed ? throw new ObjectDisposedException("CollectionView<>") : _viewSource;
            set
            {
                if (_isDisposed)
                    throw new ObjectDisposedException("CollectionView<>");
                if (_viewSource == value)
                    return;
                if (_viewSource != null)
                {
                    if (_viewSource is INotifyCollectionChanged cc)
                        cc.CollectionChanged -= ViewSource_CollectionChanged;
                }
                _viewSource = value;
                // If INotifyCollectionChanged subscribe
                if (value != null && value is INotifyCollectionChanged cc2)
                    cc2.CollectionChanged += ViewSource_CollectionChanged;
                // Default is empty View
                if (value == null)
                    _viewSource = new List<T>();
                RefreshView();
            }
        }

        /// <summary>
        /// Reevaluates filter on the ViewSource.
        /// Call this if AutoRefresh set to false and when the filter changes
        /// </summary>
        public void RefreshView()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("CollectionView<>");
            var oldItems = _filteredItems.ToArray();
            _filteredItems = ApplyFilter()?.ToList() ?? new List<T>();
            _viewChanged?.Invoke(this, new CollectionViewChangedEventArgs<T>(oldItems.Except(_filteredItems), _filteredItems.Except(oldItems)));
        }

        public IEnumerator<T> GetEnumerator() => _isDisposed ? throw new ObjectDisposedException("CollectionView<>") : _filteredItems.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _isDisposed ? throw new ObjectDisposedException("CollectionView<>") : _filteredItems.GetEnumerator();

        private bool _isDisposed = false;
        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("CollectionView<>");
            _isDisposed = true;
            if (_viewSource is INotifyCollectionChanged cc)
                cc.CollectionChanged -= ViewSource_CollectionChanged;
        }

        /// <summary>
        /// Notifies view about a potential change in a single specific item.
        /// Evaluates the filter and removes/adds it from/to filtered items.
        /// </summary>
        /// <param name="item"></param>
        public void RefreshItem(T item)
        {
            if (_filter == null || _filter.GetInvocationList().Cast<Predicate<T>>()
                        .All(filter => filter(item)))
            {
                if(!_filteredItems.Contains(item))
                {
                    _filteredItems.Add(item);
                    _viewChanged?.Invoke(this, new CollectionViewChangedEventArgs<T>(new T[] {}, new T[] {item}, false));
                }
            }
            else
            {
                if (_filteredItems.Remove(item))
                {
                    _viewChanged?.Invoke(this, new CollectionViewChangedEventArgs<T>(new T[] {item}, new T[] { }, false));
                }
            }
        }
    }

}
