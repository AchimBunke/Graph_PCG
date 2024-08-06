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
