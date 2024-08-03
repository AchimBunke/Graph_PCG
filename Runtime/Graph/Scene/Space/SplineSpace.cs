using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;


namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    /// <summary>
    /// TODO
    /// </summary>

    [ExecuteInEditMode]
    public class SplineSpace : Space
    {
        [SerializeField] SplineContainer _splineContainer;
        [SerializeField] int _splineIndex = 0;
        [SerializeField] float _splineRadius = 0;
        [Range(SplineUtility.PickResolutionMin, SplineUtility.PickResolutionMax)]
        [SerializeField] int _splineResolution = SplineUtility.PickResolutionDefault;
        [Range(SplineUtility.PickResolutionMin, SplineUtility.PickResolutionMax)]
        [SerializeField] int _splineIterations = 2;


        public Spline Spline => _splineContainer[_splineIndex];
        public float SplineRadius => _splineRadius;
        public SplineContainer SplineContainer => _splineContainer;
        private void Awake()
        {
            if (_splineContainer == null)
                _splineContainer = GetComponent<SplineContainer>();
        }
        public override bool IsPointInsideSpace(Vector3 point)
        {
            return Distance(point) <= _splineRadius;
        }

        public override float Distance(Vector3 point, out Vector3 closestPoint)
        {
            var d = SplineUtility.GetNearestPoint(Spline, point, out float3 closest, out float t, _splineResolution, _splineIterations);
            closestPoint = closest;
            return d;
        }

        public override Bounds ApproximateBounds => Spline.GetBounds();
    }

}
    