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
        public ReactiveProperty<HGraphAttributeRelationContent> Data { get; } = new();

        private HGraphAttributeRelation() 
        {
            Data.Subscribe(v => _data = v);
        }

        public static HGraphAttributeRelation Construct(string id, string categoryId, HGraphAttributeRelationContent data)
        {
            var attributeRelation = new HGraphAttributeRelation();
            attributeRelation.HGraphId.Value = id ;
            attributeRelation._category.Value = categoryId;
            attributeRelation.Data.Value = data;
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
            Data.Value = data.data;
        }
    }
}
