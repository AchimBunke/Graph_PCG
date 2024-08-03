using System;
using System.Xml.Serialization;

namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    [XmlType("rule")]
    public class HGraphRuleData
    {
        public string RuleGUID;
    }
}

