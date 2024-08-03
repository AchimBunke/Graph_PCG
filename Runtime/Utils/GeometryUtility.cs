using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Utils
{
    public static class GeometryUtility
    {
        /// <summary>
        /// https://discussions.unity.com/t/shortest-distance-between-two-meshes-colliders/247640/4
        /// !! Approximation not totally accurate
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static float Distance(Bounds b1, Bounds b2, out Vector3 closestPoint_b1, out Vector3 closestPoint_b2)
        {
            float sd0 = sdBounds(b1.center, b2, out closestPoint_b1);
            float sd1 = sdBounds(b2.center, b1, out closestPoint_b2);
            return Vector3.Distance(closestPoint_b1, closestPoint_b2);
        }
        static float sdBounds(Vector3 point, Bounds bounds, out Vector3 contact)
        {
            Vector3 dir = point - bounds.center;
            float sd = sdBox(dir, bounds.extents);

            contact = point - dir.normalized * sd;
            // note: we dont need to know the real contact point in this case, this is pure conjecture

            return sd;
        }

        // src: https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        static float sdBox(Vector3 p, Vector3 b)
        {
            Vector3 q = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z)) - b;
            return Vector3.Magnitude(Vector3.Max(q, Vector3.zero)) + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f);
        }
    }
}

