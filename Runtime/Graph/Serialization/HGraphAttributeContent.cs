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
using System;
using System.Xml.Serialization;
using Unity.Plastic.Newtonsoft.Json;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{


    public delegate void HGraphAttributeChangedEventHandler();


    [Serializable]
    [XmlInclude(typeof(NominalHGraphAttribute))]
    [XmlInclude(typeof(FloatHGraphAttribute))]
    [XmlInclude(typeof(EnumHGraphAttribute))]
    [XmlInclude(typeof(FlagsEnumHGraphAttribute))]
    [XmlInclude(typeof(RangeHGraphAttribute))]
    [XmlInclude(typeof(Vector3HGraphAttribute))]
    [XmlInclude(typeof(Vector2HGraphAttribute))]
    [XmlInclude(typeof(BooleanHGraphAttribute))]
    [XmlType("content")]
    public class HGraphAttributeContent : ISerializationCallbackReceiver
    {
        public HGraphAttributeContent() { }

        public event HGraphAttributeChangedEventHandler DataChanged;

        public virtual HGraphAttributeContent Copy()
        {
            throw new NotImplementedException();
        }

        public void OnAfterDeserialize()
        {
            DataChanged?.Invoke();
        }

        public void OnBeforeSerialize()
        {
        }
        [JsonIgnore]
        public int FeatureVectorLength => EncodeFeatureVector().Length;
        public virtual double[] EncodeFeatureVector()
        {
            throw new NotImplementedException();
        }
        public virtual HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            throw new NotImplementedException();
        }

        public HGraphAttributeType GetAttributeType()
        {
            switch (this)
            {
                case NominalHGraphAttribute: return HGraphAttributeType.Nominal;
                case FloatHGraphAttribute: return HGraphAttributeType.Float;
                case FlagsEnumHGraphAttribute: return HGraphAttributeType.FlagsEnum;
                case EnumHGraphAttribute: return HGraphAttributeType.Enum;
                case RangeHGraphAttribute: return HGraphAttributeType.Range;
                case Vector2HGraphAttribute: return HGraphAttributeType.Vector2;
                case Vector3HGraphAttribute: return HGraphAttributeType.Vector3;
                case BooleanHGraphAttribute: return HGraphAttributeType.Boolean;
                default:
                    throw new InvalidOperationException();
            }
        }
        public virtual bool TrySetValue(object value) { throw new NotImplementedException(); }
        public virtual object GetValue() { throw new NotImplementedException(); }
        public bool TryGetValue<T>(out T value)
        {
            try
            {
                value = (T)Convert.ChangeType(GetValue(), typeof(T));
                return true;
            }
            catch (Exception)
            {
                value = default(T);
                return false;
            }
        }
    }

    #region Content Types
    [Serializable]
    public class NominalHGraphAttribute : HGraphAttributeContent
    {
        public int Value;
        public override HGraphAttributeContent Copy()
        {
            return new NominalHGraphAttribute() { Value = this.Value };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[] { Value };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new NominalHGraphAttribute() { Value = (int)Mathf.RoundToInt((float)featureVector[0]) };
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                Value = (int)Convert.ChangeType(value, typeof(int));
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Value;
        }

    }

    [Serializable]
    public class BooleanHGraphAttribute : HGraphAttributeContent
    {
        public bool Value;
        public override HGraphAttributeContent Copy()
        {
            return new BooleanHGraphAttribute() { Value = this.Value };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[] { Convert.ToDouble(Value) };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new BooleanHGraphAttribute() { Value = Convert.ToBoolean(featureVector[0]) };
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                Value = (bool)Convert.ChangeType(value, typeof(bool));
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Value;
        }

    }
    [Serializable]
    public class FloatHGraphAttribute : HGraphAttributeContent
    {
        public float Value;
        public override HGraphAttributeContent Copy()
        {
            return new FloatHGraphAttribute() { Value = this.Value };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[] { Value };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new FloatHGraphAttribute() { Value = (float)featureVector[0] };
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                Value = (float)Convert.ChangeType(value, typeof(float));
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Value;
        }
    }
    [Serializable]
    public class EnumHGraphAttribute : HGraphAttributeContent
    {
        public const uint MaxEnumSize = 16;
        public string Enum;
        public int Value;

        public override HGraphAttributeContent Copy()
        {
            return new EnumHGraphAttribute { Value = this.Value, Enum = this.Enum };
        }
        public override double[] EncodeFeatureVector()
        {
            double[] oneHot = new double[MaxEnumSize];
            oneHot[Value] = 1;
            return oneHot;
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            throw new InvalidOperationException("Cannot reconstruct EnumAttributeContent");
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                if (value is int sv)
                    Value = sv;
                else if (value is Tuple<string, int> tuple)
                {
                    Enum = tuple.Item1;
                    Value = tuple.Item2;
                }
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Value;
        }
    }

    [Serializable]
    public class FlagsEnumHGraphAttribute : EnumHGraphAttribute
    {
        public override double[] EncodeFeatureVector()
        {
            var hEnum = HGraph.Instance.EnumDefinitions[this.Enum];
            double[] oneHot = new double[MaxEnumSize];
            var flags = hEnum.GetFlags(Value);
            foreach (var flag in flags)
            {
                oneHot[hEnum.Entries.FindIndex(0, e => e.Name == flag)] = 1;
            }
            return oneHot;

            throw new NotImplementedException();
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            throw new InvalidOperationException("Cannot reconstruct EnumAttributeContent");
        }
        public override HGraphAttributeContent Copy()
        {
            return new FlagsEnumHGraphAttribute { Value = this.Value, Enum = this.Enum };
        }
    }

    [Serializable]
    public class RangeHGraphAttribute : HGraphAttributeContent
    {
        public float LowValue;
        public float HighValue;

        public override HGraphAttributeContent Copy()
        {
            return new RangeHGraphAttribute { LowValue = this.LowValue, HighValue = this.HighValue };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[] { LowValue, HighValue };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new RangeHGraphAttribute() { LowValue = (float)featureVector[0], HighValue = (float)featureVector[1] };
        }

        public override bool TrySetValue(object value)
        {
            try
            {
                Tuple<float, float> tuple = (Tuple<float, float>)Convert.ChangeType(value, typeof(Tuple<float, float>));
                LowValue = tuple.Item1;
                HighValue = tuple.Item2;
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Tuple.Create(LowValue, HighValue);
        }
    }

    [Serializable]
    public class Vector2HGraphAttribute : HGraphAttributeContent
    {
        public Vector2 Vector;
        public override HGraphAttributeContent Copy()
        {
            return new Vector2HGraphAttribute() { Vector = this.Vector };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[]
            {
            Vector.x,
            Vector.y
            };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new Vector2HGraphAttribute { Vector = new Vector2((float)featureVector[0], (float)featureVector[1]) };
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                Vector = (Vector2)Convert.ChangeType(value, typeof(Vector2));
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Vector;
        }
    }

    [Serializable]
    public class Vector3HGraphAttribute : HGraphAttributeContent
    {
        public Vector3 Vector;
        public override HGraphAttributeContent Copy()
        {
            return new Vector3HGraphAttribute() { Vector = this.Vector };
        }
        public override double[] EncodeFeatureVector()
        {
            return new double[]
            {
            Vector.x,
            Vector.y,
            Vector.z
            };
        }
        public override HGraphAttributeContent DecodeFeatureVector(double[] featureVector)
        {
            return new Vector3HGraphAttribute { Vector = new Vector3((float)featureVector[0], (float)featureVector[1], (float)featureVector[2]) };
        }
        public override bool TrySetValue(object value)
        {
            try
            {
                Vector = (Vector3)Convert.ChangeType(value, typeof(Vector3));
                return true;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }
        public override object GetValue()
        {
            return Vector;
        }
    }

    #endregion
}
