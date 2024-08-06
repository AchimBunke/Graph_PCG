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
