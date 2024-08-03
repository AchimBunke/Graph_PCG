using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Interpolation
{
    public abstract class HGraphAttributeInterpolationMethod
    {
        public IEnumerable<HGraphNode> NodeSource { get; set; } = HGraph.Instance.Nodes.Values;
        protected IEnumerable<HGraphNode> GetNodes() => NodeSource;
        protected IEnumerable<FeatureVector> GetFeatures(bool normalized = false)
            => GetNodes().Select(n => FeatureVectorUtil.CreateFeatureVector(n, normalized));
        public abstract FeatureVector InterpolateFeatures(Vector3 position, SpatialDistanceMeasure distanceMeasure, bool normalized = false);
    }
}
