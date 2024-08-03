using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    /// <summary>
    /// Has useful methods to manipuate the HGraph
    /// </summary>
    public static class HGraphController
    {
        /// <summary>
        /// Loads deserialized graphData.
        /// </summary>
        /// <param name="data">The data to load.</param>
        /// <param name="keepCurrentGraph">If True will not unload current data in graph.</param>
        /// <param name="update">If True will update existing nodes when a collision happens. If GraphData contains colliding data will stop and return false.</param>
        public static bool Load(HGraphData data, bool keepCurrentGraph = false, bool update = false)
        {
            if (!keepCurrentGraph)
            {
                ClearGraph();
            }
            if (!update)
            {
                if (HasCollidingData(data))
                    return false;
            }
            if (data.enumData == null)
            {
                data.enumData = new();
            }
            foreach (var enumData in data.enumData)
            {
                if (update && HGraph.Instance.EnumDefinitions.TryGetValue(enumData.id, out var existingEnum))
                {
                    existingEnum.Update(enumData);
                }
                else
                {
                    var hEnum = HGraphEnum.Construct(enumData);
                    HGraph.Instance.EnumDefinitions.Add(hEnum.HGraphId.Value, hEnum);
                }
            }

            foreach (var categoryData in data.categories)
            {
                if (update && HGraph.Instance.Categories.TryGetValue(categoryData.id,out var existingCategory))
                {
                    existingCategory.Update(categoryData);
                }
                else
                {
                    var category = HGraphCategory.Construct(categoryData);
                    HGraph.Instance.Categories.Add(category.HGraphId.Value, category);
                }
            }
            foreach(var nodeData in data.nodes)
            {
                if (update && HGraph.Instance.Nodes.TryGetValue(nodeData.id, out var existingNode))
                {
                    existingNode.Update(nodeData);
                }
                else
                {
                    var node = HGraphNode.Construct(nodeData);
                    HGraph.Instance.Nodes.Add(node.HGraphId.Value, node);
                }
            }
            foreach(var relationData in data.relations)
            {
                if (update && HGraph.Instance.Relations.TryGetValue(relationData.id, out var existingRelation))
                {
                    existingRelation.Update(relationData);
                }
                else
                {
                    var relation = HGraphRelation.Construct(relationData);
                    HGraph.Instance.Relations.Add(relation.HGraphId.Value, relation);
                }
            }
            if (!string.IsNullOrWhiteSpace(data.rulesGUID))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(data.rulesGUID);
                var rules = AssetDatabase.LoadAssetAtPath<HGraphRuleCollection>(assetPath);
                if(rules != null)
                    HGraph.Instance.Rules.Value = rules;
            }
            HGraph.Instance.RaiseGraphLoaded();
            return true;
        }


        public static bool HasCollidingData(HGraphData data)
        {
            var currentGraph = HGraph.Instance;
            foreach (var c in data.enumData)
            {
                if (IsColliding(c))
                    return true;
            }
            foreach (var c in data.categories)
            {
                if (IsColliding(c))
                    return true;
            }
            foreach (var n in data.nodes)
            {
                if (IsColliding(n))
                    return true;
            }
            foreach (var r in data.relations)
            {
                if (IsColliding(r))
                    return true;
            }
            return false;
        }
        public static bool IsColliding(HGraphEnumData enumData)
        {
            return HGraph.Instance.Categories.ContainsKey(enumData.id);
        }
        public static bool IsColliding(HGraphCategoryData categoryData)
        {
            return HGraph.Instance.Categories.ContainsKey(categoryData.id);
        }
        public static bool IsColliding(HGraphNodeData nodeData)
        {
            return HGraph.Instance.Nodes.ContainsKey(nodeData.id);
        }
        public static bool IsColliding(HGraphRelationData relationData)
        {
            return HGraph.Instance.Relations.ContainsKey(relationData.id);
        }


        public static void ClearGraph()
        {
            HGraph.Instance.EnumDefinitions.Clear();
            HGraph.Instance.Nodes.Clear();
            HGraph.Instance.Categories.Clear();
            HGraph.Instance.Relations.Clear();
            HGraph.Instance.Rules.Value = null;
        }

        public static void DestroyNode(HGraphNode node, bool destroyRelations = false)
        {
            if (destroyRelations)
            {
                var relationsToDestroy = node.Relations.ToArray();
                foreach (var r in relationsToDestroy)
                {
                    if (HGraph.Instance.Relations.TryGetValue(r, out var relation))
                    {
                        DestroyRelation(relation);
                    }
                }
            }
            HGraph.Instance.Nodes.Remove(node.HGraphId.Value);
            node.Dispose();
        }
        public static void DestroyNode(string nodeId)
        {
            if(HGraph.Instance.Nodes.TryGetValue(nodeId, out var node))
                DestroyNode(node);
        }
        public static void DestroyRelation(HGraphRelation relation)
        {
            HGraph.Instance.Relations.Remove(relation.HGraphId.Value);
            if (HGraph.Instance.Nodes.TryGetValue(relation.Source.Value, out var source))
                source.Relations.Remove(relation.HGraphId.Value);
            if (HGraph.Instance.Nodes.TryGetValue(relation.Target.Value, out var target))
                target.Relations.Remove(relation.HGraphId.Value);
        }
        public static void DestroyRelation(string relationId)
        {
            if (HGraph.Instance.Relations.TryGetValue(relationId, out var relation))
                DestroyRelation(relation);
        }

        public static void DestroyCategory(HGraphCategory category, bool destroyAttributes = false)
        {
            if (destroyAttributes)
            {
                foreach(var node in HGraph.Instance.Nodes.Values)
                {
                    node.LocalAttributes.Remove(category.HGraphId.Value);
                }
            }
            HGraph.Instance.Categories.Remove(category.HGraphId.Value);
        }
        public static void DestroyCategory(string categoryId)
        {
            if (HGraph.Instance.Categories.TryGetValue(categoryId, out var category))
                DestroyCategory(category);
        }

        public static HGraphNode FindNode(string nodeId)
        {
            return HGraph.Instance.Nodes[nodeId];
        }
        public static bool TryFindNode(string nodeId, out HGraphNode node)
        {
            return HGraph.Instance.Nodes.TryGetValue(nodeId, out node);
        }

        public static bool IsAncestorOf(HGraphNode node, HGraphNode ancestor)
        {
            var currentNode = node.SuperNode.Value;
            while(HGraphResources.IsHGraphIdValid(currentNode))
            {
                if (currentNode == ancestor.HGraphId.Value)
                    return true;
                currentNode = HGraphController.FindNode(currentNode).SuperNode.Value;
            }
            return false;
        }

        public static bool TryGetRelatedNode(HGraphNode source, HGraphRelation relation, out HGraphNode relatedNode)
        {
            if (relation.Source.Value == source.HGraphId.Value)
                return relation.TryGetTargetNode(out relatedNode);
            else if (relation.Target.Value == source.HGraphId.Value)
                return relation.TryGetSourceNode(out relatedNode);
            else
                relatedNode = null;
            return false;
        }
        /// <summary>
        /// Returns All Related Nodes
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<HGraphNode> GetRelatedNodes(HGraphNode node)
        {
            var hGraph = HGraph.Instance;
            HashSet<HGraphNode> relatedNodes = new HashSet<HGraphNode>();
            foreach (var r in node.Relations)
            {
                if (hGraph.Relations.TryGetValue(r, out var relation))
                {
                    if(TryGetRelatedNode(node,relation,out var relatedNode))
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
        public static IEnumerable<HGraphNode> GetClosedNodeNeighborhood(params HGraphNode[] nodes)
        {
            HashSet<HGraphNode> neighborhood = new HashSet<HGraphNode>(nodes);
            foreach(var node in nodes)
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
        public static IEnumerable<HGraphNode> GetOpenNodeNeighborhood(params HGraphNode[] nodes)
        {
            return GetClosedNodeNeighborhood(nodes).Except(nodes);
        }

    }
}
