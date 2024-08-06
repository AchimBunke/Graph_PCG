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

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ALib.Extensions;
using Random = UnityEngine.Random;
using Space = Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Space;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public class PCGFullRandomSurfaceSampler : PCGPointSampler
    {
        [SerializeField] LayerMask _surfaceMask = Physics.AllLayers;
        [SerializeField] Space _space;
        [SerializeField] protected int _pointCount = 0;


        public override IEnumerable<PCGPoint> SamplePoints()
        {
            List<PCGPoint> points = new List<PCGPoint>();
            if (_pointCount == 0)
            {
                Debug.LogWarning("No Points to sample.");
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
            int sampleTries = 0;// Stop at some point if sampling does not work
            while (points.Count < _pointCount && sampleTries < _pointCount * 3)// stop if enough points or more than 3*pointsCount positions tested (then bounds are setup badly)
            {
                if (TrySampleRandomSurfacePosition(bounds, out var hit))
                {
                    var point = new PCGPoint()
                    {
                        Position = hit.point,
                        Normal = hit.normal,
                        Extends = 0,
                    };
                    points.Add(point);
                }
                ++sampleTries;
            }
            return points;
        }
        private bool TrySampleRandomSurfacePosition(Bounds bounds, out RaycastHit hit)
        {
            var randomOffset = new Vector3(
              Random.Range(-bounds.extents.x, bounds.extents.x),
              bounds.extents.y,
              Random.Range(-bounds.extents.z, bounds.extents.z));
            Vector3 randomBoundsStartPoint = bounds.center + randomOffset;
            var ray = new Ray(randomBoundsStartPoint, Vector3.down);
            var hits = Physics.RaycastAll(ray, bounds.extents.y * 2f, _surfaceMask, QueryTriggerInteraction.Ignore);
            hits = hits.DistinctBy(h => h.collider).ToArray();
            Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            if (hits.Length > 0)
            {
                hit = hits[Random.Range(0, hits.Length)];
                return true;
            }
            hit = default;
            return false;
        }



    }
}