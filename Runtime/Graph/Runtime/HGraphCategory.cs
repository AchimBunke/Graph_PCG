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
using UniRx;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    [Serializable]
    public class HGraphCategorySO : ScriptableObject
    {
        [SerializeField]
        public HGraphCategory Category;
    }
    [Serializable]
    public class HGraphCategory : HGraphRuntimeBase
    {

        [field: SerializeField]
        private ReactiveProperty<string> _name = new();

        public ReactiveProperty<string> Name => _name;

        [ReadOnlyField]
        [field:SerializeField]
        private ReactiveProperty<HGraphAttributeType> _type = new();
        public ReactiveProperty<HGraphAttributeType> Type => _type;

        [ReadOnlyField]
        [SerializeField]
        private float _minValue;
        [ReadOnlyField]
        [SerializeField]
        private float _maxValue;
        public float MinValue { get => _minValue; set => _minValue = value; }
        public float MaxValue { get => _maxValue; set => _maxValue = value; }

        [SerializeReference]
        public HGraphAttributeContent DefaultData;


        [SerializeField]
        private Color _displayColor;
        public Color DisplayColor => _displayColor;

        public HGraphAttribute CreateAttribute()
        {
            var attribute = HGraphAttribute.Construct(DefaultData?.Copy() ?? _type.Value.CreateData());
            attribute.Category.Value = this.HGraphId.Value;
            return attribute;
        }

        public static HGraphCategory Construct(string id)
        {
            var category = new HGraphCategory();
            //category.DefaultData = new HGraphAttributeContent();
            category.HGraphId.Value = id;
            return category;
        }
        public static HGraphCategory Construct(HGraphCategoryData data)
        {
            var category = Construct(data.id);
            category.Name.Value = data.name;
            category._type.Value = data.type;
            category.DefaultData = data.defaultContent;
            category._displayColor = data.displayColor;
            category._minValue = data.minValue;
            category._maxValue = data.maxValue;
            return category;
        }
        public void Update(HGraphCategoryData categoryData)
        {
            Name.Value = categoryData.name;
            _type.Value = categoryData.type;
            DefaultData = categoryData.defaultContent;
            _displayColor = categoryData.displayColor;
            MinValue = categoryData.minValue;
            MaxValue = categoryData.maxValue;
        }

    
    }
}
