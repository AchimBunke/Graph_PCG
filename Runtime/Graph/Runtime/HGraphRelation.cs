using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    [Serializable]
    public class HGraphRelation : HGraphRuntimeBase
    {
        [field: SerializeField]
        public ReactiveProperty<string> Source { get; } = new();
        [field: SerializeField]
        public ReactiveProperty<string> Target { get; } = new();

        [field: SerializeField]
        public SerializedReactiveDictionary<string, HGraphAttributeRelation> AttributeRelations { get; } = new();
         

        private ReactiveProperty<HGraphSceneRelation> _sceneRelation = new();
        public IReactiveProperty<HGraphSceneRelation> SceneRelation => _sceneRelation;

        public static HGraphRelation Construct(string id, string sourceId = null, string targetId = null)
        {
            var relation = new HGraphRelation();
            relation.HGraphId.Value = id;
            if (!string.IsNullOrEmpty(sourceId))
                relation.Source.Value = sourceId;
            if (!string.IsNullOrEmpty(targetId))
                relation.Target.Value = targetId;
            return relation;
        }
        public static HGraphRelation Construct(HGraphRelationData data)
        {
            var relation = Construct(data.id, data.source, data.target);
            foreach(var attributeRelation in data.attributeRelations)
            {
                var attRel = HGraphAttributeRelation.Construct(attributeRelation);
                relation.AttributeRelations[attRel.Category.Value] = attRel;
            }
            return relation;
        }

        public bool Connect(HGraphSceneRelation sceneRelation)
        {
            if (_sceneRelation.Value != null)
                return false;
            _sceneRelation.Value = sceneRelation;
            return true;
        }
        public void Disconnect()
        {
            _sceneRelation.Value = null;
        }

        public void Update(HGraphRelationData relationData, bool additive = false)
        {
            Source.Value = relationData.source; 
            Target.Value = relationData.target;
            if (!additive)
            {
                var attibuteRelationsToRemove = AttributeRelations.Keys.Except(relationData.attributeRelations.Select(a => a.category));
                foreach (var toRemove in attibuteRelationsToRemove)
                {
                    AttributeRelations.Remove(toRemove);
                }
            }
            foreach (var toAdd in relationData.attributeRelations)
            {
                if (AttributeRelations.TryGetValue(toAdd.category, out var existingAttributeRelation))
                {
                    existingAttributeRelation.Update(toAdd);
                }
                else
                {
                    var newAttRelation = HGraphAttributeRelation.Construct(toAdd);
                    AttributeRelations.Add(newAttRelation.Category.Value, newAttRelation);
                }
            }
        }

        public bool TryGetSourceNode(out HGraphNode sourceNode)
        {
            return HGraph.Instance.Nodes.TryGetValue(Source.Value, out sourceNode);
        }
        public bool TryGetTargetNode(out HGraphNode targetNode)
        {
            return HGraph.Instance.Nodes.TryGetValue(Target.Value, out targetNode);
        }

    }
}
