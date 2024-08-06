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
using UnityEngine;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    [CreateAssetMenu(fileName = "InferredValue", menuName = "ScriptableObjects/HGraph-Rules/InferredValue", order = 1)]
    public class InferredValueRule : HGraphNodeDataRule
    {
        [SerializeField] string computedCategoryName;
        [SerializeField] string spreadingValueCategoryName;
        public override void Apply(HGraphNodeData nodeData, PCGGraph graph)
        {
            if (!graph.Categories.TryGetValue(computedCategoryName, out var computedCategory))
                return;

            if (!graph.TryGetAttribute(nodeData, spreadingValueCategoryName, out var spreadingAtt))
                return;
            var value = spreadingAtt.data.GetValue();
            if (value is not float fv)
                return;
            if (!graph.TryGetAttribute(nodeData, computedCategoryName, out var att))
            {
                graph.TryCreateAttributeData(computedCategory, out att);
                nodeData.attributes.Add(att);
            }
            att.data.TrySetValue(fv * -1);
        }
    }
}