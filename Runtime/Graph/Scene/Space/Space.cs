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

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    public abstract class Space : MonoBehaviour
    {
        public abstract Bounds ApproximateBounds { get; }
        public abstract bool IsPointInsideSpace(Vector3 point);
        public float Distance(Vector3 point) => Distance(point, out _);

        public abstract float Distance(Vector3 point, out Vector3 closestPoint);

        /// <summary>
        /// https://discussions.unity.com/t/shortest-distance-between-two-meshes-colliders/247640/4
        /// </summary>
        /// <param name="space"></param>
        /// <returns></returns>
        public virtual float Distance(Space space, out Vector3 closestPoint, out Vector3 closestPoint_space)
        {
            return Gamespace_PCG.Runtime.Utils.GeometryUtility.Distance(ApproximateBounds, space.ApproximateBounds, out closestPoint, out closestPoint_space);
        }
    }
}