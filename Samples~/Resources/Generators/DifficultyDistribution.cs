using System;
using System.Linq;
using UnityEngine;
using GK;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    public class DifficultyDistribution
    {
        public DifficultyPositionPair[] DifficultyPositions { get; set; }
        public InterpolationType Type { get; set; }

        [Serializable]
        public struct DifficultyPositionPair
        {
            public Vector2 Position;
            public float Difficulty;
        }

        public enum InterpolationType
        {
            Voronoi,
            Delaunay
        }



        public CustomDelaunayTriangulationWrapper DelaunayResult { get; private set; }
        public VoronoiDiagram VoronoiResult { get; private set; }

        public bool HasResult => Type == InterpolationType.Delaunay ? DelaunayResult != null : VoronoiResult != null;
        public void Triangulate()
        {
            if (Type == InterpolationType.Delaunay)
            {
                if (DifficultyPositions == null || DifficultyPositions.Length < 3)
                    return;
                var delaunay = new DelaunayCalculator();
                var delaunayResult = delaunay.CalculateTriangulation(DifficultyPositions.Select(dp => dp.Position).ToList());
                DelaunayResult = new CustomDelaunayTriangulationWrapper(delaunayResult);
            }
            else if (Type == InterpolationType.Voronoi)
            {
                var voronoi = new VoronoiCalculator();
                VoronoiResult = voronoi.CalculateDiagram(DifficultyPositions.Select(dp => dp.Position).ToList());
            }
        }

        public float FindDifficultyForPosition(Vector2 position)
        {
            if (Type == InterpolationType.Delaunay)
            {
                DelaunayResult.FindTriangle(position, out var v0, out var v1, out var v2);
                FindDifficultyPositionsForTriangle(v0, v1, v2, out int dp0, out int dp1, out int dp2);
                if (dp0 == -1 || dp1 == -1 || dp2 == -1)
                    return 0;
                else
                    return InterpolateValue(position, DifficultyPositions[dp0], DifficultyPositions[dp1], DifficultyPositions[dp2]);
            }
            else if (Type == InterpolationType.Voronoi)
            {
                int closestSiteIdx = -1;
                float minDist = float.MaxValue;
                for (int i = 0; i < VoronoiResult.Sites.Count; ++i)
                {
                    var dist = Vector2.Distance(position, VoronoiResult.Sites[i]);
                    if (closestSiteIdx == -1 || dist < minDist)
                    {
                        closestSiteIdx = i;
                        minDist = dist;
                    }
                }
                if (closestSiteIdx == -1)
                    return 0;
                FindDifficultyPositionsForSite(closestSiteIdx, out int dp);
                return DifficultyPositions[dp].Difficulty;
            }
            return 0;
        }
        public float[] FindDifficultiesForPositions(Vector2[] positions)
        {
            return positions
                //.AsParallel()
                .Select(p => FindDifficultyForPosition(p))
                .ToArray();
        }
        private void FindDifficultyPositionsForSite(int siteIdx, out int dp)
        {
            dp = siteIdx;
        }
        private void FindDifficultyPositionsForTriangle(Vector2 v0, Vector2 v1, Vector2 v2,
            out int dp0, out int dp1, out int dp2)
        {
            dp0 = dp1 = dp2 = -1;
            for (int i = 0; i < DifficultyPositions.Length; ++i)
            {
                var dp = DifficultyPositions[i];
                if (dp0 == -1 && dp.Position == v0)
                    dp0 = i;
                if (dp1 == -1 && dp.Position == v1)
                    dp1 = i;
                if (dp2 == -1 && dp.Position == v2)
                    dp2 = i;
            }
        }

        float InterpolateValue(Vector2 P, DifficultyPositionPair A, DifficultyPositionPair B, DifficultyPositionPair C)
        {
            // Calculate barycentric coordinates
            float u = InterpolateBarycentric(P, A.Position, B.Position, C.Position);
            float v = InterpolateBarycentric(P, B.Position, C.Position, A.Position);
            float w = 1 - u - v;

            // Interpolate value
            float interpolatedValue = u * A.Difficulty + v * B.Difficulty + w * C.Difficulty;

            return interpolatedValue;
        }

        private float InterpolateBarycentric(Vector2 P, Vector2 A, Vector2 B, Vector2 C)
        {
            float denominator = ((B.y - C.y) * (A.x - C.x) + (C.x - B.x) * (A.y - C.y));
            float u = ((B.y - C.y) * (P.x - C.x) + (C.x - B.x) * (P.y - C.y)) / denominator;
            return u;
        }
    }

}