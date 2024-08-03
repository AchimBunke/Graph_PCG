﻿using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections.Generic;

namespace Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding
{
    public static class FeatureVectorUtil
    {
        public static double[] NormalizeFeatures(double[] features, float minValue, float maxValue)
        {
            for (int z = 0; z < features.Length; ++z)
            {
                features[z] = (features[z] - minValue) / (maxValue - minValue);
            }
            return features;
        }

        // TODO: maybe dont sort everytime but keep sorted array all along!
        /// <summary>
        /// Creates a feature Vector for all current exisitng categories!.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="instance"></param>
        /// <returns></returns>
        public static FeatureVector CreateFeatureVector(HGraphNode node, bool normalized = false, HGraph instance = null)
        {
            var graph = instance ?? HGraph.Instance;

            List <(HGraphAttributeType, double[]) > values = new();
            List<string> categories = new();

            foreach (var attributeKV in node.Attributes)
            {
                values.Add((attributeKV.Value.Type, attributeKV.Value.Data.EncodeFeatureVector()));
                categories.Add(attributeKV.Key);
            }
            var featureVector = FeatureVector.Create(categories.ToArray(), values.ToArray());

            if (normalized)
                return featureVector.Normalized();
            else
                return featureVector;
        }
        public static FeatureVector CreateFeatureVector(HGraphAssetData assetData, bool normalized=false, HGraph instance = null) 
        {
            var graph = instance ?? HGraph.Instance;
            List<(HGraphAttributeType, double[])> values = new();
            List<string> categories = new();
            if (assetData.attributes != null)
            {
                foreach (var attribute in assetData.attributes)
                {
                    var attKey = attribute.category;
                    if (graph.Categories.ContainsKey(attKey))
                    {
                        values.Add((attribute.type, attribute.data.EncodeFeatureVector()));
                        categories.Add(attKey);
                    }
                }
            }
            var featureVector = FeatureVector.Create(categories.ToArray(), values.ToArray());

            if (normalized)
                return featureVector.Normalized();
            else
                return featureVector;
        }
        public static FeatureVector CreateFeatureVector(HGraphNodeData nodeData, PCGGraph graph, bool normalized = false)
        {
            List<(HGraphAttributeType, double[])> values = new();
            List<string> categories = new();
            if (nodeData.attributes != null)
            {
                foreach (var attribute in nodeData.attributes)
                {
                    var attKey = attribute.category;
                    if (graph.Categories.ContainsKey(attKey))
                    {
                        values.Add((attribute.type, attribute.data.EncodeFeatureVector()));
                        categories.Add(attKey);
                    }
                }
            }
            var featureVector = FeatureVector.Create(categories.ToArray(), values.ToArray());

            if (normalized)
                return featureVector.Normalized();
            else
                return featureVector;
        }

        //public static IEnumerable<KeyValuePair<string, Tuple<double[], HGraphAttributeType>>> ExtractAttributeFeatures(FeatureVector featureVector)
        //{
        //    for (int i = 0; i < featureVector.hGraphCategories.Length; ++i)
        //    {
        //        string c = featureVector.hGraphCategories[i];
        //        HGraphAttributeType type = featureVector.attributeTypes[i];
        //        double[] f = new double[featureVector.attributeTypes[i].GetFeatureLength()];
        //        for (int z = 0; z < f.Length; ++z)
        //        {
        //            f[z] = featureVector.features[i + z];
        //        }
        //        yield return KeyValuePair.Create(c, Tuple.Create(f, type));
        //    }
        //}
    }
}
