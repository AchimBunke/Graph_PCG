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