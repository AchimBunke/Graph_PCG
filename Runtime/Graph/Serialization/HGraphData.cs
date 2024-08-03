using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    public class HGraphData
    {
        [XmlArray(nameof(enumData))]
        public List<HGraphEnumData> enumData;
        [XmlArray(nameof(categories))]
        public List<HGraphCategoryData> categories;
        [XmlArray(nameof(nodes))]
        public List<HGraphNodeData> nodes;
        [XmlArray(nameof(relations))]
        public List<HGraphRelationData> relations;

        public string rulesGUID;

    }
}

