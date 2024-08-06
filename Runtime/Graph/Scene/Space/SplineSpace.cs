/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

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
    