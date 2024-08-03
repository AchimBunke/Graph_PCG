using System;
using System.Xml.Serialization;


namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    public class HGraphDataIdentifiable
    {
        [XmlElement(nameof(id))]
        public string id;
        public HGraphDataIdentifiable() { }
    }
}
