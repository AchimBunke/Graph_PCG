using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    [XmlType("relation")]
    public class HGraphRelationData : HGraphDataIdentifiable
    {
        [XmlElement(nameof(source))]
        public string source;
        [XmlElement(nameof(target))]
        public string target;
        [XmlArray(nameof(attributeRelations))]
        public List<HGraphAttributeRelationData> attributeRelations;
    }
}

