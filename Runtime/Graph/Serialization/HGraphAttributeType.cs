﻿/*
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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Serialization
{
    public enum HGraphAttributeType
    {
        Nominal,
        Float,
        Enum,
        FlagsEnum,
        Range,
        Vector3,
        Vector2,
        Boolean,

    }

    public static class HGraphAttributeTypeExtensions
    {
        public static HGraphAttributeContent CreateData(this HGraphAttributeType type)
        {
            var contentType = GetContentType(type);
            ConstructorInfo constructor = contentType?.GetConstructor(Type.EmptyTypes);
            if (constructor != null)
            {
                HGraphAttributeContent instance = constructor.Invoke(null) as HGraphAttributeContent;
                return instance;
            }
            return null;
        }
        public static Type GetContentType(this HGraphAttributeType type)
        {
            switch (type)
            {
                case HGraphAttributeType.Nominal:
                    return typeof(NominalHGraphAttribute);
                case HGraphAttributeType.Float:
                    return typeof(FloatHGraphAttribute);
                case HGraphAttributeType.Enum:
                    return typeof(EnumHGraphAttribute);
                case HGraphAttributeType.FlagsEnum:
                    return typeof(FlagsEnumHGraphAttribute);
                case HGraphAttributeType.Range:
                    return typeof(RangeHGraphAttribute);
                case HGraphAttributeType.Vector3:
                    return typeof(Vector3HGraphAttribute);
                case HGraphAttributeType.Vector2:
                    return typeof(Vector2HGraphAttribute);
                case HGraphAttributeType.Boolean:
                    return typeof(BooleanHGraphAttribute);
                default:
                    return null;
            }
        }
        public static bool IsNumericValueType(this HGraphAttributeType type, out Type valueType)
        {
            switch (type)
            {
                case HGraphAttributeType.Nominal:
                    {
                        valueType = typeof(uint);
                        return true;
                    }
                case HGraphAttributeType.Boolean:
                    {
                        valueType = typeof(int);
                        return true;
                    }
                case HGraphAttributeType.Float:
                    {
                        valueType = typeof(float);
                        return true;
                    }
                case HGraphAttributeType.Range:
                    {
                        valueType = typeof(float);
                        return true;
                    }
                case HGraphAttributeType.Vector3:
                case HGraphAttributeType.Vector2:
                    {
                        valueType = typeof(float);
                        return true;
                    }
                default:
                    {
                        valueType = typeof(object);
                        return false;
                    }
            }
        }
        public static int GetFeatureLength(this HGraphAttributeType type)
        {
            switch (type)
            {
                case HGraphAttributeType.Nominal:
                    return 1;
                case HGraphAttributeType.Float:
                    return 1;
                case HGraphAttributeType.Enum:
                    return (int)EnumHGraphAttribute.MaxEnumSize;
                case HGraphAttributeType.FlagsEnum:
                    return (int)EnumHGraphAttribute.MaxEnumSize;
                case HGraphAttributeType.Range:
                    return 2;
                case HGraphAttributeType.Vector3:
                    return 3;
                case HGraphAttributeType.Vector2:
                    return 2;
                case HGraphAttributeType.Boolean:
                    return 1;
                default:
                    return 0;
            }
        }

    }
}
