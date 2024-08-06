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

namespace Achioto.Gamespace_PCG.Runtime.Utils
{
    public static class GeometryUtility
    {
        /// <summary>
        /// https://discussions.unity.com/t/shortest-distance-between-two-meshes-colliders/247640/4
        /// !! Approximation not totally accurate
        /// </summary>
        /// <param name="b1"></param>
        /// <param name="b2"></param>
        /// <returns></returns>
        public static float Distance(Bounds b1, Bounds b2, out Vector3 closestPoint_b1, out Vector3 closestPoint_b2)
        {
            float sd0 = sdBounds(b1.center, b2, out closestPoint_b1);
            float sd1 = sdBounds(b2.center, b1, out closestPoint_b2);
            return Vector3.Distance(closestPoint_b1, closestPoint_b2);
        }
        static float sdBounds(Vector3 point, Bounds bounds, out Vector3 contact)
        {
            Vector3 dir = point - bounds.center;
            float sd = sdBox(dir, bounds.extents);

            contact = point - dir.normalized * sd;
            // note: we dont need to know the real contact point in this case, this is pure conjecture

            return sd;
        }

        // src: https://www.iquilezles.org/www/articles/distfunctions/distfunctions.htm
        static float sdBox(Vector3 p, Vector3 b)
        {
            Vector3 q = new Vector3(Mathf.Abs(p.x), Mathf.Abs(p.y), Mathf.Abs(p.z)) - b;
            return Vector3.Magnitude(Vector3.Max(q, Vector3.zero)) + Mathf.Min(Mathf.Max(q.x, Mathf.Max(q.y, q.z)), 0f);
        }
    }
}

