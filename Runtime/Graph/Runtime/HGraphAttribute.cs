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
    public class HGraphAttributeSO : ScriptableObject
    {
        public HGraphAttribute Attribute;
    }
    [Serializable]
    public class HGraphAttribute
    {

        [ReadOnlyField]
        [SerializeField]
        private ReactiveProperty<string> _category = new();

        public ReactiveProperty<string> Category => _category;

        [ReadOnlyField]
        [SerializeField]
        private HGraphAttributeType _type;
        public HGraphAttributeType Type => _type;

        [SerializeReference]
        private HGraphAttributeContent _data = new();
        public HGraphAttributeContent Data => _data;

        private Subject<UniRx.Unit> _dataChanged = new Subject<UniRx.Unit>();
        public IObservable<UniRx.Unit> DataChanged => _dataChanged;

        private void OnDataChanged()
        {
            _dataChanged.OnNext(UniRx.Unit.Default);
        }
        private HGraphAttribute()
        {
        }
        
        public static HGraphAttribute Construct(HGraphAttributeContent data)
        {
            var attribute = new HGraphAttribute();
            //attribute.HGraphId.Value = attribute.GetHashCode().ToString();
            attribute._data = data;
            attribute._type = data.GetAttributeType();
            attribute._data.DataChanged += attribute.OnDataChanged;
            return attribute;
        }
        public static HGraphAttribute Construct(HGraphAttributeData data)
        {
            var attribute = Construct(data.data);
            //attribute.HGraphId.Value = data.id;
            attribute._type = data.type;
            attribute.Category.Value = data.category;
            return attribute;
        }
        public void Update(HGraphAttributeData attributeData)
        {
            _category.Value = attributeData.category;
            _type = attributeData.type;
            _data.DataChanged -= OnDataChanged;
            _data = attributeData.data;
            _data.DataChanged += OnDataChanged;
        }

    }
}
