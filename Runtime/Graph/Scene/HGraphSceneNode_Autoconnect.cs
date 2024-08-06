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

using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    public class HGraphSceneNode_Autoconnect : HGraphSceneNode
    {
        [SerializeField] HGraphSceneNode _autoconnectSuperNode;
        public HGraphSceneNode AutoconnectSuperNode => _autoconnectSuperNode;
        protected override void OnEnable()
        {
            base.OnEnable();
            string id;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
            if (HGraphResources.IsHGraphIdValid(HGraphId.Value) && !IsDuplicate.Value)
            {
                // already initialized.. 
                id = HGraphId.Value;
            }
            else
            {
                id = GetInstanceID().ToString();
            }
            var name = gameObject.name;
            HGraphId.Value = id.ToString();
            if (!IsHGraphConnected)
            {
                //Debug.Log("Node not connected to graph.. creating new node");
                ConnectAsNewNode();
                NodeData.Value.Name.Value = name;
                if (_autoconnectSuperNode != null)
                    NodeData.Value.SuperNode.Value = _autoconnectSuperNode.HGraphId.Value;
            }
        }
        protected override void OnValidate()
        {
            base.OnValidate();
        }
    }
}

