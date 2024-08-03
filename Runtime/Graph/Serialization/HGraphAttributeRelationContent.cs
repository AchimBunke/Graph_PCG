using System;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    public class HGraphAttributeRelationContent
    {
        [ReadOnlyField]
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
