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
    [ExecuteInEditMode]
    public class ColliderSpace : Space
    {
        [SerializeField] Collider _collider;
        public Collider Collider => _collider;
        private void Awake()
        {
            if (_collider == null)
                _collider = GetComponent<Collider>();
        }
        public override bool IsPointInsideSpace(Vector3 point)
        {
            return _collider.ClosestPoint(point) == point;
        }

        public override float Distance(Vector3 point, out Vector3 closestPoint)
        {
            return Vector3.Distance(closestPoint = _collider.ClosestPoint(point), point);
        }

        public override Bounds ApproximateBounds => _collider.bounds;
    }
}
