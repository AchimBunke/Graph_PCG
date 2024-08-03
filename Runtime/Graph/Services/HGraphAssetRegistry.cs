using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System.Collections.Generic;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Services
{
    public class HGraphAssetRegistry
    {
        private static HGraphAssetRegistry _instance;
        public static HGraphAssetRegistry Instance => _instance ?? (_instance = new HGraphAssetRegistry());

        /// <summary>
        /// Guids - HGraphId (node)
        /// </summary>
        public Dictionary<string, string> PrefabAssets { get; set; } = new();

        /// <summary>
        /// Guids - AssetData
        /// </summary>
        public Dictionary<string, HGraphAssetData> Assets { get; set; } = new();
        public void RegisterAsset(string assetGUID, HGraphAssetData data) => Assets.Add(assetGUID, data);
    }
}