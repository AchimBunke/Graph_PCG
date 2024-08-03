using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using System;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Distance
{
    public abstract class SpatialDistanceMeasure
    {
        public abstract float Distance(Vector3 a, Vector3 b);
        public virtual float Distance(HGraphNode node, Vector3 position)
        {
            if (node.SceneNode.Value == null)
                throw new ArgumentException("Cannot read spatial data!");
            var nodeSpace = node.SceneNode.Value.GetComponent<HGraphNodeSpace>();
            //Bounds bounds_a, bounds_b = new Bounds(position, Vector3.zero);
            if (nodeSpace != null)
            {
                var _ = nodeSpace.Distance(position, out var closest);
                return Distance(position, closest);
            }
            return Distance(node.SceneNode.Value.transform.position, position);
        }
        public virtual float Distance(HGraphNode a, HGraphNode b)
        {
            if (a.SceneNode.Value == null || b.SceneNode.Value == null)
                throw new ArgumentException("Cannot read spatial data!");
            var p_a = a.SceneNode.Value.GetComponent<HGraphNodeSpace>();
            var p_b = b.SceneNode.Value.GetComponent<HGraphNodeSpace>();
            if (p_a != null && p_b != null)
            {
                return Distance(p_a, p_b);
            }
            else
            {
                if (p_a != null && p_b == null)
                {
                    var _ = p_a.Distance(b.SceneNode.Value.transform.position, out var closest);
                    return Distance(b.SceneNode.Value.transform.position, closest);
                }
                if (p_a == null && p_b != null)
                {
                    var _ = p_b.Distance(a.SceneNode.Value.transform.position, out var closest);
                    return Distance(a.SceneNode.Value.transform.position, closest);
                }
                return Distance(a.SceneNode.Value.transform.position, b.SceneNode.Value.transform.position);
            }
        }
        public virtual float Distance(HGraphNodeSpace space_a, HGraphNodeSpace space_b)
        {
            space_a.Distance(space_b, out var point_a, out var point_b);
            return Distance(point_a, point_b);
        }
    }

    public class EuclideanSpatialDistance : SpatialDistanceMeasure
    {
        public override float Distance(Vector3 a, Vector3 b)
            => Vector3.Distance(a, b);
    }
    public class ManhattenSpatialDistance : SpatialDistanceMeasure
    {
        public override float Distance(Vector3 a, Vector3 b) {
            checked
            {
                return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y) + Mathf.Abs(a.z - b.z);
            }
        }
    }
}
