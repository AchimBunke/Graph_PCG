using System;
using System.Collections.Generic;
using System.Xml.Serialization;


namespace Achioto.Gamespace_PCG.Runtime.HGraph.Serialization
{
    [Serializable]
    [XmlType("enum")]
    public class HGraphEnumData : HGraphDataIdentifiable
    {
        [XmlArray(nameof(Entries))]
        public List<EnumEntry> Entries;
        [XmlArray(nameof(Flags))]
        public bool Flags;
    }
}

