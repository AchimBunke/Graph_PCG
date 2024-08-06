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

using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    public class HGraphNodeSO : ScriptableObject
    {
        public HGraphNode Node;
    }


    [Serializable]
    public class HGraphNode : HGraphRuntimeBase
    {
        [field: SerializeField]
        public SerializedReactiveDictionary<string, HGraphAttribute> LocalAttributes { get; } = new();
        [field: SerializeField]
        public SerializedReactiveCollection<string> Relations { get; } = new();
        [field: SerializeField]
        public ReactiveProperty<string> SuperNode { get; } = new();

        [field: SerializeField]
        public ReactiveProperty<string> Name { get; } = new();

        public ReactiveFallbackAttributeDictionary Attributes { get; } = new();

        private ReactiveProperty<HGraphSceneNode> _sceneNode = new();
        public IReadOnlyReactiveProperty<HGraphSceneNode> SceneNode => _sceneNode;

        private HGraphSpaceData _spaceData;
        public HGraphSpaceData SpaceData => _spaceData;

        private HGraphNode()
        {
            var hgraph = HGraph.Instance;
            Attributes.BaseDictionary = LocalAttributes;
            // Setup Attributes
            // On SuperNode Value changes
            SuperNode.Subscribe(newVal =>
            {
                if (newVal == HGraphId.Value)
                {
                    Attributes.FallbackDictionary = null;
                    return;
                }
                if (HGraphResources.IsHGraphIdValid(newVal) && HGraph.Instance.Nodes.TryGetValue(newVal, out var newSuperNode))
                {
                    Attributes.FallbackDictionary = newSuperNode.Attributes;
                }
                else
                {
                    Attributes.FallbackDictionary = null;
                }
            });
            // On Graph changes
            _nodeChangedSubscriber = hgraph.Nodes.ObserveAdd().Where(kv => kv.Key == SuperNode.Value).Select(kv => kv.Value).Merge(
                    hgraph.Nodes.ObserveRemove().Where(kv => kv.Key == SuperNode.Value).Select(_ => (HGraphNode)null),
                    hgraph.Nodes.ObserveReplace().Where(kv => kv.Key == SuperNode.Value).Select(kv => kv.NewValue),
                    hgraph.Nodes.ObserveReset().Select(_ => (HGraphNode)null))
                .Subscribe(newNode =>
                {
                    if (newNode == this)
                    {
                        Attributes.FallbackDictionary = null;
                        return;
                    }
                    Attributes.FallbackDictionary = newNode?.Attributes;
                });
        }

        IDisposable _nodeChangedSubscriber;
        public override void Dispose()
        {
            base.Dispose();
            _nodeChangedSubscriber?.Dispose();

            //SuperNode.Dispose();
            //LocalAttributes.Dispose();
            //Relations.Dispose();
            //Name.Dispose();
            //Attributes.Dispose();
            //_sceneNode.Dispose();
        }
        public static HGraphNode Construct(string id)
        {
            var node = new HGraphNode();
            node.HGraphId.Value = id;
            return node;
        }
        public static HGraphNode Construct(HGraphNodeData data)
        {
            var node = Construct(data.id);
            node.SuperNode.Value = data.superNode;
            node.Name.Value = data.name;
            node._spaceData = data.spaceData;
            foreach (var rel in data.relations)
            {
                node.Relations.Add(rel);
            }
            foreach (var attribute in data.attributes)
            {
                var hGraphAttribute = HGraphAttribute.Construct(attribute);
                node.LocalAttributes.Add(hGraphAttribute.Category.Value, hGraphAttribute);
            }
            return node;
        }

        public bool HasRelationTo(string otherId)
        {
            return Relations.Contains(otherId);
        }
        public bool HasRelationTo(HGraphNode other)
        {
            return HasRelationTo(other.HGraphId.Value);
        }

        public bool Connect(HGraphSceneNode sceneNode)
        {
            if (SceneNode.Value != null)
                return false;
            _sceneNode.Value = sceneNode;
            return true;
        }
        public void Disconnect()
        {
            _sceneNode.Value = null;
        }

        /// <summary>
        /// Update this node with a NodeData object
        /// </summary>
        /// <param name="newData"></param>
        /// <param name="additive">If True will keep current attribute and relations that are not in the nodeData</param>
        public void Update(HGraphNodeData newData, bool additive = false)
        {
            Name.Value = newData.name;
            SuperNode.Value = newData.superNode;
            _spaceData = newData.spaceData;
            if (!additive)
            {
                var attributeKeysToRemove = LocalAttributes.Keys.Except(newData.attributes.Select(a => a.category));
                foreach (var toRemove in attributeKeysToRemove)
                {
                    LocalAttributes.Remove(toRemove);
                }
            }
            foreach (var toAdd in newData.attributes)
            {
                if (LocalAttributes.TryGetValue(toAdd.category, out var existingAttribute))
                {
                    existingAttribute.Update(toAdd);
                }
                else
                {
                    var newAtt = HGraphAttribute.Construct(toAdd);
                    LocalAttributes.Add(newAtt.Category.Value, newAtt);
                }
            }
            if (!additive)
            {
                var relationToRemove = Relations.Except(newData.relations);
                foreach (var toRemove in relationToRemove)
                {
                    Relations.Remove(toRemove);
                }
            }
            foreach (var toAdd in newData.relations)
            {
                if (!Relations.Contains(toAdd))
                {
                    Relations.Add(toAdd);
                }
            }
        }


        public IEnumerable<HGraphNode> GetChildren()
            => HGraph.Instance.Nodes.Values.Where(n => n.SuperNode.Value == HGraphId.Value);// Might lead to bad performance
        public IEnumerable<HGraphNode> GetAncestors()
        {
            HashSet<HGraphNode> ancestors = new();
            var node = this;
            while (HGraphResources.IsHGraphIdValid(node.SuperNode.Value) && HGraph.Instance.Nodes.TryGetValue(node.SuperNode.Value, out var superNode))
            {
                ancestors.Add(superNode);
                node = superNode;
            }
            return ancestors;
        }

    }

}