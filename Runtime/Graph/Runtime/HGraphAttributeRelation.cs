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
    public class HGraphAttributeRelationSO : ScriptableObject
    {
        public HGraphAttributeRelation AttributeRelation;
    }

    [Serializable]
    public class HGraphAttributeRelation : HGraphRuntimeBase
    {
        [ReadOnlyField]
        [SerializeField]
        private ReactiveProperty<string> _category = new();
        public ReactiveProperty<string> Category => _category;

        [SerializeReference]
        private HGraphAttributeRelationContent _data = new();
        public HGraphAttributeRelationContent Data => _data;

        private HGraphAttributeRelation() 
        {
        }

        public static HGraphAttributeRelation Construct(string id, string categoryId, HGraphAttributeRelationContent data)
        {
            var attributeRelation = new HGraphAttributeRelation();
            attributeRelation.HGraphId.Value = id ;
            attributeRelation._category.Value = categoryId;
            attributeRelation._data = data;
            return attributeRelation;
        }
        public static HGraphAttributeRelation Construct(HGraphAttributeRelationData data)
        {
            var attributeRelation = Construct(data.id, data.category, data.data);
            return attributeRelation;
        }
        public void Update(HGraphAttributeRelationData data)
        {
            Category.Value = data.category;
            _data = data.data;
        }
    }
}
