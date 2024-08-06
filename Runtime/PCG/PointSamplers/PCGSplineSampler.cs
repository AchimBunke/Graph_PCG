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

using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public class PCGSplineSampler : PCGPointSampler
    {
        //[SerializeField] SplineContainer _splineContainer;
        //[SerializeField] int _splineIndex = 0;
        //[SerializeField] float _splineRadius = 0;
        [SerializeField] SplineSpace _splineSpace;

        [SerializeField, Tooltip("Relates to the cell size. Higher value responds to more distance between samples")] float _pointExtends = 1f;
        [SerializeField, Tooltip("Relates to the chance of sampling a cell")] float _pointsPerSquareMeter = 1;
        [SerializeField] bool _projectToSurface;
        [SerializeField] float _maxSurfaceDistance = 100;
        [SerializeField] LayerMask _surfaceMask = Physics.AllLayers;
        [SerializeField] bool _drawSplineGizmos;

        public float CellSize => _pointExtends;
        public Spline Spline => _splineSpace.Spline;

        public int GetSplineStepCount()
        {
            if (CellSize <= 0)
                return -1;
            var spline = Spline;
            var length = spline.GetLength();
            return 1 + (int)(length / CellSize);
        }
        //public int GetCellCount()
        //{
        //    if (_splineContainer == null || _splineContainer.Splines.Count >= _splineIndex || _pointExtends <= 0)
        //        return -1;
        //    var spline = _splineContainer[_splineIndex];
        //    var splineLength = spline.GetLength();

        //    var sizeX = bounds.extents.x * 2;
        //    var sizeZ = bounds.extents.z * 2;
        //    var numCellsX = (int)(1 + (sizeX / CellSize));// min. 1 cell
        //    var numCellsZ = (int)(1 + (sizeZ / CellSize));// min. 1 cell
        //    return numCellsX * numCellsZ;
        //}

        public override IEnumerable<PCGPoint> SamplePoints()
        {
            List<PCGPoint> points = new List<PCGPoint>();

            if (_pointsPerSquareMeter <= 0)
            {
                Debug.LogWarning("Invalid PointsPreSquaredMeter: No Points to sample.");
                return points;
            }
            if (CellSize <= 0)
            {
                Debug.LogWarning("Invalid CellSize: No Points to sample.");
                return points;
            }
            if (_splineSpace == null || Spline == null)
            {
                Debug.LogError("No Spline to sample from.");
                return points;
            }
            var targetPointCount = GetSplineStepCount() * _pointsPerSquareMeter;
            var pointPerCellRatio = Mathf.Clamp(targetPointCount / GetSplineStepCount(), 0, 1);
            if (pointPerCellRatio <= 0)
                return points;
            var splineRadius = _splineSpace.SplineRadius;
            Action<float3, float3, float3> cellAction = (float3 pos, float3 tangent, float3 up) =>
            {
                if (Random.Range(0f, 1f) >= pointPerCellRatio)// Check if sample current cell by chance
                    return;
                var halfCellSize = CellSize / 2;
                var randomOffset = new Vector3(
                        Random.Range(-splineRadius, splineRadius),
                        Random.Range(-splineRadius, splineRadius),
                        Random.Range(-splineRadius, splineRadius));
                var right = Vector3.Cross(new Vector3(tangent.x, tangent.y, tangent.z).normalized, new Vector3(up.x, up.y, up.z).normalized).normalized;
                var r = Random.insideUnitCircle * splineRadius;
                randomOffset = right * r.x + new Vector3(up.x, up.y, up.z) * r.y;
                var randomSplinePoint = _splineSpace.SplineContainer.transform.TransformPoint(new Vector3(pos.x, pos.y, pos.z)) + randomOffset;

                if (_projectToSurface)
                {
                    Vector3 rayStartPoint = new Vector3(randomSplinePoint.x, randomSplinePoint.y, randomSplinePoint.z);
                    if (Physics.Raycast(rayStartPoint, Vector3.down, out var hit, _maxSurfaceDistance, _surfaceMask, QueryTriggerInteraction.Ignore))
                    {
                        var point = new PCGPoint()
                        {
                            Position = hit.point,
                            Normal = hit.normal,
                            Extends = _pointExtends
                        };
                        points.Add(point);
                    }
                }
                else
                {
                    var point = new PCGPoint()
                    {
                        Position = randomSplinePoint,
                        Normal = _splineSpace.SplineContainer.transform.TransformDirection(new Vector3(up.x, up.y, up.z)),
                        Extends = _pointExtends,
                    };
                    points.Add(point);
                }

            };
            ForEachSplinePoint(cellAction);
            return points;
        }
        private void ForEachSplinePoint(Action<float3, float3, float3> action)
        {
            if (CellSize <= 0)
                return;
            var spline = Spline;
            var length = spline.GetLength();
            var stepCount = length / CellSize;
            var stepIncrement = CellSize / length;
            for (int i = 0; i < stepCount; ++i)
            {
                if (spline.Evaluate(i * stepIncrement, out float3 position, out float3 tangent, out float3 up))
                {
                    action(position, tangent, up);
                }
            }

        }
        private void OnDrawGizmos()
        {
            if (_drawSplineGizmos)
            {

                if (_splineSpace != null && _splineSpace.Spline != null && CellSize > 0)
                {
                    if (GetSplineStepCount() > 10000)
                    {
                        Debug.LogWarning("Does not draw grid as it has too many voxels!");
                    }
                    else
                    {
                        //var centerOffset = CellSize * Vector3.one * 0.5f;
                        ForEachSplinePoint((pos, tangent, up) =>
                        {
                            var pointCenter = _splineSpace.SplineContainer.transform.TransformPoint(new Vector3(pos.x, pos.y, pos.z));

                            if (_projectToSurface)
                            {
                                Vector3 rayStartPoint = pointCenter;
                                if (Physics.Raycast(rayStartPoint, Vector3.down, out var hit, _maxSurfaceDistance, _surfaceMask, QueryTriggerInteraction.Ignore))
                                {
                                    Gizmos.DrawCube(hit.point, Vector3.one * CellSize * 0.3f);
                                }
                            }
                            else
                            {
                                Gizmos.DrawCube(pointCenter, Vector3.one * CellSize * 0.3f);
                            }
                        });
                    }
                }
            }
        }
    }
}