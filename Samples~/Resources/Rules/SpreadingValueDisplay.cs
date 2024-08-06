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