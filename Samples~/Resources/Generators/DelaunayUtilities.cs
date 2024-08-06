using UnityEngine;
using GK;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    public class CustomDelaunayTriangulationWrapper
    {
        public DelaunayTriangulation DelaunayTriangulation { get; set; }
        public CustomDelaunayTriangulationWrapper(DelaunayTriangulation delaunayTriangulation)
        {
            DelaunayTriangulation = delaunayTriangulation;
        }
        public int FindTriangle(Vector2 point)
        {
            return FindTriangle(point, out Vector2 _, out Vector2 _, out Vector2 _);
        }
        public int FindTriangle(Vector2 point, out Vector2 v0, out Vector2 v1, out Vector2 v2)
        {
            for (int i = 0; i < DelaunayTriangulation.Triangles.Count; i += 3)
            {
                v0 = DelaunayTriangulation.Vertices[DelaunayTriangulation.Triangles[i]];
                v1 = DelaunayTriangulation.Vertices[DelaunayTriangulation.Triangles[i + 1]];
                v2 = DelaunayTriangulation.Vertices[DelaunayTriangulation.Triangles[i + 2]];
                if (Geom.PointInTriangle(point, v0, v1, v2))
                    return i;
            }
            v0 = v1 = v2 = default;
            return -1;
        }

    }
}