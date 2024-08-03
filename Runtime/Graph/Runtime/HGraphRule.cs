using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    public abstract class HGraphRule : ScriptableObject
    {
        [field: SerializeField]
        public int ExecutionPriority { get; set; } = 0;

        [field: SerializeField]
        public bool ApplyRule { get; set; } = true;
    }

    public abstract class HGraphGlobalRule : HGraphRule
    {
        public abstract void Apply(PCGGraph graph);
    }
    public abstract class HGraphNodeDataRule : HGraphRule
    {
        public abstract void Apply(HGraphNodeData nodeData, PCGGraph graph);
    }



}
