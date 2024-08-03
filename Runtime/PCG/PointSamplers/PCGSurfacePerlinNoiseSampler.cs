using ALib.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;
using Space = Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Space;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    /// <summary>
    /// Cell based approach from https://github.com/EpicGames/UnrealEngine/blob/release/Engine/Plugins/PCG/Source/PCG/Private/Elements/PCGSurfaceSampler.cpp
    /// </summary>
    public class PCGSurfacePerlinNoiseSampler : PCGPointSampler
    {
        [SerializeField] LayerMask _surfaceMask = Physics.AllLayers;
        [SerializeField] Vector2 _perlinOffset = Vector2.zero;
        [SerializeField] float _perlinScale = 10;
        [SerializeField] float _perlinThreshold = 0.5f;
        [SerializeField] bool _randomOffsetInsideCell = false;

        [SerializeField] Space _space;
        [SerializeField] PCGSampleMode _sampleMode;
        [SerializeField, Tooltip("Relates to the cell size. Higher value responds to more distance between samples")] float _pointExtends = 1f;
        [SerializeField] bool _drawCellGizmos;
        //public Collider SampleVolume => _sampleVolume;
        public LayerMask SurfaceMask => _surfaceMask;
        public float CellSize => _pointExtends;
        public int EstimatePointCount() => (int)(_perlinThreshold * GetCellCount());
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

            if (_perlinThreshold <= 0)
            {
                Debug.LogWarning("Invalid PerlinThreshold: No Points to sample.");
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
            var targetPointCount = (bounds.extents.x * 2) * (bounds.extents.z * 2) * _perlinThreshold;
            //var pointPerCellRatio = Mathf.Clamp(targetPointCount / (float)GetCellCount(), 0, 1);
            //if (pointPerCellRatio <= 0)
            //    return points;
            Action<Vector2> cellAction = (Vector2 cell) =>
            {
                if (GetNoiseValue(cell.x, cell.y) < _perlinThreshold)// Check if sample current cell by chance
                    return;
                // check all avaiulable surfaces
                if (_sampleMode == PCGSampleMode.Down || _sampleMode == PCGSampleMode.UpDown)
                {
                    Vector3 rayStartPoint = new Vector3(cell.x, bounds.max.y, cell.y);
                    var surfacePoints = Physics.RaycastAll(rayStartPoint, Vector3.down, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);
                    surfacePoints = surfacePoints.DistinctBy(h => h.collider).ToArray();
                    foreach (var surface in surfacePoints)// for each surface evaluate if sample. Allows layered surface above each other
                    {
                        Vector3 randomOffset = Vector3.zero;
                        if (_randomOffsetInsideCell)
                        {
                            randomOffset = new Vector3(
                                Random.Range(0, CellSize),
                                0,
                                Random.Range(0, CellSize));
                        }
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
                    var surfacePoints = Physics.RaycastAll(rayStartPoint, Vector3.up, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);
                    surfacePoints = surfacePoints.DistinctBy(h => h.collider).ToArray();
                    foreach (var surface in surfacePoints)// for each surface evaluate if sample. Allows layered surface above each other
                    {
                        Vector3 randomOffset = Vector3.zero;
                        if (_randomOffsetInsideCell)
                        {
                            randomOffset = new Vector3(
                                Random.Range(0, CellSize),
                                0,
                                Random.Range(0, CellSize));
                        }
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
        private float GetNoiseValue(float x, float y)
        {
            return Mathf.PerlinNoise((x * _perlinScale) + _perlinOffset.x, (y * _perlinScale) + _perlinOffset.y);
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
        private void OnDrawGizmos()
        {
            if (_drawCellGizmos)
            {

                if (_space != null && CellSize > 0)
                {
                    if (EstimatePointCount() > 10000)
                    {
                        Debug.LogWarning("Does not draw grid as it has too many cells!");
                    }
                    else
                    {
                        Gizmos.color = Color.magenta;
                        var bounds = _space.ApproximateBounds;
                        ForEachCell(pos =>
                        {
                            var noise = GetNoiseValue(pos.x, pos.y);
                            if (noise < _perlinThreshold)// Check if sample current cell by chance
                                return;
                            var surfacePoints = Physics.RaycastAll(new Vector3(pos.x, bounds.max.y, pos.y), Vector3.down, bounds.extents.y * 2f, SurfaceMask, QueryTriggerInteraction.Ignore);
                            foreach (var p in surfacePoints)
                            {
                                Gizmos.DrawCube(p.point, Vector3.one * CellSize * 0.3f);
                            }
                        });
                    }
                }
            }
        }

    }

}