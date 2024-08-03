using System;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    public class HGraphAttributeRelationContent
    {
        //TODO: [ReadOnlyField]
        public HGraphAttributeRelationType type;
    }
    [Serializable]
    public class HGraphAttributeRelationContent_Step : HGraphAttributeRelationContent
    {
        [SerializeField]
        [Range(0f, 1f)]
        public float step;
    }



}
