using System;
using UniRx;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{
    [Serializable]
    public class HGraphRuntimeBase : IDisposable
    {
        [ReadOnlyField]
        [SerializeField]
        private ReactiveProperty<string> _hGraphId = new();

        public ReactiveProperty<string> HGraphId => _hGraphId;

        public virtual void Dispose() { }
    }
}
