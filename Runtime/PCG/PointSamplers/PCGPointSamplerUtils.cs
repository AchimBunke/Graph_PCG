using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public static class PCGPointSamplerUtils
    {
        public static bool IsPointInsideCollider(Vector3 point, Collider collider)
        {
            Vector3 closest = collider.ClosestPoint(point);
            return closest == point;
        }
    }
}

