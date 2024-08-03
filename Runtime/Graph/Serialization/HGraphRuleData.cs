using System;
using System.Xml.Serialization;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    [XmlType("rule")]
    public class HGraphRuleData
    {
        public string RuleGUID;
    }
}

