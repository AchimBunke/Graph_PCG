using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Linq;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Distance
{
    public class EuclideanFeatureDistance : FeatureDistanceMeasure
    {
        public override double Aggregate(double[] attributeDistances)
        {
            return attributeDistances.Sum();
        }

        protected override double AttributeDistance(double[] obj1, double[] obj2)
        {
            if (obj1.Length != obj2.Length)
                throw new ArgumentException("Vector length must be equal");
            double sum = 0;
            for (int i = 0; i < obj1.Length; i++)
            {
                sum += Math.Pow(obj1[i] - obj2[i], 2);
            }
            return Math.Sqrt(sum);
        }

        protected override double AttributeNorm(HGraphAttributeType type)
        {
            switch (type)
            {
                case HGraphAttributeType.Enum:
                    return 1;
                case HGraphAttributeType.Nominal:
                case HGraphAttributeType.Float:
                default:
                    return Math.Sqrt(type.GetFeatureLength());
            }
        }
    }
    public class ManhattenFeatureDistance : FeatureDistanceMeasure
    {
        public override double Aggregate(double[] attributeDistances)
        {
            return attributeDistances.Sum();
        }

        protected override double AttributeDistance(double[] normalizedAttributeFeatures_1, double[] normalizedttributeFeatures_2)
        {
            if (normalizedAttributeFeatures_1.Length != normalizedttributeFeatures_2.Length)
                throw new ArgumentException("Vector length must be equal");
            double sum = 0;
            for (int i = 0; i < normalizedAttributeFeatures_1.Length; i++)
            {
                sum += Math.Abs(normalizedAttributeFeatures_1[i] - normalizedttributeFeatures_2[i]);
            }
            return sum;
        }

        protected override double AttributeNorm(HGraphAttributeType type)
        {
            switch (type)
            {
                case HGraphAttributeType.Enum:
                    return 1;
                case HGraphAttributeType.Nominal:
                case HGraphAttributeType.Float:
                default:
                    return type.GetFeatureLength();
            }
        }
    }
}
