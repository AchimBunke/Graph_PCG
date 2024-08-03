using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Interpolation
{

    public class InverseDistanceWeighting : HGraphAttributeInterpolationMethod
    {
        /// <summary>
        /// Higher Values => Weigth Distance more => Voronoi.
        /// Values <= 3 (Dimensions) cause the interpolated values to be dominated by points far away.
        /// https://en.wikipedia.org/wiki/Inverse_distance_weighting
        /// https://gisgeography.com/inverse-distance-weighting-idw-interpolation/
        /// </summary>
        public double PowerParameter { get; set; }
        public InverseDistanceWeighting(double powerParameter = 2.5)
        {
            PowerParameter = powerParameter;
        }
        public override FeatureVector InterpolateFeatures(Vector3 position, SpatialDistanceMeasure distanceMeasure, bool normalized = false)
        {
            var nodes = GetNodes().Where(n => n.SceneNode.Value != null);
            FeatureVector interpolatedVector = FeatureVector.Create();
            double totalWeigth = 0;
            Dictionary<string, double> perAttributeTotalWeight = new();
            foreach (var node in nodes)
            {
                var nodeFeatures = FeatureVectorUtil.CreateFeatureVector(node, normalized);
                var distance = distanceMeasure.Distance(node, position);
                if (distance == 0)
                    return nodeFeatures;
                var weight = 1d / Math.Pow(distance, PowerParameter);
                foreach(var cat in nodeFeatures.Data)
                {
                    if (!perAttributeTotalWeight.ContainsKey(cat.Key))
                        perAttributeTotalWeight[cat.Key] = 0;
                    perAttributeTotalWeight[cat.Key] += weight;
                }
                totalWeigth += weight;
                interpolatedVector += weight * nodeFeatures;
            }
            //interpolatedVector /= totalWeigth;
            foreach (var tw in perAttributeTotalWeight)
            {
                interpolatedVector /= (tw.Key, tw.Value);
            }
            return interpolatedVector;
        }
    }
    /// <summary>
    /// Inverse Distance Weighting but adjusted for Spaces where distance = 0.
    /// Normal IDW will take the attributes of the point where distance = 0 and does not interpolate other values.
    /// Adjusted to weight by offsetting distance by 1 so distances are [1,infinite]. Reduces higher impact of close proximity positions.
    /// </summary>
    public class SpaceAdjusted_InverseDistanceWeighting : HGraphAttributeInterpolationMethod
    {
        /// <summary>
        /// Higher Values => Weigth Distance more => Voronoi.
        /// Values <= 3 (Dimensions) cause the interpolated values to be dominated by points far away.
        /// https://en.wikipedia.org/wiki/Inverse_distance_weighting
        /// </summary>
        public double PowerParameter { get; set; }
        public SpaceAdjusted_InverseDistanceWeighting(double powerParameter = 2.5)
        {
            PowerParameter = powerParameter;
        }
        public override FeatureVector InterpolateFeatures(Vector3 position, SpatialDistanceMeasure distanceMeasure, bool normalized = false)
        {
            var nodes = GetNodes().Where(n => n.SceneNode.Value != null);
            FeatureVector interpolatedVector = FeatureVector.Create();
            double totalWeigth = 0;
            Dictionary<string, double> perAttributeTotalWeight = new();
            foreach (var node in nodes)
            {
                var nodeFeatures = FeatureVectorUtil.CreateFeatureVector(node, normalized);
                var distance = distanceMeasure.Distance(node, position);
                distance += 1;// Adjust
                var weight = 1d / Math.Pow(distance, PowerParameter);
                foreach (var cat in nodeFeatures.Data)
                {
                    if (!perAttributeTotalWeight.ContainsKey(cat.Key))
                        perAttributeTotalWeight[cat.Key] = 0;
                    perAttributeTotalWeight[cat.Key] += weight;
                }
                totalWeigth += weight;
                interpolatedVector += weight * nodeFeatures;
            }
            //interpolatedVector /= totalWeigth;
            foreach (var tw in perAttributeTotalWeight)
            {
                interpolatedVector /= (tw.Key, tw.Value);
            }
            return interpolatedVector;
        }
    }
}
