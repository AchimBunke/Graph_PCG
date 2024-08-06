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