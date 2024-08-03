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
