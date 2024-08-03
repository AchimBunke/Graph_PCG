using System;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Distance
{
    [Serializable]
    public struct SpatialDistanceMeasureConfiguration
    {
        public SpatialDistanceMeasures Measure;
        public static SpatialDistanceMeasureConfiguration Default => new SpatialDistanceMeasureConfiguration()
        {
            Measure = SpatialDistanceMeasures.Euclidean,
        };

        public SpatialDistanceMeasure Create()
        {
            switch (Measure)
            {
                case SpatialDistanceMeasures.Euclidean:
                    return new EuclideanSpatialDistance();
                default: return default;
            }
        }
    }
    public enum SpatialDistanceMeasures
    {
        Euclidean
    }
}