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

using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using UnityEditor;

namespace Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding
{
    public struct FeatureVector
    {
        Dictionary<string, (HGraphAttributeType, double[])> _data;
        public IReadOnlyDictionary<string, (HGraphAttributeType, double[])> Data => _data;

        private int categoryHash;
        
        private FeatureVector(FeatureVector other)
        {
            categoryHash = other.categoryHash;
            _data = new Dictionary<string, (HGraphAttributeType, double[])>();
            foreach (var pair in other._data)
            {
                double[] copyArray = new double[pair.Value.Item2.Length];
                Array.Copy(pair.Value.Item2, copyArray, pair.Value.Item2.Length);
                _data.Add(pair.Key, (pair.Value.Item1, copyArray));
            }
        }
        private FeatureVector(string[] categories, (HGraphAttributeType, double[])[] data)
        {
            if (categories.Length != data.Length)
                throw new ArgumentException("Arguments do not have the same length");
            _data = new Dictionary<string, (HGraphAttributeType, double[])>(categories.Zip(data, (k, v) => KeyValuePair.Create(k, v)));
            this.categoryHash = ((IStructuralEquatable)categories).GetHashCode(EqualityComparer<string>.Default);
        }

        public static FeatureVector Create()
        {
            var f = new FeatureVector();
            f._data = new();
            return f;
        }
        public static FeatureVector Create(string[] categories, (HGraphAttributeType, double[])[] data)
        {
            return new FeatureVector(categories, data);
        }


        public static FeatureVector operator *(FeatureVector a, FeatureVector b)
        {
            var f = new FeatureVector(a);
            foreach(var kv in b._data)
            {
                if (f._data.ContainsKey(kv.Key))
                {
                    var features_f = f._data[kv.Key].Item2;
                    for (int i = 0; i < features_f.Length; ++i)
                    {
                        features_f[i] *= kv.Value.Item2[i];
                    }
                    f._data[kv.Key] = (kv.Value.Item1, features_f);
                }
                else
                {
                    f._data[kv.Key] = kv.Value;
                }
            }
            return f;
        }
        public static FeatureVector operator +(FeatureVector a, FeatureVector b)
        {
            var f = new FeatureVector(a);
            foreach (var kv in b._data)
            {
                if (f._data.ContainsKey(kv.Key))
                {
                    var features_f = f._data[kv.Key].Item2;
                    for (int i = 0; i < features_f.Length; ++i)
                    {
                        features_f[i] += kv.Value.Item2[i];
                    }
                    f._data[kv.Key] = (kv.Value.Item1, features_f);
                }
                else
                {
                    f._data[kv.Key] = kv.Value;
                }
            }
            return f;
        }
        public static FeatureVector operator /(FeatureVector a, FeatureVector b)
        {
            var f = new FeatureVector(a);
            foreach (var kv in b._data)
            {
                if (f._data.ContainsKey(kv.Key))
                {
                    var features_f = f._data[kv.Key].Item2;
                    for (int i = 0; i < features_f.Length; ++i)
                    {
                        features_f[i] /= kv.Value.Item2[i];
                    }
                    f._data[kv.Key] = (kv.Value.Item1, features_f);
                }
                else
                {
                    f._data[kv.Key] = kv.Value;
                }
            }
            return f;
        }
        public static FeatureVector operator -(FeatureVector a, FeatureVector b)
        {
            var f = new FeatureVector(a);
            foreach (var kv in b._data)
            {
                if (f._data.ContainsKey(kv.Key))
                {
                    var features_f = f._data[kv.Key].Item2;
                    for (int i = 0; i < features_f.Length; ++i)
                    {
                        features_f[i] -= kv.Value.Item2[i];
                    }
                    f._data[kv.Key] = (kv.Value.Item1, features_f);
                }
                else
                {
                    f._data[kv.Key] = kv.Value;
                }
            }
            return f;
        }

        public static FeatureVector operator *(double b, FeatureVector a) => a * b;
        public static FeatureVector operator *(FeatureVector a, double b)
        {
            var f = new FeatureVector(a);
            foreach (var kv in f._data.ToArray())
            {
                double[] features = kv.Value.Item2;
                for (int i = 0; i < features.Length; ++i)
                {
                    features[i] *= b;
                }
                f._data[kv.Key] = (kv.Value.Item1, features);
            }
            return f;
        }
        public static FeatureVector operator /(FeatureVector a, double b)
        {
            var f = new FeatureVector(a);
            foreach (var kv in f._data.ToArray())
            {
                double[] features = kv.Value.Item2;
                for (int i = 0; i < features.Length; ++i)
                {
                    features[i] /= b;
                }
                f._data[kv.Key] = (kv.Value.Item1, features);
            }
            return f;
        }

        /// <summary>
        /// Multiplies a value onto a single feature encoding.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static FeatureVector operator *(FeatureVector a, (string key, double multiplier) b)
        {
            var f = new FeatureVector(a);
            var featureData = f._data[b.key];
            double[] features = featureData.Item2;
            for (int i = 0; i < features.Length; ++i)
            {
                features[i] *= b.multiplier;
            }
            f._data[b.key] = (featureData.Item1, features);
            return f;
        }
        /// <summary>
        /// Multiplies a value onto a single feature encoding.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static FeatureVector operator /(FeatureVector a, (string key, double divisor) b)
        {
            var f = new FeatureVector(a);
            var featureData = f._data[b.key];
            double[] features = featureData.Item2;
            for (int i = 0; i < features.Length; ++i)
            {
                features[i] /= b.divisor;
            }
            f._data[b.key] = (featureData.Item1, features);
            return f;
        }

        public void Normalize()
        {
            foreach (var kv in _data)
            {
                if (!HGraph.Instance.Categories.TryGetValue(kv.Key, out var hGraphCategory))
                    throw new ArgumentException("Category could not be found int HGraph");
                if (!kv.Value.Item1.IsNumericValueType(out _))
                    continue;
                var features = kv.Value.Item2;
                for(int i = 0; i < features.Length; ++i)
                {
                    features[i] = (features[i] - hGraphCategory.MinValue) / (hGraphCategory.MaxValue - hGraphCategory.MinValue);
                }
            }
        }
        public FeatureVector Normalized()
        {
            var f = new FeatureVector(this);
            f.Normalize();
            return f;
        }



        public override string ToString()
        {
            StringBuilder s=new StringBuilder();
            s.Append("[");
            s.Append(string.Join(",", _data.Select(kv => kv.Key + ": " + "[" + string.Join(",", Array.ConvertAll(kv.Value.Item2,x=>x.ToString(CultureInfo.InvariantCulture))) + "]")));
            s.Append("]");
            return s.ToString();
        }
        private class DataEqualityComparer : IEqualityComparer<(HGraphAttributeType, double[])>
        {
            public bool Equals((HGraphAttributeType, double[]) x, (HGraphAttributeType, double[]) y)
            {
                if (x.Item1 != y.Item1)
                    return false;

                if (x.Item2.Length != y.Item2.Length)
                    return false;

                for (int i = 0; i < x.Item2.Length; i++)
                {
                    if (x.Item2[i] != y.Item2[i])
                        return false;
                }

                return true;
            }

            public int GetHashCode((HGraphAttributeType, double[]) obj)
            {
                int hash = 17;
                hash = hash * 23 + obj.Item1.GetHashCode();
                foreach (var item in obj.Item2)
                {
                    hash = hash * 23 + item.GetHashCode();
                }
                return hash;
            }
        }
        //public override bool Equals(object obj)
        //{
        //    if(obj is FeatureVector v)
        //    {
        //        return v.categoryHash == categoryHash && _data.Values.SequenceEqual(v._data.Values, new DataEqualityComparer());
        //    }
        //    return false;
        //}
        //public override int GetHashCode()
        //{
        //    return HashCode.Combine(categoryHash, ((IStructuralEquatable)features).GetHashCode(EqualityComparer<double>.Default));
        //}
        public bool CategoryEquals(FeatureVector other)
        {
            return categoryHash == other.categoryHash;
        }

        //public void AlignWith(FeatureVector other)
        //{
        //    if (CategoryEquals(other))
        //        return;
        //    //TODO:
        //}
    }
}
