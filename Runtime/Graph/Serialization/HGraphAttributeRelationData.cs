using System;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    [XmlType("attributeRelation")]
    public class HGraphAttributeRelationData : HGraphDataIdentifiable
    {
        [XmlElement(nameof(category))]
        public string category;
        [XmlElement(nameof(data))]
        public HGraphAttributeRelationContent data;
        public HGraphAttributeRelationData() { }
    }
}

