using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Database
{
    public class PCGNodeDistanceAssetFilter
    {
        [Serializable]
        public struct PCGNodeDistanceAssetFilterSettings
        {
            [Min(1)]
            public int topKAssets;
            public FeatureDistanceMeasureConfiguration featureDistanceConfig;

            public static PCGNodeDistanceAssetFilterSettings Default => new PCGNodeDistanceAssetFilterSettings()
            {
                topKAssets = 1,
                featureDistanceConfig = FeatureDistanceMeasureConfiguration.Default,
            };
        }
        public PCGNodeDistanceAssetFilterSettings Settings { get; set; }


        public PCGNodeDistanceAssetFilter()
        {
        }

        public IEnumerable<string> FilterAssets(IEnumerable<string> assetPaths, HGraphNodeData node)
        {
            var featureDistanceMesasure = Settings.featureDistanceConfig.Create();
            List<(string guid, HGraphAssetData data)> assets = new(assetPaths.Select(path =>
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                HGraphAssetRegistry.Instance.Assets.TryGetValue(guid, out var assetData);
                return (guid, assetData);
            }));
            assets.RemoveAll(a => a.data == null);
            var nodeFeatures = FeatureVectorUtil.CreateFeatureVector(node, PCGGraphManager.Instance.PCGGraph, normalized: true);
            assets.Sort((a, b) =>
            {
                var fa = FeatureVectorUtil.CreateFeatureVector(a.data, normalized: true);
                var da = featureDistanceMesasure.FeatureDistance(fa, nodeFeatures, normalized: true);

                var fb = FeatureVectorUtil.CreateFeatureVector(b.data, normalized: true);
                var db = featureDistanceMesasure.FeatureDistance(fb, nodeFeatures, normalized: true);

                return da.CompareTo(db);

            });
            return assets.Select(a => AssetDatabase.GUIDToAssetPath(a.guid)).Take(Settings.topKAssets);
        }


    }


}