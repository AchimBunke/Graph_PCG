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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    [ExecuteInEditMode]
    public class SpreadingValueDisplay : MonoBehaviour
    {
        [SerializeField] string spreadingCategoryName;
        [SerializeField] string computedCategoryName;
        Dictionary<string, float> nodeSpreadingValuePairs = new();
        Dictionary<string, float> nodeComputedValuePairs = new();

        private void OnDrawGizmos()
        {
            foreach (var pair in nodeSpreadingValuePairs)
            {
                if (HGraph.Instance.Nodes.TryGetValue(pair.Key, out var node))
                {
                    if (node.SceneNode.Value != null)
                        Handles.Label(node.SceneNode.Value.transform.position + new Vector3(0, 1, 0), "SpreadingValue = " + pair.Value.ToString());              
                }
            }
            foreach (var pair in nodeComputedValuePairs)
            {
                if (HGraph.Instance.Nodes.TryGetValue(pair.Key, out var node))
                {     
                    if (node.SceneNode.Value != null)
                        Handles.Label(node.SceneNode.Value.transform.position + new Vector3(0, 1.5f, 0), "Inferred Value = " + pair.Value.ToString());
                }
            }
        }
        private void Update()
        {
            nodeSpreadingValuePairs.Clear();
            nodeComputedValuePairs.Clear();
            var graph = PCGGraphManager.Instance.PCGGraph;
            foreach (var node in graph.Nodes.Values)
            {
                if (graph.TryGetAttribute(node, spreadingCategoryName, out var spreadingAtt))
                {
                    var value = spreadingAtt.data.GetValue();
                    if (value is float fv)
                        nodeSpreadingValuePairs.Add(node.id, fv);
                }
                if (graph.TryGetAttribute(node, computedCategoryName, out var computedAtt))
                {
                    var value = computedAtt.data.GetValue();
                    if (value is float fv)
                        nodeComputedValuePairs.Add(node.id, fv);
                }
            }
        }
    }
}