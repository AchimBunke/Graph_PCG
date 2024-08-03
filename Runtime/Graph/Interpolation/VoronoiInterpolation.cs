using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using System.Linq;
using UnityEngine;
using UnityUtilities.NetBase;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Interpolation
{
    public class VoronoiInterpolation : HGraphAttributeInterpolationMethod
    {
        public override FeatureVector InterpolateFeatures(Vector3 position, SpatialDistanceMeasure spatialDistanceMeasure, bool normalized = false)
        {
            var sceneFeatures = GetNodes().Where(n => n.SceneNode.Value != null);
            if (sceneFeatures.Count() > 0)
                return FeatureVectorUtil.CreateFeatureVector(sceneFeatures.MinBy((n) => spatialDistanceMeasure.Distance(n, position)), normalized);
            else return FeatureVector.Create();
        }
    }
}
