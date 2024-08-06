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
