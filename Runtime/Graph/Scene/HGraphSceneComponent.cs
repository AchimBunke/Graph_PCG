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
