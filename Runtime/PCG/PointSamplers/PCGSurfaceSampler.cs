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

using ALib.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityUtilities.UnityBase;
using Random = UnityEngine.Random;
using Space = Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Space;


namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    /// <summary>
    /// Cell based approach from https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Plugins/PCG/Source/PCG/Private/Elements/PCGSurfaceSampler.cpp
    /// </summary>
    public class PCGSurfaceSampler : PCGPointSampler
    {
        [SerializeField] LayerMask _surfaceMask = Physics.AllLayers;
        [SerializeField] Collider _targetSurface;
        [SerializeField] Space _space;
        [SerializeField] PCGSampleMode _sampleMode;
        [SerializeField, Tooltip("Relates to the cell size. Higher value responds to more distance between samples")] float _pointExtends = 1f;
        [SerializeField, Tooltip("Relates to the chance of sampling a cell")] float _pointsPerSquareMeter = 1;
        [SerializeField] bool _drawCellGizmos;
        [SerializeField] int _maxDebugCubeCount = 10000;
        [SerializeField] float _cullDistance = 50f;

        //public Collider SampleVolume => _sampleVolume;
        public LayerMask SurfaceMask => _surfaceMask;
        public float CellSize => _pointExtends;
        public int GetCellCount()
        {
            if (_space == null || _pointExtends <= 0)
                return -1;
            var bounds = _space.ApproximateBounds;
            var sizeX = bounds.extents.x * 2;
            var sizeZ = bounds.extents.z * 2;
            var numCellsX = (int)(1 + (sizeX / CellSize));// min. 1 cell
            var numCellsZ = (int)(1 + (sizeZ / CellSize));// min. 1 cell
            return numCellsX * numCellsZ;
        }

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
            if (_space == null)
            {
                if ((_space = GetComponent<Space>()) == null)
                {
                    Debug.LogError("No Volume to sample from.");
                    return points;
                }
            }
            var bounds = _space.ApproximateBounds;
            var targetPointCount = (bounds.extents.x * 2) * (bounds.extents.z * 2) * _pointsPerSquareMeter;
            var pointPerCellRatio = Mathf.Clamp(targetPointCount / (float)GetCellCount(), 0, 1);
            if (pointPerCellRatio <= 0)
                return points;
            Action<Vector2> cellAction = (Vector2 cell) =>
            {
                // check all avaiulable surfaces
                if (_sampleMode == PCGSampleMode.Down || _sampleMode == PCGSampleMode.UpDown)
                {
                    Vector3 rayStartPoint = new Vector3(cell.x, bounds.max.y, cell.y);
                    RaycastHit[] surfacePoints;
                    if (_targetSurface != null)
                    {
                        
                        if (_targetSurface.Raycast(new Ray(rayStartPoint, Vector3.down), out var hit, bounds.extents.y * 2f))
                        {
                            surfacePoints = new RaycastHit[] { hit };
                        }
                        else
                        {
                            surfacePoints = new RaycastHit[0];
                        }
                    }
                    else
                    {
                        surfacePoints = Physics.RaycastAll(rayStartPoint, Vector3.down, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);
                    }

                    surfacePoints = surfacePoints.DistinctBy(h => h.collider).ToArray();
                    foreach (var surface in surfacePoints)// for each surface evaluate if sample. Allows layered surface above each other
                    {
                        if (Random.Range(0f, 1f) >= pointPerCellRatio)// Check if sample current cell by chance
                            continue;
                        var randomOffset = new Vector3(
                            Random.Range(0, CellSize),
                            0,
                            Random.Range(0, CellSize));
                        var randomCellRayOrigin = rayStartPoint + randomOffset;
                        var ray = new Ray(randomCellRayOrigin, Vector3.down);
                        if (surface.collider.Raycast(ray, out var info, bounds.extents.y * 2))// perform raycast against the target surface
                        {
                            if (!_space.IsPointInsideSpace(info.point))// Skip if in bounds but outside sample volume
                                continue;
                            var point = new PCGPoint()
                            {
                                Position = info.point,
                                Normal = info.normal,
                                Extends = _pointExtends
                            };
                            points.Add(point);
                        }
                    }
                }
                if (_sampleMode == PCGSampleMode.Up || _sampleMode == PCGSampleMode.UpDown)
                {
                    Vector3 rayStartPoint = new Vector3(cell.x, bounds.min.y, cell.y);
                    RaycastHit[] surfacePoints;
                    if (_targetSurface != null)
                    {
                        if (_targetSurface.Raycast(new Ray(rayStartPoint, Vector3.up), out var hit, bounds.extents.y * 2f))
                        {
                            surfacePoints = new RaycastHit[] { hit };
                        }
                        else
                        {
                            surfacePoints = new RaycastHit[0];
                        }
                    }
                    else
                    {
                        surfacePoints = Physics.RaycastAll(rayStartPoint, Vector3.up, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);

                    }
                    surfacePoints = surfacePoints.DistinctBy(h => h.collider).ToArray();
                    foreach (var surface in surfacePoints)// for each surface evaluate if sample. Allows layered surface above each other
                    {
                        if (Random.Range(0f, 1f) >= pointPerCellRatio)// Check if sample current cell by chance
                            continue;
                        var randomOffset = new Vector3(
                            Random.Range(0, CellSize),
                            0,
                            Random.Range(0, CellSize));
                        var randomCellRayOrigin = rayStartPoint + randomOffset;
                        var ray = new Ray(randomCellRayOrigin, Vector3.up);
                        if (surface.collider.Raycast(ray, out var info, bounds.extents.y * 2))// perform raycast against the target surface
                        {
                            if (!_space.IsPointInsideSpace(info.point))// Skip if in bounds but outside sample volume
                                continue;
                            var point = new PCGPoint()
                            {
                                Position = info.point,
                                Normal = info.normal,
                                Extends = _pointExtends
                            };
                            points.Add(point);
                        }
                    }
                }
            };
            ForEachCell(cellAction);
            return points;
        }
        private void ForEachCell(Action<Vector2> action)
        {
            var bounds = _space.ApproximateBounds;
            var cellSize = CellSize;
            var sizeX = bounds.max.x - bounds.min.x;
            var sizeZ = bounds.max.z - bounds.min.z;
            int cellCountX = (int)(Mathf.Max(1, sizeX / cellSize));
            int cellCountZ = (int)(Mathf.Max(1, sizeZ / cellSize));
            for (int x = 0; x < cellCountX; ++x)
            {
                for (int z = 0; z < cellCountZ; ++z)
                {
                    var cellStart = new Vector2(bounds.min.x + x * cellSize, bounds.min.z + z * cellSize);
                    action(cellStart);
                }
            }
        }
        private void ForEachCellAsync(Action<Vector2> action)
        {
            var bounds = _space.ApproximateBounds;
            var cellSize = CellSize;
            var sizeX = bounds.max.x - bounds.min.x;
            var sizeZ = bounds.max.z - bounds.min.z;
            int cellCountX = (int)(Mathf.Max(1, sizeX / cellSize));
            int cellCountZ = (int)(Mathf.Max(1, sizeZ / cellSize));
            Parallel.For(0, cellCountX, x =>
            {
                for (int z = 0; z < cellCountZ; ++z)
                {
                    var cellStart = new Vector2(bounds.min.x + x * cellSize, bounds.min.z + z * cellSize);
                    action(cellStart);
                }
            });
        }
        private void OnDrawGizmos()
        {
            if (_drawCellGizmos)
            {
                if (GetDebugPointCount() > _maxDebugCubeCount)
                {
                    Debug.LogWarning("To many debug points. Not drawing gizmos.");
                    return;
                }
                if (_space != null && CellSize > 0)
                {
                    var bounds = _space.ApproximateBounds;
                    var cameraPos = SceneView.lastActiveSceneView.camera.transform.position.ToVector2XZ();
                    var cullingDistance = _cullDistance;
                    int cubeCount = 0;
                    ConcurrentBag<Vector2> positions = new();
                    ForEachCellAsync(pos =>
                    {
                        if (cubeCount >= _maxDebugCubeCount)
                            return;
                        if (Vector2.Distance(cameraPos, new Vector2(pos.x, pos.y)) > cullingDistance)
                            return;
                        positions.Add(pos);
                    });
                    Gizmos.color = Color.magenta;
                    foreach (var pos in positions)
                    {
                        RaycastHit[] surfacePoints;
                        if (_targetSurface != null)
                        {
                            if (_targetSurface.Raycast(new Ray(new Vector3(pos.x, bounds.max.y, pos.y), Vector3.down), out var hit, bounds.extents.y * 2f))
                            {
                                surfacePoints = new RaycastHit[] { hit };
                            }
                            else
                            {
                                surfacePoints = new RaycastHit[0];
                            }
                        }
                        else
                        {
                            surfacePoints = Physics.RaycastAll(new Vector3(pos.x, bounds.max.y, pos.y), Vector3.down, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);
                        }
                        foreach (var p in surfacePoints)
                        {
                            if (_space.IsPointInsideSpace(p.point))
                            {
                                Gizmos.DrawCube(p.point, Vector3.one * CellSize * 0.3f);
                                ++cubeCount;
                            }
                        }
                    }

                }
            }
        }
        private int GetDebugPointCount() => (int)((Mathf.Pow(_cullDistance / 2, 2) * Mathf.PI) / (_pointExtends * _pointExtends));

    }


}