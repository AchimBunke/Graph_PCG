using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    [XmlType("category")]
    public class HGraphCategoryData : HGraphDataIdentifiable
    {
        [XmlElement(nameof(name))]
        public string name;
        [XmlElement(nameof(type))]
        public HGraphAttributeType type;
        [XmlElement(nameof(displayColor))]
        public Color displayColor;
        [XmlElement(nameof(defaultContent))]
        public HGraphAttributeContent defaultContent;
        [XmlElement(nameof(minValue))]
        public float minValue;
        [XmlElement(nameof(maxValue))]
        public float maxValue;
    }
}

