using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    [XmlType(HGraphNodeData.XmlNodeName)]
    public class HGraphNodeData : HGraphDataIdentifiable
    {
        [XmlIgnore]
        [NonSerialized]
        public const string XmlNodeName = "node";

        [XmlElement(nameof(name))]
        public string name;
        [XmlElement(nameof(superNode))]
        public string superNode;
        [XmlArray(nameof(attributes))]
        public List<HGraphAttributeData> attributes;
        [XmlArray(nameof(relations))]
        public List<string> relations;

        public HGraphSpaceData spaceData;

        public HGraphNodeData() { }
    }
}