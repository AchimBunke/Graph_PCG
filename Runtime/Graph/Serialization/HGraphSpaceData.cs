using System;
using System.Xml.Serialization;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    public enum ColliderType
    {
        BoxCollider,
        SphereCollider,
        CapsuleCollider,
    }

    [Serializable]
    [XmlType(HGraphSpaceData.XmlSpaceName)]
    public class HGraphSpaceData
    {
        [XmlIgnore]
        [NonSerialized]
        public const string XmlSpaceName = "spaceData";

        public bool isAtomic;
        public bool isImplicit;
        public Vector3 nodePosition;

        // SpaceData
        public ColliderType colliderType;

        public Vector3 center;
        public Vector3 size; // For BoxCollider
        public float radius; // For SphereCollider and CapsuleCollider
        public float height; // For CapsuleCollider
        public int direction; // For CapsuleCollider


        #region Operations
        public bool IsPointInsideSpace(Vector3 point)
        {
            if (isImplicit || isAtomic)
                return false;
            switch (colliderType)
            {
                case ColliderType.BoxCollider:
                    return IsPointInsideBox(point);
                case ColliderType.SphereCollider:
                    return IsPointInsideSphere(point);
                case ColliderType.CapsuleCollider:
                    return IsPointInsideCapsule(point);
                default:
                    return false;
            }
        }
        private Vector3 GetColliderGlobalCenter() => nodePosition + center;

        public float Distance(Vector3 point, out Vector3 closestPoint)
        {
            if (isAtomic)
            {
                closestPoint = point;
                return Vector3.Distance(point, GetColliderGlobalCenter());
            }
            if (!isImplicit)
                return DistanceToCollider(point, out closestPoint);
            throw new InvalidOperationException();
        }
        private float DistanceToCollider(Vector3 point, out Vector3 closestPoint)
        {
            switch (colliderType)
            {
                case ColliderType.BoxCollider:
                    return DistanceToPointBox(point, out closestPoint);
                case ColliderType.SphereCollider:
                    return DistanceToPointSphere(point, out closestPoint);
                case ColliderType.CapsuleCollider:
                    return DistanceToPointCapsule(point, out closestPoint);
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
        private float DistanceToPointSphere(Vector3 point, out Vector3 closestPoint)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 direction = (point - globalCenter).normalized;
            closestPoint = globalCenter + direction * radius;
            float distance = Vector3.Distance(closestPoint, point);
            return Mathf.Abs(distance);
        }
        private float DistanceToPointBox(Vector3 point, out Vector3 closestPoint)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 localPoint = point;
            Vector3 halfSize = size * 0.5f;
            Vector3 min = globalCenter - halfSize;
            Vector3 max = globalCenter + halfSize;

            closestPoint = localPoint;
            closestPoint.x = Mathf.Clamp(localPoint.x, min.x, max.x);
            closestPoint.y = Mathf.Clamp(localPoint.y, min.y, max.y);
            closestPoint.z = Mathf.Clamp(localPoint.z, min.z, max.z);

            float sqrDist = 0.0f;

            // Calculate squared distance from point to closest point on the box
            for (int i = 0; i < 3; i++)
            {
                float v = localPoint[i];
                if (v < min[i]) sqrDist += (min[i] - v) * (min[i] - v);
                if (v > max[i]) sqrDist += (v - max[i]) * (v - max[i]);
            }

            return Mathf.Sqrt(sqrDist);
        }
        private float DistanceToPointCapsule(Vector3 point, out Vector3 closestPoint)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 dir = Vector3.zero;
            switch (direction)
            {
                case 0: // X-axis
                    dir = Vector3.right;
                    break;
                case 1: // Y-axis
                    dir = Vector3.up;
                    break;
                case 2: // Z-axis
                    dir = Vector3.forward;
                    break;
            }

            float halfHeight = height * 0.5f - radius;
            Vector3 lineStart = globalCenter - dir * halfHeight;
            Vector3 lineEnd = globalCenter + dir * halfHeight;

            Vector3 closestPointOnLine = ClosestPointOnLineSegment(lineStart, lineEnd, point);
            Vector3 directionToPoint = (point - closestPointOnLine).normalized;
            closestPoint = closestPointOnLine + directionToPoint * radius;

            float distance = Vector3.Distance(closestPoint, point);
            return Mathf.Max(0, distance - radius);
        }
        private bool IsPointInsideBox(Vector3 point)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 halfSize = size * 0.5f;
            Vector3 min = globalCenter - halfSize;
            Vector3 max = globalCenter + halfSize;

            return point.x >= min.x && point.x <= max.x &&
                   point.y >= min.y && point.y <= max.y &&
                   point.z >= min.z && point.z <= max.z;
        }
        private bool IsPointInsideSphere(Vector3 point)
        {
            var globalCenter = GetColliderGlobalCenter();
            float distanceSquared = (point - globalCenter).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }
        private bool IsPointInsideCapsule(Vector3 point)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 dir = Vector3.zero;
            switch (direction)
            {
                case 0: // X-axis
                    dir = Vector3.right;
                    break;
                case 1: // Y-axis
                    dir = Vector3.up;
                    break;
                case 2: // Z-axis
                    dir = Vector3.forward;
                    break;
            }

            float halfHeight = height * 0.5f - radius;
            Vector3 lineStart = globalCenter - dir * halfHeight;
            Vector3 lineEnd = globalCenter + dir * halfHeight;

            Vector3 closestPoint = ClosestPointOnLineSegment(lineStart, lineEnd, point);
            float distanceSquared = (point - closestPoint).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }
        private Vector3 ClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 lineDir = end - start;
            float t = Vector3.Dot(point - start, lineDir) / lineDir.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return start + t * lineDir;
        }
        #endregion
    }
}
