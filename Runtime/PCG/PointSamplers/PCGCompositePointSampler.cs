using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers
{
    public class PCGCompositePointSampler : PCGPointSampler
    {
        [SerializeField] bool _cleanIncludePoints;
        [SerializeField] float _cleanPointsExtendScaling = 1;
        [SerializeField] float _excludePointsExtendScaling = 1;
        [SerializeField] PCGPointSampler[] _includePointSamplers;
        [SerializeField] PCGPointSampler[] _excludePointSamplers;
        public override IEnumerable<PCGPoint> SamplePoints()
        {
            List<PCGPoint> includePoints = new List<PCGPoint>();
            List<PCGPoint> excludePoints = new List<PCGPoint>();
            if (_includePointSamplers == null)
                return includePoints;
            foreach (var includeSampler in _includePointSamplers)
            {
                includePoints.AddRange(includeSampler.SamplePoints());
            }
            var includePointsCount = excludePoints.Count;
            if (_excludePointSamplers != null)
            {
                foreach (var excludeSampler in _excludePointSamplers)
                {
                    excludePoints.AddRange(excludeSampler.SamplePoints());
                }
            }
            int cleanedCount = 0;
            IEnumerable<PCGPoint> cleanedPoints = includePoints;
            if (_cleanIncludePoints)
            {
                cleanedCount = CleanOverlappingPoints(includePoints, out cleanedPoints);
            }
            var removedCount = RemovePoints(cleanedPoints, excludePoints, out var cleanRemoved);
            //Debug.Log($"Cleaned {cleanedCount}/{includePointsCount} overlapping points - Excluded {removedCount}/{cleanedPoints.Count()} overlapping points!");
            return cleanRemoved;

        }

        private int CleanOverlappingPoints(IEnumerable<PCGPoint> points, out IEnumerable<PCGPoint> result)
        {
            ConcurrentBag<PCGPoint> cleanedPoints = new ConcurrentBag<PCGPoint>();
            int removed = 0;
            Parallel.ForEach(points, currentPoint =>
            {
                foreach (var cleanPoint in cleanedPoints)
                {
                    var distance = Vector3.Distance(currentPoint.Position, cleanPoint.Position);
                    if (distance < Mathf.Max(currentPoint.Extends, cleanPoint.Extends) * _cleanPointsExtendScaling)
                    {
                        ++removed;
                        return;
                    }
                }
                cleanedPoints.Add(currentPoint);
            });
            result = cleanedPoints;
            return removed;
        }
        private int RemovePoints(IEnumerable<PCGPoint> points, IEnumerable<PCGPoint> toRemove, out IEnumerable<PCGPoint> result)
        {
            ConcurrentBag<PCGPoint> cleanedPoints = new ConcurrentBag<PCGPoint>();
            int removed = 0;
            Parallel.ForEach(points, currentPoint =>
            {
                foreach (var cleanPoint in toRemove)
                {
                    var distance = Vector3.Distance(currentPoint.Position, cleanPoint.Position);
                    if (distance < cleanPoint.Extends * _excludePointsExtendScaling)
                    {
                        ++removed;
                        return;
                    }
                }
                cleanedPoints.Add(currentPoint);
            });
            result = cleanedPoints;
            return removed;
        }
    }
}