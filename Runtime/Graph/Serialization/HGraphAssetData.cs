using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    [Serializable]
    public class HGraphAssetData
    {
        [XmlElement("attributes")]
        [SerializeField]
        public HGraphAttributeData[] attributes;

        public HGraphAssetData() { }
    }
}

