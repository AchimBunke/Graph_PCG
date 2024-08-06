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

using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UniRx;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Utils
{
    public struct HGraphAttributeFilter
    {
        public HashSet<string> Categories;
    }
    public class HGraphAttributeFilterView : IDisposable
    {
        Dictionary<Tuple<HGraphNode,string>, IDisposable> _attributeDataDisposables = new();
        Dictionary<HGraphNode, IDisposable> _attributeDisposables = new();

        private CollectionView<HGraphNode> _nodeCollectionView;
        private ObservableCollection<HGraphNode> _nodeCollection;
        public HGraphAttributeFilterView() 
        {
            _nodeCollectionView = new();
            _categoryFilter = new HGraphAttributeFilter();
            _nodeCollectionView.Filter += NodeCollectionView_Filter;
            _nodeCollection = HGraph.Instance.Nodes.ToObservableCollection();
            _nodeCollection.CollectionChanged += _nodeCollection_CollectionChanged; // If any node is added track attribute added/removed
            _nodeCollectionView.ViewSource = _nodeCollection;
            SetupListeners();
        }

        public HGraphAttributeFilterView(HGraphAttributeFilter filter) : this()
        {
            SetAttributeFilter(filter);
        }

        /// <summary>
        /// Setups listeners for attribute changes and attribute removal/adding.
        /// </summary>
        private void SetupListeners()
        {
            _nodeCollectionView.ViewChanged += NodeCollectionView_ViewChanged;
            SetupAttributeDataListenersForCurrentView();
            SetupAttributeListeners();
        }

        /// <summary>
        /// Creates listeners for added and removed attributes on all nodes that are added/removed!
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _nodeCollection_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach (var kv in _attributeDisposables)
                {
                    kv.Value?.Dispose();
                }
            }
            if (e.OldItems != null)
            {
                foreach (HGraphNode oldNode in e.OldItems)
                {
                    DisposeAttributeListener(oldNode);
                }
            }
            if (e.NewItems != null)
            {
                foreach (HGraphNode newNode in e.NewItems)
                {
                    SetupAttributeListener(newNode);
                }
            }
        }
        /// <summary>
        /// Creates listeners for added and removed attributes on all nodes
        /// </summary>
        private void SetupAttributeListeners()
        {
            foreach (var n in _nodeCollection)
            {
                SetupAttributeListener(n);
            }
        }
        /// <summary>
        /// Creates listeners for added and removed attributes on a node
        /// </summary>
        private void SetupAttributeListener(HGraphNode node)
        {
            _attributeDisposables[node] = node.Attributes.ObserveAnyChange().Subscribe(_ =>
            {
                _nodeCollectionView.RefreshItem(node);
            });
        }
        /// <summary>
        /// Disposes listeners for added and removed attributes on a node
        /// </summary>
        private void DisposeAttributeListener(HGraphNode node)
        {
            _attributeDisposables[node]?.Dispose();
        }

        /// <summary>
        /// Creates listeners for attribute data changes in the current view
        /// </summary>
        private void SetupAttributeDataListenersForCurrentView()
        {
            foreach(var n in _nodeCollectionView)
            {
                SetupAttributeDataListeners(n);
            }
        }
        private void SetupAttributeDataListeners(HGraphNode node)
        {
            foreach (var attName in _categoryFilter.Categories ?? new HashSet<string>())
            {
                SetupAttributeDataListener(node, node.Attributes[attName]);
            }
        }
        private void SetupAttributeDataListener(HGraphNode node, HGraphAttribute attribute)
        {
            var key = Tuple.Create(node, attribute.Category.Value);
            _attributeDataDisposables[key] = attribute.DataChanged.Subscribe(_ =>
            {
                _attributeChanged.OnNext(new NodeAttributeArgs() { Node = node, Attribute = attribute });
            });
        }
        private void DisposeAttributeDataListeners()
        {
            foreach (var kv in _attributeDataDisposables.ToList())
            {
                _attributeDataDisposables[kv.Key].Dispose();
                _attributeDataDisposables.Remove(kv.Key);
            }
        }
        private void DisposeAttributeDataListeners(HGraphNode node)
        {
            foreach (var attName in _categoryFilter.Categories)
            {
                var key = Tuple.Create(node, attName);
                _attributeDataDisposables[key].Dispose();
                _attributeDataDisposables.Remove(key);
            }
        }
        /// <summary>
        /// Creates or disposes attribtue data listeners on any added/removed filtered node.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void NodeCollectionView_ViewChanged(object sender, CollectionViewChangedEventArgs<HGraphNode> args)
        {
            if (args.IsReset)
            {
                DisposeAttributeDataListeners();
            }
            else
            {
                foreach (var oldNode in args.RemovedItems)
                {
                    DisposeAttributeDataListeners(oldNode);
                }
                foreach (var newNode in args.AddedItems)
                {
                    SetupAttributeDataListeners(newNode);
                }
            }
            _nodesChanged?.OnNext(args);
        }

        private bool NodeCollectionView_Filter(HGraphNode item)
        {
            return _categoryFilter.Categories?.All(k => item.Attributes.ContainsKey(k)) ?? true;
        }

        private HGraphAttributeFilter _categoryFilter;
        public void SetAttributeFilter(HGraphAttributeFilter filter)
        {
            if (_isDisposed)
                throw new ObjectDisposedException("CollectionView<>");
            _categoryFilter = filter;
            _nodeCollectionView.RefreshView();
        }

        private bool _isDisposed;
        public void Dispose()
        {
            if (_isDisposed)
                throw new ObjectDisposedException("CollectionView<>");
            _isDisposed = true;
            _nodeCollectionView?.Dispose();
            DisposeAttributeDataListeners();
            _attributeChanged.Dispose();
            _nodesChanged.Dispose();
        }

        private Subject<NodeAttributeArgs> _attributeChanged = new();
        public IObservable<NodeAttributeArgs> AttributeChanged => _attributeChanged;
        private Subject<CollectionViewChangedEventArgs<HGraphNode>> _nodesChanged = new();
        public IObservable<CollectionViewChangedEventArgs<HGraphNode>> NodesChanged => _nodesChanged;
        public IEnumerable<HGraphNode> FilteredNodes => _isDisposed ? throw new ObjectDisposedException("CollectionView<>") : _nodeCollectionView.FilteredItems;

    }
    public struct NodeAttributeArgs
    {
        public HGraphNode Node;
        public HGraphAttribute Attribute;
    }
}
