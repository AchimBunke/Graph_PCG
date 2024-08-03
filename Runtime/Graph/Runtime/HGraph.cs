using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using System;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    public class HGraph
    {
        static HGraph instance;
        public static HGraph Instance => instance ?? (instance = new HGraph());

        private HGraph()
        {
            EnumDefinitions.ObserveAnyChange()
                .Merge(Nodes.ObserveAnyChange())
                .Merge(Categories.ObserveAnyChange())
                .Merge(Relations.ObserveAnyChange())
                .Merge(Rules.Select(_=>Unit.Default))
                .Subscribe(_ => PCGGraphManager.Instance.SetPCGGraphDirty());
        }
        public ReactiveDictionary<string, HGraphEnum> EnumDefinitions { get; } = new();
        public ReactiveDictionary<string, HGraphNode> Nodes { get; } = new();
        public ReactiveDictionary<string, HGraphCategory> Categories { get; } = new();
        public ReactiveDictionary<string, HGraphRelation> Relations { get; } = new();
        //public ReactiveCollection<Rules.HGraphRule> Rules { get; } = new();
        public ReactiveProperty<HGraphRuleCollection> Rules { get; set; } = new();

        public ReactiveDictionary<string, HGraphSceneNode> SceneNodes { get; } = new();
        public ReactiveDictionary<string, HGraphSceneRelation> SceneRelations { get; } = new();

        public HGraphSceneRoot SceneRoot
        {
            get
            {
                var o = GameObject.FindFirstObjectByType<HGraphSceneRoot>();
                if (o == null)
                {
                    var obj = new GameObject();
                    obj.name = "HGraphSceneRoot";
                    o = obj.AddComponent<HGraphSceneRoot>();
                }
                return o;
            }
        }

        public event Action GraphLoaded;
        public void RaiseGraphLoaded()
        {
            GraphLoaded?.Invoke();
        }

    }
    
}
