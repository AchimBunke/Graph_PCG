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

using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    public class PCGGraph
    {
        public PCGGraph(HGraphData data)
        {
            Nodes = new(data.nodes.Select(n => KeyValuePair.Create(n.id, n)));
            Categories = new(data.categories.Select(c => KeyValuePair.Create(c.id, c)));
            Relations = new(data.relations.Select(r => KeyValuePair.Create(r.id, r)));
            Enums = new(data.enumData.Select(e => KeyValuePair.Create(e.id, e)));
            RulesGUID = data.rulesGUID;
        }
        public string RulesGUID { get; private set; }
        public Dictionary<string, HGraphNodeData> Nodes { get; private set; }
        public Dictionary<string, HGraphCategoryData> Categories { get; private set; }
        public Dictionary<string, HGraphRelationData> Relations { get; private set; }
        public Dictionary<string, HGraphEnumData> Enums { get; private set; }

        public bool TryGetSuperNode(HGraphNodeData nodeData, out HGraphNodeData superNode) => Nodes.TryGetValue(nodeData.id, out superNode);
        public bool TryGetCategory(HGraphAttributeData attributeData, out HGraphCategoryData category) => Categories.TryGetValue(attributeData.category, out category);
        public bool TryCreateAttributeContent(HGraphCategoryData category, out HGraphAttributeContent content)
        {
            if (category.defaultContent == null)
            {
                content = null;
                return false;
            }
            content = category.defaultContent.Copy() ?? category.type.CreateData();
            return true;
        }
        public bool TryCreateAttributeData(HGraphCategoryData category, out HGraphAttributeData attributeData)
        {
            if (TryCreateAttributeContent(category, out var content))
            {
                var attribute = HGraphAttribute.Construct(content);
                attribute.Category.Value = category.id;
                attributeData = HGraphSerializationController.Serialize(attribute);
                return true;
            }
            attributeData = null;
            return false;
        }
        public string GetRelationPartner(HGraphRelationData relationData, string nodeId)
        {
            if (relationData.source == nodeId)
                return relationData.target;
            else if (relationData.target == nodeId)
                return relationData.source;
            else throw new ArgumentException("Node not part of relation");
        }
        public bool TryGetRelationPartner(HGraphRelationData relationData, HGraphNodeData nodeData, out HGraphNodeData relatedNode)
        {
            return Nodes.TryGetValue(GetRelationPartner(relationData, nodeData.id), out relatedNode);
        }
        public IEnumerable<HGraphNodeData> GetNeighbors(HGraphNodeData nodeData)
        {
            return nodeData.relations.Select(r => TryGetRelationPartner(Relations[r], nodeData, out var partner) ? partner : null).Where(n => n != null);
        }
        public bool TryGetAttribute(HGraphNodeData nodeData, string category, out HGraphAttributeData attributeData)
        {
            return (attributeData = nodeData.attributes.FirstOrDefault(a => a.category == category)) != null;
        }
        public bool TryGetAttribute(HGraphNodeData nodeData, HGraphCategoryData category, out HGraphAttributeData attributeData)
            => TryGetAttribute(nodeData, HGraphResources.CreateCategoryId(category.name), out attributeData);
        public HGraphAttributeData Copy(HGraphAttributeData data)
        {
            return new HGraphAttributeData() { category = data.category, type = data.type, data = data.data.Copy() };
        }
        public IEnumerable<HGraphNodeData> GetChildren(HGraphNodeData node)
            => Nodes.Values.Where(n => n.superNode == node.id);// Might lead to bad performance

        public float Distance(Vector3 point, HGraphNodeData node, out Vector3 closestPoint)
        {
            var spaceData = node.spaceData;
            if (!spaceData.isImplicit)
                return spaceData.Distance(point, out closestPoint);
            float minDist = float.MaxValue;
            closestPoint = default;
            foreach (var child in GetChildren(node))
            {
                if (child == null)
                    continue;
                var subSpace = child.spaceData;
                if (subSpace == null)
                    continue;
                var d = Distance(point, child, out var cp);
                if (d < minDist)
                {
                    closestPoint = cp;
                    minDist = d;
                }
            }
            return minDist;
        }

        public IEnumerable<HGraphNodeData> GetAncestors(HGraphNodeData node)
        {
            HGraphNodeData parent;
            while (TryGetSuperNode(node, out parent))
            {
                yield return parent;
            }
        }

        public IEnumerable<HGraphNodeData> GetRelatedNodes(HGraphNodeData node)
        {
            HashSet<HGraphNodeData> relatedNodes = new HashSet<HGraphNodeData>();
            foreach (var r in node.relations)
            {
                if (Relations.TryGetValue(r, out var relation))
                {
                    if (TryGetRelationPartner(relation, node, out var relatedNode))
                    {
                        relatedNodes.Add(relatedNode);
                    }
                }
            }
            return relatedNodes;
        }
        /// <summary>
        /// Returns the neighborhood including the given nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public IEnumerable<HGraphNodeData> GetClosedNodeNeighborhood(params HGraphNodeData[] nodes)
        {
            HashSet<HGraphNodeData> neighborhood = new HashSet<HGraphNodeData>(nodes);
            foreach (var node in nodes)
            {
                neighborhood.UnionWith(GetRelatedNodes(node));
            }
            return neighborhood;
        }
        /// <summary>
        /// Returns the neighborhood excluding the given nodes.
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public IEnumerable<HGraphNodeData> GetOpenNodeNeighborhood(params HGraphNodeData[] nodes)
        {
            return GetClosedNodeNeighborhood(nodes).Except(nodes);
        }


    }

    public class PCGGraphManager
    {
        #region Singleton
        private static PCGGraphManager _instance;
        public static PCGGraphManager Instance => _instance ?? (_instance = new PCGGraphManager());

        private PCGGraphManager()
        {
        }
        #endregion

        private PCGGraph _pcgGraph;
        public PCGGraph PCGGraph
        {
            get
            {

                if (IsPCGGraphDirty() || _pcgGraph == null)
                    GeneratePCGGraph();
                return _pcgGraph;
            }
        }
        private bool _isGraphDirty;
        public void SetPCGGraphDirty() => _isGraphDirty = true;
        private bool IsPCGGraphDirty()
        {
            return _isGraphDirty;
        }
        private void GeneratePCGGraph()
        {
            var pcgGraphData = HGraphSerializationController.SerializeToData(HGraph.Instance);
            _pcgGraph = new PCGGraph(pcgGraphData);
            ApplyRules();
            FlattenNodeAttributes();
            _isGraphDirty = false;
        }
        public void ApplyRules()
        {
            var assetPath = AssetDatabase.GUIDToAssetPath(_pcgGraph.RulesGUID);
            var ruleCollection = AssetDatabase.LoadAssetAtPath<HGraphRuleCollection>(assetPath);
            if (ruleCollection == null)
                return;
            List<HGraphRule> rules = ruleCollection.Rules?
                .Where(r => r.ApplyRule)
                .OrderBy(r => r.ExecutionPriority)
                .ToList() ?? new List<HGraphRule>();
            foreach (var rule in rules)
            {
                if (rule is HGraphGlobalRule globalRule)
                {
                    globalRule.Apply(_pcgGraph);
                }
                else if (rule is HGraphNodeDataRule nodeDataRule)
                {
                    foreach (var node in _pcgGraph.Nodes.Values)
                    {
                        nodeDataRule.Apply(node, _pcgGraph);
                    }
                }
            }
        }
        public void FlattenNodeAttributes()
        {
            HashSet<string> flattenedNodes = new();
            foreach (var n in _pcgGraph.Nodes.Values)
            {
                FlattenNode(n, flattenedNodes);
            }

        }
        private void FlattenNode(HGraphNodeData node, HashSet<string> flattenedNodes)
        {
            var nodeId = node.id;
            if (flattenedNodes.Contains(nodeId))
                return;
            var superNodeId = node.superNode;
            if (!string.IsNullOrEmpty(superNodeId))
            {
                var superNode = _pcgGraph.Nodes[superNodeId];
                FlattenNode(superNode, flattenedNodes);
                var superNodeAttributes = superNode.attributes;
                var localAttributes = node.attributes;
                foreach (var att in superNodeAttributes.Except(localAttributes))
                {
                    node.attributes.Add(_pcgGraph.Copy(att));
                }
            }
            flattenedNodes.Add(nodeId);
        }
    }
}