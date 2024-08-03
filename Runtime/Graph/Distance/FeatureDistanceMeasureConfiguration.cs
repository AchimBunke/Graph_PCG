using System;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Distance
{
    [Serializable]
    public struct FeatureDistanceMeasureConfiguration
    {
        public FeatureDistanceMeasures Measure;
        public float AttributeMismatchDistance;
        public bool MaxDistanceIfOutOfRange;
        public bool MinDistanceIfInsideRange;

        public FeatureDistanceMeasure Create()
        {
            FeatureDistanceMeasure measure;
            switch (Measure)
            {
                case FeatureDistanceMeasures.Euclidean:
                    measure = new EuclideanFeatureDistance();
                    break;
                case FeatureDistanceMeasures.Manhatten:
                    measure = new ManhattenFeatureDistance();
                    break;
                default: return default;
            }
            measure.AttributeMismatchDistance = AttributeMismatchDistance;
            measure.MaxDistanceIfOutOfRange = MaxDistanceIfOutOfRange;
            measure.MinDistanceIfInsideRange = MinDistanceIfInsideRange;
            return measure;
        }
        public static FeatureDistanceMeasureConfiguration Default => new FeatureDistanceMeasureConfiguration()
        {
            AttributeMismatchDistance = 0,
            Measure = FeatureDistanceMeasures.Euclidean,
            MaxDistanceIfOutOfRange = true,
            MinDistanceIfInsideRange = true,
        };
    }
    public enum FeatureDistanceMeasures
    {
        Euclidean,
        Manhatten
    }

}