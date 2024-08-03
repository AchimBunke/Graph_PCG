using System;
using System.Reflection;
using System.Xml.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    [XmlType(HGraphAttributeData.XmlNodeName)]
    public class HGraphAttributeData
    {
        [XmlIgnore]
        [NonSerialized]
        public const string XmlNodeName = "attribute";

        [XmlElement(nameof(category))]
        public string category;
        [XmlElement(nameof(type))]
        public HGraphAttributeType type;
        [XmlElement(nameof(data))]
        [SerializeReference]
        public HGraphAttributeContent data;
        public HGraphAttributeData()
        {

        }
    }
}


