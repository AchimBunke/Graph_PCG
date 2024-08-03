using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    public abstract class Space : MonoBehaviour
    {
        public abstract Bounds ApproximateBounds { get; }
        public abstract bool IsPointInsideSpace(Vector3 point);
        public float Distance(Vector3 point) => Distance(point, out _);

        public abstract float Distance(Vector3 point, out Vector3 closestPoint);

        /// <summary>
        /// https://discussions.unity.com/t/shortest-distance-between-two-meshes-colliders/247640/4
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public virtual float Distance(Space space, out Vector3 closestPoint, out Vector3 closestPoint_space)
        {
            return Gamespace_PCG.Runtime.Utils.GeometryUtility.Distance(ApproximateBounds, space.ApproximateBounds, out closestPoint, out closestPoint_space);
        }
    }
}