using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Vector3 = UnityEngine.Vector3;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services
{
    public class HGraphSpaceSearch
    {
        public HGraphSpaceSearchSettings Settings { get; set; }

        public HGraphSpaceSearch() { Settings = HGraphSpaceSearchSettings.Default; }
        public HGraphSpaceSearch(HGraphSpaceSearchSettings settings)
        {
            Settings = settings;
        }

        private SpatialDistanceMeasure _spatialDistantMeasure;
        public HGraphSpaceSearchResult FindNearbyNodes(HGraphNode node)
        {
            return FindNearbyNodes(node, null);

        }
        /// <summary>
        /// Finds nearby nodes starting from a node.
        /// </summary>
        /// <param name="node">The node in which the position lies. Starting point for the graph search.</param>
        /// <param name="position"></param>
        /// <returns></returns>
        public HGraphSpaceSearchResult FindNearbyNodes(HGraphNode node, Vector3? position)
        {
            _spatialDistantMeasure = Settings.SpatialDistanceMeasureConfiguration.Create();
            var root = FindSubtreeRoot(node);
            var foundSubtreeNodes = FindNearbySubtreeNodes(root, node, position); // Selects all nodes inside the same tree as the targetNode. Does not consider Neighborhood.
            var neighborhoodNodes = HGraphController.GetOpenNodeNeighborhood(foundSubtreeNodes.Select(t => t.Item1).Concat(new[] { node }).ToArray());
            var filteredNeighborhoodSet = new HashSet<(HGraphNode node, float distance)>();
            foreach (var n in neighborhoodNodes)
            {
                if (filteredNeighborhoodSet.Contains((n, 0f)))
                    continue;
                filteredNeighborhoodSet.UnionWith(FindNearbySubtreeNodes(n, node, position));
            }
            filteredNeighborhoodSet.Remove((node, 0f));
            var completeSet = foundSubtreeNodes.Concat(filteredNeighborhoodSet);
            var completeWithSpacesRemoved = completeSet.Where(n =>
            {
                var space = n.node.SceneNode.Value.GetComponent<HGraphNodeSpace>();
                if (!Settings.SelectSpaces && (space != null))
                    return false;
                if (Settings.ExcludeImplicitSpaces && space != null && space.ImplicitSpace)
                    return false;
                return true;
            }).Concat(new[] { (node, 0f) });
            if (Settings.ExcludeAncestors)
                completeWithSpacesRemoved = completeWithSpacesRemoved.Except(node.GetAncestors().Select(n => (n, 0f)));
            var result = new HGraphSpaceSearchResult() { Nodes = completeWithSpacesRemoved.Select(i => i.node).ToList(), Distances = completeWithSpacesRemoved.Select(i => i.Item2).ToList() };
            return result;
        }
        /// <summary>
        /// Find the space and the nearby nodes. Use <see cref="FindNearbyNodes(HGraphNode, Vector3?)"/> if space is already known to speed up search.
        /// </summary>
        /// <param name="position"></param>
        /// <returns></returns>
        public HGraphSpaceSearchResult FindNearbyNodes(Vector3 position)
        {
            var space = FindSpace(position);
            if (space == null)
                return HGraphSpaceSearchResult.Empty;
            return FindNearbyNodes(space.SceneNode.NodeData.Value, position);
        }

        public HGraphNodeSpace FindSpace(Vector3 position) => Settings.FallbackNearestSpace ? HGraphSpaceUtility.FindNearestSpace(position) : HGraphSpaceUtility.FindSpace(position);


        /// <summary>
        /// Finds all nodes inside a subtree of spaces
        /// </summary>
        /// <param name="currentSpace"></param>
        /// <returns></returns>
        private IEnumerable<(HGraphNode node, float distance)> FindNearbySubtreeNodes(HGraphNode currentNode, HGraphNode targetNode, Vector3? position)
        {
            HashSet<(HGraphNode, float)> foundNodes = new(new TupleFirstItemEqualityComparer<HGraphNode, float>());

            bool inDistance = false;
            float distance = 0;
            if (currentNode != targetNode)
            {
                var sceneNode = currentNode.SceneNode.Value;
                if (sceneNode != null)// Discard missing nodes
                {
                    if (position != null)
                    {
                        distance = _spatialDistantMeasure.Distance(currentNode, (Vector3)position);
                        if (distance <= Settings.MaxDistance)// Discard far away nodes
                            inDistance = true;
                    }
                    else
                    {
                        distance = _spatialDistantMeasure.Distance(currentNode, targetNode);
                        if (distance <= Settings.MaxDistance)// Discard far away nodes
                            inDistance = true;
                    }
                }
                if (inDistance)
                {
                    var currentSpace = sceneNode.GetComponent<HGraphNodeSpace>();
                    if (currentSpace == null)// Atomic Node removal
                    {
                        if (Settings.SelectAtomics)
                        {
                            foundNodes.Add((currentNode, distance));
                        }
                    }
                    else
                    {
                        foundNodes.Add((currentNode, distance));
                    }
                }
                else // Early return if not in distance
                {
                    return foundNodes;
                }
            }
            foreach (var child in currentNode.GetChildren())
            {
                foundNodes.UnionWith(FindNearbySubtreeNodes(child, targetNode, position)); // Recursion to find Nodes/Spaces inside subspaces
            }
            return foundNodes;
        }

        private HGraphNode FindSubtreeRoot(HGraphNode targetNode) => targetNode.GetAncestors().LastOrDefault() ?? targetNode;


    }

    class TupleFirstItemEqualityComparer<T1, T2> : IEqualityComparer<(T1, T2)>
    {
        public bool Equals((T1, T2) x, (T1, T2) y)
        {
            return EqualityComparer<T1>.Default.Equals(x.Item1, y.Item1);
        }

        public int GetHashCode((T1, T2) obj)
        {
            return EqualityComparer<T1>.Default.GetHashCode(obj.Item1);
        }
    }

    [Serializable]
    public struct HGraphSpaceSearchSettings
    {
        public NeighborhoodMode NeighborhoodMode;
        public float MaxDistance;
        public bool SelectSpaces;
        public bool SelectAtomics;
        public bool ExcludeAncestors;
        public bool FallbackNearestSpace;
        public bool ExcludeImplicitSpaces;
        public SpatialDistanceMeasureConfiguration SpatialDistanceMeasureConfiguration;

        public static HGraphSpaceSearchSettings Default => new HGraphSpaceSearchSettings()
        {
            MaxDistance = 10,
            SelectSpaces = true,
            SelectAtomics = true,
            ExcludeAncestors = true,
            FallbackNearestSpace = true,
            ExcludeImplicitSpaces = true,
            SpatialDistanceMeasureConfiguration = SpatialDistanceMeasureConfiguration.Default,
        };
    }
    public struct HGraphSpaceSearchResult
    {
        public List<HGraphNode> Nodes;
        public List<float> Distances;
        public static HGraphSpaceSearchResult Empty
            => new HGraphSpaceSearchResult() { Nodes = new(), Distances = new() };
    }
    public struct PCGGraphSpaceSearchResult
    {
        public List<HGraphNodeData> Nodes;
        public List<float> Distances;
        public static PCGGraphSpaceSearchResult Empty
            => new PCGGraphSpaceSearchResult() { Nodes = new(), Distances = new() };
    }

    public enum NeighborhoodMode
    {
        Graph,
        Spatial
    }

    public class PCGGraphSpaceSearch
    {
        private PCGGraph GetPCGGraph() => PCGGraphManager.Instance.PCGGraph;
        private PCGGraph pcgGraph;
        public PCGGraph PCGGraph
        {
            get => pcgGraph ?? GetPCGGraph();
            set => pcgGraph = value;
        }
        public HGraphSpaceSearchSettings Settings { get; set; }

        public PCGGraphSpaceSearch() { Settings = HGraphSpaceSearchSettings.Default; }
        public PCGGraphSpaceSearch(HGraphSpaceSearchSettings settings)
        {
            Settings = settings;
        }
        public PCGGraphSpaceSearch(HGraphSpaceSearchSettings settings, PCGGraph graph)
        {
            Settings = settings;
            PCGGraph = graph;
        }

        private SpatialDistanceMeasure _spatialDistantMeasure;
        public PCGGraphSpaceSearchResult FindNearbyNodes(HGraphNodeData node)
        {
            return FindNearbyNodes(node, null);

        }
        public PCGGraphSpaceSearchResult FindNearbyNodes(HGraphNodeData node, Vector3? position)
        {
            _spatialDistantMeasure = Settings.SpatialDistanceMeasureConfiguration.Create();
            var root = FindSubtreeRoot(node);
            var foundSubtreeNodes = FindNearbySubtreeNodes(root, node, position); // Selects all nodes inside the same tree as the targetNode. Does not consider Neighborhood.
            var neighborhoodNodes = PCGGraph.GetOpenNodeNeighborhood(foundSubtreeNodes.Select(t => t.Item1).Concat(new[] { node }).ToArray());
            var filteredNeighborhoodSet = new HashSet<(HGraphNodeData node, float distance)>();
            foreach (var n in neighborhoodNodes)
            {
                if (filteredNeighborhoodSet.Contains((n, 0f)))
                    continue;
                filteredNeighborhoodSet.UnionWith(FindNearbySubtreeNodes(n, node, position));
            }
            filteredNeighborhoodSet.Remove((node, 0f));
            var completeSet = foundSubtreeNodes.Concat(filteredNeighborhoodSet);
            var completeWithSpacesRemoved = completeSet.Where(n =>
            {
                var spaceData = n.node.spaceData;
                if (!Settings.SelectSpaces && (spaceData != null && !spaceData.isAtomic))
                    return false;
                if (Settings.ExcludeImplicitSpaces && spaceData != null && spaceData.isImplicit)
                    return false;
                return true;
            }).Concat(new[] { (node, 0f) });
            if (Settings.ExcludeAncestors)
                completeWithSpacesRemoved = completeWithSpacesRemoved.Except(PCGGraph.GetAncestors(node).Select(n => (n, 0f)));
            var result = new PCGGraphSpaceSearchResult() { Nodes = completeWithSpacesRemoved.Select(i => i.node).ToList(), Distances = completeWithSpacesRemoved.Select(i => i.Item2).ToList() };
            return result;
        }
        private IEnumerable<(HGraphNodeData node, float distance)> FindNearbySubtreeNodes(HGraphNodeData currentNode, HGraphNodeData targetNode, Vector3? position)
        {
            HashSet<(HGraphNodeData, float)> foundNodes = new(new TupleFirstItemEqualityComparer<HGraphNodeData, float>());

            bool inDistance = false;
            float distance = 0;
            var currentSpace = currentNode.spaceData;
            if (currentNode != targetNode)
            {
                if (position != null)
                {
                    distance = PCGGraphSpaceUtility.Distance((Vector3)position, currentNode, PCGGraph, out _);
                    if (distance <= Settings.MaxDistance)// Discard far away nodes
                        inDistance = true;
                }
                else
                {
                    distance = PCGGraphSpaceUtility.Distance(targetNode.spaceData.nodePosition, currentNode, PCGGraph, out _);
                    if (distance <= Settings.MaxDistance)// Discard far away nodes
                        inDistance = true;
                }

                if (inDistance)
                {
                    if (currentSpace.isAtomic)// Atomic Node removal
                    {
                        if (Settings.SelectAtomics)
                        {
                            foundNodes.Add((currentNode, distance));
                        }
                    }
                    else
                    {
                        foundNodes.Add((currentNode, distance));
                    }
                }
                else // Early return if not in distance
                {
                    return foundNodes;
                }
            }
            foreach (var child in PCGGraphManager.Instance.PCGGraph.GetChildren(currentNode))
            {
                foundNodes.UnionWith(FindNearbySubtreeNodes(child, targetNode, position)); // Recursion to find Nodes/Spaces inside subspaces
            }
            return foundNodes;
        }


        public PCGGraphSpaceSearchResult FindNearbyNodes(Vector3 position)
        {
            var spaceNode = FindSpace(position);
            if (spaceNode == null)
                return PCGGraphSpaceSearchResult.Empty;
            return FindNearbyNodes(spaceNode, position);
        }
        public HGraphNodeData FindSpace(Vector3 position) => Settings.FallbackNearestSpace ? PCGGraphSpaceUtility.FindNearestSpace(position, PCGGraph) : PCGGraphSpaceUtility.FindSpace(position, PCGGraph);

        private HGraphNodeData FindSubtreeRoot(HGraphNodeData targetNode) => PCGGraphManager.Instance.PCGGraph.GetAncestors(targetNode).LastOrDefault() ?? targetNode;

    }
    public static class HGraphSpaceUtility
    {
        /// <summary>
        /// Finds the best space for the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public static HGraphNodeSpace FindSpace(Vector3 point)
        {
            var hGraph = HGraph.Instance;
            var firstHit = hGraph.SceneNodes.Values.Where(sn => sn.IsHGraphConnected).FirstOrDefault(sn => sn.GetComponent<HGraphNodeSpace>()?.IsPointInsideSpaceOrSubspace(point, out _) ?? false);
            if (firstHit == null)
                return default;
            return FindSubspace(firstHit.GetComponent<HGraphNodeSpace>(), point);
        }
        /// <summary>
        /// Finds the best space for the given node (excluding itself).
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public static HGraphNodeSpace FindSpace(HGraphSceneNode node)
        {
            var hGraph = HGraph.Instance;
            var point = node.transform.position;
            var firstHit = hGraph.SceneNodes.Values.Where(sn => sn != node && sn.IsHGraphConnected).FirstOrDefault(sn => sn.GetComponent<HGraphNodeSpace>()?.IsPointInsideSpaceOrSubspace(point, out _) ?? false);
            if (firstHit == null)
                return default;
            return FindSubspace(firstHit.GetComponent<HGraphNodeSpace>(), point);
        }
        public static HGraphNodeSpace FindNearestSpace(Vector3 point)
        {
            var hGraph = HGraph.Instance;
            var currentMinDistance = float.MaxValue;
            HGraphNodeSpace currentNearestSpace = default;
            foreach (var n in hGraph.SceneNodes.Values.Where(sn => sn.IsHGraphConnected).Select(n => n.GetComponent<HGraphNodeSpace>()))
            {
                if (n == null)
                    continue;
                if (n.ImplicitSpace)
                    continue;
                var distance = n.Distance(point);
                if (distance < currentMinDistance)
                {
                    currentNearestSpace = n;
                    currentMinDistance = distance;
                }
                if (distance == 0)
                    break;
            }
            return currentMinDistance == 0 ? FindSubspace(currentNearestSpace, point) : currentNearestSpace;
        }

        /// <summary>
        /// Finds the best subspace from a space for the given point.
        /// </summary>
        /// <param name="space"></param>
        /// <param name="point"></param>
        /// <returns>The best space for the given point</returns>
        public static HGraphNodeSpace FindSubspace(HGraphNodeSpace space, Vector3 point)
        {
            if (space.IsPointInsideSpaceOrSubspace(point, out var subSpace))
            {
                if (subSpace == space)
                    return space;
                return FindSubspace(subSpace, point);
            }
            else
            {
                throw new ArgumentException("Point not inside provided space");
            }
        }



    }
    public static class PCGGraphSpaceUtility
    {
        public static HGraphNodeData FindSpace(Vector3 point, PCGGraph graph)
        {
            var firstHit = graph.Nodes.Values.Where(n => n.spaceData != null && !n.spaceData.isAtomic).FirstOrDefault(sn => IsPointInsideSpaceOrSubspace(point, sn, graph, out _));
            if (firstHit == null)
                return default;
            return FindSubspace(firstHit, graph, point);
        }

        public static HGraphNodeData FindSubspace(HGraphNodeData node, PCGGraph graph, Vector3 point)
        {
            var nodeSpace = node.spaceData;
            if (IsPointInsideSpaceOrSubspace(point, node, graph, out var subSpaceNode))
            {
                if (subSpaceNode == node)
                    return subSpaceNode;
                return FindSubspace(subSpaceNode, graph, point);
            }
            else
            {
                throw new ArgumentException("Point not inside provided space");
            }
        }

        public static HGraphNodeData FindNearestSpace(Vector3 point, PCGGraph graph)
        {
            var currentMinDistance = float.MaxValue;
            HGraphNodeData currentNearestSpaceNode = default;
            foreach (var n in graph.Nodes.Values.Where(n => n.spaceData != null))
            {
                var spaceData = n.spaceData;
                if (n == null)
                    continue;
                if (spaceData.isImplicit || spaceData.isAtomic)
                    continue;
                var distance = graph.Distance(point, n, out _);
                if (distance < currentMinDistance)
                {
                    currentNearestSpaceNode = n;
                    currentMinDistance = distance;
                }
                if (distance == 0)
                    break;
            }
            return currentMinDistance == 0 ? FindSubspace(currentNearestSpaceNode, graph, point) : currentNearestSpaceNode;
        }
        public static bool IsPointInsideSpaceOrSubspace(Vector3 point, HGraphNodeData nodeData, PCGGraph graph, out HGraphNodeData subSpaceNode)
        {
            HGraphSpaceData nodeSpace = null;
            subSpaceNode = null;
            if ((nodeSpace = nodeData.spaceData) == null)
            {
                subSpaceNode = default;
                return false;
            }
            if (nodeSpace.isAtomic)
            {
                subSpaceNode = default;
                return false;
            }
            if (nodeSpace.isImplicit)
            {
                if (CheckChildrenForPoint(nodeData, point, graph, out subSpaceNode))
                    return true;
                return false;
            }
            if (nodeSpace.IsPointInsideSpace(point))
            {
                // check if in subspace instead
                if (CheckChildrenForPoint(nodeData, point, graph, out subSpaceNode))
                    return true;
                else
                    subSpaceNode = nodeData;
                return true;
            }
            return false;
        }
        private static bool CheckChildrenForPoint(HGraphNodeData nodeData, Vector3 point, PCGGraph graph, out HGraphNodeData subSpaceNode)
        {
            foreach (var child in graph.GetChildren(nodeData))
            {
                if (child == null)
                    continue;
                var subSpace = child.spaceData;
                if (subSpace == null)
                    continue;
                if (subSpace.isImplicit)
                {
                    if (IsPointInsideSpaceOrSubspace(point, child, graph, out subSpaceNode))
                        return true;
                }
                else if (subSpace.IsPointInsideSpace(point))
                {
                    subSpaceNode = child;
                    return true;
                }
            }
            subSpaceNode = default;
            return false;
        }

        public static float Distance(Vector3 point, HGraphNodeData node, PCGGraph graph, out Vector3 closestPoint)
            => graph.Distance(point, node, out closestPoint);
    }
}