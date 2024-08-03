using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Distance
{

    public abstract class FeatureDistanceMeasure
    {
        public double AttributeDistance(HGraphAttribute a1, HGraphAttribute a2)
        {
            if (a1.Category.Value != a2.Category.Value)
                throw new ArgumentException("Attributes must be of same category");
            if(HGraph.Instance.Categories.TryGetValue(a1.Category.Value, out var category))
            {
                if (category.Type.Value.IsNumericValueType(out _))
                {
                    return AttributeDistance(a1.Data, a2.Data, category.MinValue, category.MaxValue);
                }
                else
                {
                    return AttributeDistance(a1.Data, a2.Data, 0, 1);
                }
            }
            else
                throw new ArgumentException("Attribute category does not exists. Cannot normalize feature vector");
        }

        /// <summary>
        /// </summary>
        /// <param name="c1"></param>
        /// <param name="c2"></param>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public double AttributeDistance(HGraphAttributeContent c1, HGraphAttributeContent c2, float minValue, float maxValue)
        {
            if (c1 is RangeHGraphAttribute || c2 is RangeHGraphAttribute)
                return RangeAttributeDistance(FeatureVectorUtil.NormalizeFeatures(c1.EncodeFeatureVector(), minValue, maxValue),
                    FeatureVectorUtil.NormalizeFeatures(c2.EncodeFeatureVector(), minValue, maxValue));
            else
                return AttributeDistance(
                    FeatureVectorUtil.NormalizeFeatures(c1.EncodeFeatureVector(), minValue, maxValue),
                    FeatureVectorUtil.NormalizeFeatures(c2.EncodeFeatureVector(), minValue, maxValue)
                );
        }

        public double Distance(HGraphNode n1, HGraphNode n2)
            => FeatureDistance(FeatureVectorUtil.CreateFeatureVector(n1, normalized: true), FeatureVectorUtil.CreateFeatureVector(n2, normalized: true));
       
        /// <summary>
        /// Calculates the Distance between 2 normalized Feature vectors. Normalized = each attribute feature in the vector is normalized to 0-1
        /// </summary>
        /// <param name="normalizedFeatureVector_1"></param>
        /// <param name="normalizedFeatureVector_2"></param>
        /// <returns></returns>
        public virtual double FeatureDistance(FeatureVector normalizedFeatureVector_1, FeatureVector normalizedFeatureVector_2, bool normalized = false)
        {
            IReadOnlyDictionary<string, (HGraphAttributeType, double[])> f1 = normalizedFeatureVector_1.Data;
            IReadOnlyDictionary<string, (HGraphAttributeType, double[])> f2 = normalizedFeatureVector_2.Data;
            List<double> attributeDistances = new();
            int intersections = 0;
            foreach (var c in f1.Keys.Intersect(f2.Keys))
            {
                double attributeDistance;
                if (f1[c].Item1 == HGraphAttributeType.Range || f2[c].Item1 == HGraphAttributeType.Range) 
                    attributeDistance = RangeAttributeDistance(f1[c].Item2, f2[c].Item2);
                else
                    attributeDistance = AttributeDistance(f1[c].Item2, f2[c].Item2);
                attributeDistance /= AttributeNorm(f1[c].Item1);// TODO Fix for ranges-values comparisons
                attributeDistances.Add(attributeDistance);
                ++intersections;
            }
            for (int i = 0; i < f1.Count() + f2.Count() - (2 * intersections); i++)
            {
                attributeDistances.Add(AttributeMismatchDistance);
            }
            var aggregated = Aggregate(attributeDistances.ToArray());
            if (normalized)
                return aggregated / attributeDistances.Count;
            return aggregated;
        }

        /// <summary>
        /// Calculates the distance between 2 normalized attribute features. Normalized = each value in the feature is normalized to 0-1.
        /// </summary>
        /// <param name="normalizedAttributeFeatures_1"></param>
        /// <param name="normalizedAttributeFeatures_2"></param>
        /// <returns></returns>
        public double FeatureEncodingDistance(double[] normalizedAttributeFeatures_1, double[] normalizedAttributeFeatures_2, HGraphAttributeType type)
        {
            if (type == HGraphAttributeType.Range)
                return RangeAttributeDistance(normalizedAttributeFeatures_1, normalizedAttributeFeatures_2);
            return AttributeDistance(normalizedAttributeFeatures_1, normalizedAttributeFeatures_2);
        }
        /// <summary>
        /// Does not handle Ranges
        /// </summary>
        /// <param name="normalizedAttributeFeatures_1"></param>
        /// <param name="normalizedttributeFeatures_2"></param>
        /// <returns></returns>
        protected abstract double AttributeDistance(double[] normalizedAttributeFeatures_1, double[] normalizedttributeFeatures_2);
        protected virtual double RangeAttributeDistance(double[] normalizedAttributeFeatures_1, double[] normalizedAttributeFeatures_2)
        {
            if(normalizedAttributeFeatures_1.Length == 1 && normalizedAttributeFeatures_2.Length == 2)//Single Value <=> Range
            {
                // Inside Range
                if (normalizedAttributeFeatures_1[0] >= normalizedAttributeFeatures_2[0] && normalizedAttributeFeatures_1[0] <= normalizedAttributeFeatures_2[1])
                {
                    // Returns 0 or nearest distance (from center)
                    return MinDistanceIfInsideRange ? 0 :
                        AttributeDistance(new[] { normalizedAttributeFeatures_1[0] }, new[] { (normalizedAttributeFeatures_2[1] + normalizedAttributeFeatures_2[0])/2d });
                }
                else
                {
                    // Returns 1 or nearest distance (Min or Max from range)
                    return MaxDistanceIfOutOfRange ? 1 :
                        Math.Min(AttributeDistance(new[] { normalizedAttributeFeatures_1[0] }, new[] { normalizedAttributeFeatures_2[0] }),
                            AttributeDistance(new[] { normalizedAttributeFeatures_1[0] }, new[] { normalizedAttributeFeatures_2[1] }));
                }
            }
            else if (normalizedAttributeFeatures_1.Length == 2 && normalizedAttributeFeatures_2.Length == 1)// Same as above but switched
            {
                // Inside Range
                if (normalizedAttributeFeatures_2[0] >= normalizedAttributeFeatures_1[0] && normalizedAttributeFeatures_2[0] <= normalizedAttributeFeatures_1[1])
                {
                    // Returns 0 or nearest distance (Min or Max from range)
                    return MinDistanceIfInsideRange ? 0 :
                        AttributeDistance(new[] { normalizedAttributeFeatures_2[0] }, new[] { (normalizedAttributeFeatures_1[1] + normalizedAttributeFeatures_1[0])/2d });
                }
                else
                {
                    // Returns 1 or nearest distance (Min or Max from range)
                    return MaxDistanceIfOutOfRange ? 1 :
                        Math.Min(AttributeDistance(new[] { normalizedAttributeFeatures_2[0] }, new[] { normalizedAttributeFeatures_1[0] }),
                            AttributeDistance(new[] { normalizedAttributeFeatures_2[0] }, new[] { normalizedAttributeFeatures_1[1] }));
                }
            }
            else if (normalizedAttributeFeatures_1.Length == 2 && normalizedAttributeFeatures_2.Length == 2)
            {
                switch(CheckRanges(normalizedAttributeFeatures_1, normalizedAttributeFeatures_2))
                {
                    case RangeRelation.Disjoint:
                        {
                            return MaxDistanceIfOutOfRange ? 1 :
                              AttributeDistance(normalizedAttributeFeatures_2, normalizedAttributeFeatures_1);
                        }
                    case RangeRelation.FullyIncludes:
                        {
                            return MinDistanceIfInsideRange ? 0 : 
                                AttributeDistance(normalizedAttributeFeatures_1, normalizedAttributeFeatures_2);
                        }
                    case RangeRelation.Intersect:
                        {
                            double overlapMin = Math.Max(normalizedAttributeFeatures_1[0], normalizedAttributeFeatures_2[0]);
                            double overlapMax = Math.Min(normalizedAttributeFeatures_1[1], normalizedAttributeFeatures_2[1]);
                            double overlapDistance = overlapMax - overlapMin;
                            double range_1_distance = normalizedAttributeFeatures_1[1] - normalizedAttributeFeatures_1[0];
                            double range_2_distance = normalizedAttributeFeatures_2[1] - normalizedAttributeFeatures_2[0];
                            double nonOverlapping = (range_1_distance - overlapDistance) + (range_2_distance - overlapDistance);
                            return AttributeDistance(new[] { overlapDistance }, new[]{ nonOverlapping });
                        }
                    default:
                        throw new InvalidOperationException();
                }
            }
            else
            {
                throw new InvalidOperationException("Cannot handle this type of ranges");
            }
        }

        /// <summary>
        /// Aggregates attribute distances to a total distance.
        /// </summary>
        /// <param name="attributeDistances"></param>
        /// <returns></returns>
        public abstract double Aggregate(double[] attributeDistances);
        /// <summary>
        /// Normalizes the attribute distances before aggregating. Ensures equal influnce of attribute distance on the total distance.
        /// Example: OneHot encoding: [0,1,0] - [1,0,0] = [1,1,0]; Sum([1,1,0]) = 2; 2 > [0,1]!;
        /// </summary>
        /// <param name="attributeDistances"></param>
        /// <returns></returns>
        protected abstract double AttributeNorm(HGraphAttributeType type);

        protected enum RangeRelation
        {
            Disjoint,       // Ranges do not overlap
            Intersect,      // Ranges overlap but neither fully includes the other
            FullyIncludes,  // One range fully includes the other
        }

        private RangeRelation CheckRanges(double[] range1, double[] range2)
        {
            if (range1.Length != 2 || range2.Length != 2)
            {
                throw new ArgumentException("Both ranges must contain exactly 2 values: min and max.");
            }

            double min1 = Math.Min(range1[0], range1[1]);
            double max1 = Math.Max(range1[0], range1[1]);
            double min2 = Math.Min(range2[0], range2[1]);
            double max2 = Math.Max(range2[0], range2[1]);

            if (max1 < min2 || max2 < min1)
            {
                return RangeRelation.Disjoint; // No overlap
            }

            if ((min1 <= min2 && max1 >= max2) || (min2 <= min1 && max2 >= max1))
            {
                return RangeRelation.FullyIncludes; // One range fully includes the other
            }

            return RangeRelation.Intersect; // Overlap exists but neither fully includes the other
        }

        /// <summary>
        /// Value substituted as Distance if a feature vector does not contain an attribute feature.
        /// </summary>
        public virtual double AttributeMismatchDistance { get; set; } = 1f;
        /// <summary>
        /// Indicates that the distance of ranges and other values is max if the other value is outside of the range
        /// </summary>
        public bool MaxDistanceIfOutOfRange { get; set; } = true;
        /// <summary>
        /// Indicates that the distance of ranges and other values is max if the other value is outside of the range
        /// </summary>
        public bool MinDistanceIfInsideRange { get; set; } = true;
    }
}