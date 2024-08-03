using System;
using UniRx;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    [ExecuteInEditMode]
    public abstract class HGraphSceneComponent : MonoBehaviour
    {
        protected ReactiveProperty<bool> _isRegistered = new();
        public IReactiveProperty<bool> IsRegistered => _isRegistered;
        protected ReactiveProperty<bool> _isDuplicate = new();
        public IReactiveProperty<bool> IsDuplicate => _isDuplicate;

        private string _oldGraphId;

        [field: SerializeField]
        protected ReactiveProperty<string> _hGraphId = new();
        public ReactiveProperty<string> HGraphId => _hGraphId;

        private IDisposable _idSubscription;

        protected virtual void OnValidate()
        {
            // Notify when Id changes via inspector
            if (_hGraphId.Value != _oldGraphId)
                _hGraphId.SetValueAndForceNotify(_hGraphId.Value);
        }
        protected virtual void OnEnable()
        {
            _idSubscription = _hGraphId.Subscribe(newId =>
            {
                OnHGraphIdChanged(_oldGraphId, newId);
                _oldGraphId = newId;
            });
        }

        protected virtual void OnDestroy()
        {
            _idSubscription?.Dispose();
        }
        protected abstract void OnHGraphIdChanged(string oldId, string newId);
    }
}
