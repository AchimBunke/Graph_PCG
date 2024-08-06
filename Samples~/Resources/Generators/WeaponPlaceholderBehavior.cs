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

using Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
#if USE_HGRAPH_MODULES
    [PCGGraphModule]
#endif
    public class WeaponPlaceholderBehavior : HGraphGenerator
    {
        [SerializeField, PCGGraphAttribute(name: "WeaponType")] WeaponType _weaponType;
        [SerializeField] GameObject[] _availableWeapons;

        [SerializeField, ReadOnlyField] GameObject _instatiatedAsset;
        WeaponPlaceholder placeholderAlgorithm = new();

        protected override void Clear()
        {
            if (_instatiatedAsset != null)
                DestroyImmediate(_instatiatedAsset);
        }

        protected override void Generate()
        {
            //Undo.RecordObject(transform, "Generate Weapon");
            var instance = placeholderAlgorithm.CreateAssetInstance(transform);
            Undo.RegisterCreatedObjectUndo(instance, "Generate Weapon");
            Undo.RecordObject(this, "Generate Weapon");
            _instatiatedAsset = instance;
            EditorUtility.SetDirty(this);

        }

        protected override void PrepareGeneration()
        {
            Clear();
            var sceneNode = GetComponent<HGraphSceneNode>();
            if (sceneNode != null)
            {
                var pcgGraph = PCGGraphManager.Instance.PCGGraph;
                if (pcgGraph.Nodes.TryGetValue(sceneNode.HGraphId.Value, out var node))
                {
                    PCGGraphModuleManager.LoadGraphAttributesIntoMembers(this, node);
                }
            }

            placeholderAlgorithm.WeaponType = _weaponType;
            placeholderAlgorithm.AvailableWeapons = _availableWeapons;
        }

        public WeaponPlaceholderBehavior()
        {
            _generatorTypes = new string[]
            {
                 "assets",
                 "loot"
            };
        }
    }
}
