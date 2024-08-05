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
        public Quaternion nodeRotation;
        public Vector3 nodeScale;

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
        private Vector3 GetColliderGlobalCenter() => TransformPoint(center);
        
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
                    return DistancePointToBox(point, out closestPoint);
                case ColliderType.SphereCollider:
                    return DistancePointToSphere(point, out closestPoint);
                case ColliderType.CapsuleCollider:
                    return DistanceToPointCapsule(point, out closestPoint);
                default:
                    {
                        throw new NotImplementedException();
                    }
            }
        }
        private float DistancePointToSphere(Vector3 point, out Vector3 closestPoint)
        {
            var globalCenter = GetColliderGlobalCenter();
            Vector3 direction = (point - globalCenter).normalized;
            closestPoint = globalCenter + direction * radius * Mathf.Max(nodeScale.x, nodeScale.y, nodeScale.z);
            float distance = Vector3.Distance(closestPoint, point);
            return Mathf.Abs(distance);
        }
        private float DistancePointToBox(Vector3 point, out Vector3 closestPoint)
        {
            Vector3 localPoint = InverseTransformPoint(point);

            Vector3 scaledSize = Vector3.Scale(size, nodeScale);

            Vector3 halfSize = scaledSize * 0.5f;
            Vector3 min = halfSize;
            Vector3 max = halfSize;

            Vector3 closestPointLocal = localPoint;
            closestPointLocal.x = Mathf.Clamp(localPoint.x, min.x, max.x);
            closestPointLocal.y = Mathf.Clamp(localPoint.y, min.y, max.y);
            closestPointLocal.z = Mathf.Clamp(localPoint.z, min.z, max.z);

            float sqrDist = 0.0f;

            // Calculate squared distance from point to closest point on the box
            for (int i = 0; i < 3; i++)
            {
                float v = localPoint[i];
                if (v < min[i]) sqrDist += (min[i] - v) * (min[i] - v);
                if (v > max[i]) sqrDist += (v - max[i]) * (v - max[i]);
            }
            closestPoint = TransformPoint(closestPointLocal);
            return Mathf.Sqrt(sqrDist);
        }
        private float DistanceToPointCapsule(Vector3 point, out Vector3 closestPoint)
        {
            Vector3 localPoint = InverseTransformPoint(point);

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

            float halfHeight = (height * nodeScale[direction]) * 0.5f - radius;
            Vector3 lineStart = dir * halfHeight;
            Vector3 lineEnd = dir * halfHeight;

            Vector3 closestPointOnLine = ClosestPointOnLineSegment(lineStart, lineEnd, localPoint);
            Vector3 directionToPoint = (localPoint - closestPointOnLine).normalized;
            var closestPointLocal = closestPointOnLine + directionToPoint * radius;
            closestPoint = TransformPoint(closestPointLocal);

            float distance = Vector3.Distance(closestPoint, point);
            return Mathf.Max(0, distance - radius);
        }
        private bool IsPointInsideBox(Vector3 point)
        {
            Vector3 localPoint = InverseTransformPoint(point);

            Vector3 halfSize = size * 0.5f;
            Vector3 min = -halfSize;
            Vector3 max = halfSize;

            return localPoint.x >= min.x && localPoint.x <= max.x &&
                   localPoint.y >= min.y && localPoint.y <= max.y &&
                   localPoint.z >= min.z && localPoint.z <= max.z;
        }
        private bool IsPointInsideSphere(Vector3 point)
        {
            Vector3 localPoint = InverseTransformPoint(point);
            float distanceSquared = (localPoint - center).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }
        private bool IsPointInsideCapsule(Vector3 point)
        {
            Vector3 localPoint = InverseTransformPoint(point);

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

            float halfHeight = (height * nodeScale[direction] * 0.5f) - radius;

            Vector3 lineStart = - dir * halfHeight;
            Vector3 lineEnd = dir * halfHeight;

            Vector3 closestPoint = ClosestPointOnLineSegment(lineStart, lineEnd, localPoint);
            float distanceSquared = (localPoint - closestPoint).sqrMagnitude;
            return distanceSquared <= radius * radius;
        }
        private Vector3 ClosestPointOnLineSegment(Vector3 start, Vector3 end, Vector3 point)
        {
            Vector3 lineDir = end - start;
            float t = Vector3.Dot(point - start, lineDir) / lineDir.sqrMagnitude;
            t = Mathf.Clamp01(t);
            return start + t * lineDir;
        }

        /// <summary>
        /// Transform a point from world space to local space
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector3 InverseTransformPoint(Vector3 point)
        {
            Vector3 localPoint = Quaternion.Inverse(nodeRotation) * (point - nodePosition);
            localPoint.x /= nodeScale.x;
            localPoint.y /= nodeScale.y;
            localPoint.z /= nodeScale.z;
            return localPoint;
        }
        /// <summary>
        /// Transform a point from local space to world space
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private Vector3 TransformPoint(Vector3 point)
        {
            return nodePosition + nodeRotation * Vector3.Scale(point, nodeScale);
        }
        #endregion
    }
}
