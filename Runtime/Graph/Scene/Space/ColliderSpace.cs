using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    [ExecuteInEditMode]
    public class ColliderSpace : Space
    {
        [SerializeField] Collider _collider;
        public Collider Collider => _collider;
        private void Awake()
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
        }
        public override bool IsPointInsideSpace(Vector3 point)
        {
            return _collider.ClosestPoint(point) == point;
        }

        public override float Distance(Vector3 point, out Vector3 closestPoint)
        {
            return Vector3.Distance(closestPoint = _collider.ClosestPoint(point), point);
        }

        public override Bounds ApproximateBounds => _collider.bounds;
    }
}
