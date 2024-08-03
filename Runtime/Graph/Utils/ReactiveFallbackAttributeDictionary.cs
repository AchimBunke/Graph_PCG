using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using System;
using System.Collections.Generic;
using UniRx;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Utils
{
    /// <summary>
    /// Provides Observables whenever the Data of a registered attribute changes.
    /// </summary>
    public class ReactiveFallbackAttributeDictionary : ReactiveFallbackDictionary<string, HGraphAttribute>
    {
        /// <summary>
        /// Need to keep track of disposables on all registered attributes to be able to dispose them.
        /// </summary>
        private Dictionary<string, IDisposable> _disposables = new Dictionary<string, IDisposable>();
        public ReactiveFallbackAttributeDictionary() : base()
        {
            SetupObservers();
        }
        public ReactiveFallbackAttributeDictionary(ReactiveDictionary<string, HGraphAttribute> baseDictionary, IReadOnlyReactiveDictionary<string, HGraphAttribute> fallbackDictionary = null) : base(baseDictionary, fallbackDictionary)
        {
            SetupObservers();
        }
        private void SetupObservers()
        {
            this.ObserveAdd().Subscribe(e =>
            {
                _disposables[e.Key] = e.Value.DataChanged.Subscribe(_ => OnAttributeDataChanged(e.Value));
            });
            this.ObserveRemove().Subscribe(e => { _disposables[e.Key].Dispose(); _disposables.Remove(e.Key); });
            this.ObserveReplace().Subscribe(e =>
            {
                _disposables[e.Key].Dispose(); _disposables.Remove(e.Key);
                _disposables[e.Key] = e.NewValue.DataChanged.Subscribe(_ => OnAttributeDataChanged(e.NewValue));
            });
            this.ObserveReset().Subscribe(e =>
            {
                foreach (var kv in _disposables)
                {
                    kv.Value.Dispose();
                }
                _disposables.Clear();
                foreach (var kv in this)
                {
                    _disposables[kv.Key] = kv.Value.DataChanged.Subscribe(_ => OnAttributeDataChanged(kv.Value));
                }
            });
        }
        private void OnAttributeDataChanged(HGraphAttribute source)
        {
            _attributeDataChangedSubject?.OnNext(source);
        }
        Subject<HGraphAttribute> _attributeDataChangedSubject = null;
        /// <summary>
        /// Emits whenever the data of an attribute changes.
        /// </summary>
        /// <returns></returns>
        public IObservable<HGraphAttribute> ObserverAttributeDataChanged()
        {
            if (_attributeDataChangedSubject == null)
                _attributeDataChangedSubject = new Subject<HGraphAttribute>();
            return _attributeDataChangedSubject;
        }
    }
}
