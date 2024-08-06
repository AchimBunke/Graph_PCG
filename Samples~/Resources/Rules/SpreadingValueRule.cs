using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using ALib.Graph.Search;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    [CreateAssetMenu(fileName = "SpreadingValue", menuName = "ScriptableObjects/HGraph-Rules/SpreadingValue", order = 1)]
    public class SpreadingValueRule : HGraphGlobalRule
    {
        [SerializeField] string categoryName;
        public override void Apply(PCGGraph graph)
        {
            if (categoryName == null)
                return;
            if (!graph.Categories.TryGetValue(categoryName, out var category))
                return;
            var startNodes = graph.Nodes.Values.Where(n => n.attributes.Any(a => a.category == categoryName)).ToArray();

            IBSFAlgorithm<HGraphNodeData> bsfAlgorithm = new BSFAlgorithm<HGraphNodeData>((n) => AverageValue(n, graph, category), (n) => GetNeighbors(n, graph));
            bsfAlgorithm.Execute(startNodes);
        }
        private IEnumerable<HGraphNodeData> GetNeighbors(HGraphNodeData node, PCGGraph graph)
        {
            return graph.GetNeighbors(node);
        }
        private void AverageValue(HGraphNodeData node, PCGGraph graph, HGraphCategoryData category)
        {
            if (graph.TryGetAttribute(node, category, out _))
                return;
            var neighbors = GetNeighbors(node, graph);

            float neighboringValues = 0;
            int weightedNeighbors = 0;
            foreach (var neighbor in neighbors)
            {
                if (graph.TryGetAttribute(neighbor, category, out var attribute))
                {
                    if (attribute.data.TryGetValue(out float value))
                    {
                        neighboringValues += value;
                        ++weightedNeighbors;
                    }
                }
            }
            float resultValue = neighboringValues / (weightedNeighbors + 1); // also weight self value as if its 0
            if (graph.TryCreateAttributeData(category, out var data))
            {
                if (data.data.TrySetValue(resultValue))
                    node.attributes.Add(data);
            }
        }
    }
}
