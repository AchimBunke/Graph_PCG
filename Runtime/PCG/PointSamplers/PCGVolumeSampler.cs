using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Space = Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Space;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public class PCGVolumeSampler : PCGPointSampler
    {
        [SerializeField] Space _space;
        [SerializeField] bool _drawVoxelGizmos;
        [SerializeField, Tooltip("Relates to the cell size. Higher value responds to more distance between samples")] float _pointExtends = 1f;
        [SerializeField, Tooltip("Relates to the chance of sampling a cell")] float _pointsPerCubeMeter = 1;

        public float VoxelSize => _pointExtends;
        public int GetVoxelCount()
        {
            if (_space == null || _pointExtends <= 0)
                return -1;
            var bounds = _space.ApproximateBounds;
            var sizeX = bounds.extents.x * 2;
            var sizeY = bounds.extents.y * 2;
            var sizeZ = bounds.extents.z * 2;
            var numVoxelsX = (int)(1 + (sizeX / VoxelSize));// min. 1 cell
            var numVoxelsY = (int)(1 + (sizeY / VoxelSize));// min. 1 cell
            var numVoxelsZ = (int)(1 + (sizeZ / VoxelSize));// min. 1 cell
            return numVoxelsX * numVoxelsY * numVoxelsZ;
        }

        public override IEnumerable<PCGPoint> SamplePoints()
        {
            ConcurrentBag<PCGPoint> points = new ConcurrentBag<PCGPoint>();

            if (_pointsPerCubeMeter <= 0)
            {
                Debug.LogWarning("Invalid PointsPreSquaredMeter: No Points to sample.");
                return points;
            }
            if (VoxelSize <= 0)
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
            var targetPointCount = (bounds.extents.x * 2) * (bounds.extents.y * 2) * (bounds.extents.z * 2) * _pointsPerCubeMeter;
            var pointPerVoxelRatio = Mathf.Clamp(targetPointCount / (float)GetVoxelCount(), 0, 1);
            if (pointPerVoxelRatio <= 0)
                return points;
            Action<Vector3> voxelAction = (Vector3 voxel) =>
            {
                if (Random.Range(0f, 1f) >= pointPerVoxelRatio)// Check if sample current cell by chance
                    return;
                var randomOffset = new Vector3(
                        Random.Range(0, VoxelSize),
                        Random.Range(0, VoxelSize),
                        Random.Range(0, VoxelSize));
                var randomVoxelPoint = voxel + randomOffset;
                if (!_space.IsPointInsideSpace(randomVoxelPoint))// Skip if in bounds but outside sample volume
                    return;
                var point = new PCGPoint()
                {
                    Position = randomVoxelPoint,
                    Extends = _pointExtends
                };
                points.Add(point);
            };
            ForEachVoxel(voxelAction);
            return points;
        }
        private void ForEachVoxel(Action<Vector3> action)
        {
            var bounds = _space.ApproximateBounds;
            var voxelSize = VoxelSize;
            var sizeX = bounds.max.x - bounds.min.x;
            var sizeY = bounds.max.y - bounds.min.y;
            var sizeZ = bounds.max.z - bounds.min.z;
            int voxelCountX = (int)(Mathf.Max(1, sizeX / voxelSize));
            int voxelCountY = (int)(Mathf.Max(1, sizeY / voxelSize));
            int voxelCountZ = (int)(Mathf.Max(1, sizeZ / voxelSize));
            //Parallel.For(0, GetVoxelCount(), i =>
            //{
            //      // Not possible with Collider.ClosestPoint
            //    int x = i % voxelCountX;
            //    int y = (i / voxelCountX) % voxelCountY;
            //    int z = i / (voxelCountX * voxelCountY);
            //    var voxelStart = new Vector3(bounds.min.x + x * voxelSize, bounds.min.y + y * voxelSize, bounds.min.z + z * voxelSize);
            //    action(voxelStart);
            //});
            for (int x = 0; x < voxelCountX; ++x)
            {
                for (int y = 0; y < voxelCountY; ++y)
                {
                    for (int z = 0; z < voxelCountZ; ++z)
                    {
                        var voxelStart = new Vector3(bounds.min.x + x * voxelSize, bounds.min.y + y * voxelSize, bounds.min.z + z * voxelSize);
                        action(voxelStart);
                    }
                }
            }
        }
        private void OnDrawGizmos()
        {
            if (_drawVoxelGizmos)
            {

                if (_space != null && VoxelSize > 0)
                {
                    if (GetVoxelCount() > 10000)
                    {
                        Debug.LogWarning("Does not draw grid as it has too many voxels!");
                    }
                    else
                    {
                        var bounds = _space.ApproximateBounds;
                        var centerOffset = VoxelSize * Vector3.one * 0.5f;
                        ForEachVoxel(pos =>
                        {
                            var voxelCenter = pos + centerOffset;
                            if (!_space.IsPointInsideSpace(voxelCenter))// Skip if in bounds but outside sample volume
                                return;
                            Gizmos.DrawCube(voxelCenter, Vector3.one * VoxelSize * 0.3f);
                        });
                    }
                }
            }
        }
    }
}