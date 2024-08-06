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

using Achioto.Gamespace_PCG.Runtime.Graph.PCG.Database;
using Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.PCG.Database;
using Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;
using UnityUtilities.Timers;
using static Achioto.Gamespace_PCG.Runtime.Graph.PCG.Database.PCGNodeDistanceAssetFilter;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets
{
    /// <summary>
    /// Acts as a data transformer for PCG data for AssetPlacer input.
    /// </summary>
    public class AssetPlacerBehavior : HGraphGenerator
    {

        [SerializeField] PCGAssetDatabase _assetDataBase;
        [SerializeField] PCGPointSampler _pointSampler;
        [SerializeField] AssetPlacer.AssetPlacerSettings _settings = AssetPlacer.AssetPlacerSettings.Default;
        [SerializeField] bool _useGraph;
        [SerializeField, ShowIfBool(nameof(_useGraph))] HGraphSceneNode _node;

        [SerializeField] bool _useGraphDataToFindAssets;
        [SerializeField, ShowIfBool(nameof(_useGraphDataToFindAssets))] PCGNodeDistanceAssetFilterSettings _assetFilterSettings = PCGNodeDistanceAssetFilterSettings.Default;
        [SerializeField, ReadOnlyField] Object[] _assetsPreview;

        [SerializeField, ReadOnlyField] GameObject _generatedContent;

        AssetPlacer _assetPlacer;
        PCGNodeDistanceAssetFilter _assetFilter;
        private void OnEnable()
        {
            UpdateHGraphSceneNodeIfNull();
            _timer = new ManualTimer(2);
            _timer.AutoReset = true;
            _timer.Elapsed += () =>
            {
                _assetsPreview = GetAssetSource().Select(p => AssetDatabase.LoadAssetAtPath<Object>(p)).ToArray();
            };
            _timer.StartTimer();
        }
        ManualTimer _timer;
        private void Update()
        {
            _timer.Update(Time.deltaTime);
        }
        protected override void Clear()
        {
            if (_generatedContent != null)
                DestroyImmediate(_generatedContent);
        }

        protected override void Generate()
        {
            _assetsPreview = GetAssetSource().Select(p => AssetDatabase.LoadAssetAtPath<Object>(p)).ToArray();
            _assetPlacer.PlaceAssets(_generatedContent.transform);
            EditorUtility.SetDirty(this);
        }

        protected override void PrepareGeneration()
        {
            Clear();
            _generatedContent = new GameObject("generatedContent");
            _generatedContent.transform.SetParent(transform, false);

            if (_useGraph)
            {
                UpdateHGraphSceneNodeIfNull();

                var pcgGraph = PCGGraphManager.Instance.PCGGraph;
                if (pcgGraph.Nodes.TryGetValue(_node.HGraphId.Value, out var node))
                {
                    _settings = (AssetPlacer.AssetPlacerSettings)PCGGraphModuleManager.LoadGraphAttributesIntoMembers(_settings, node);
                }
                else
                {
                    Debug.LogError($"Node with id: {_node?.HGraphId?.Value} does not exists! ");
                }
            }

            ConfigureAssetPlacer();
        }
        private void ConfigureAssetFilter()
        {
            if (_assetFilter == null)
                _assetFilter = new PCGNodeDistanceAssetFilter();
            _assetFilter.Settings = _assetFilterSettings;
        }
        private void ConfigureAssetPlacer()
        {
            if (_assetPlacer == null)
                _assetPlacer = new AssetPlacer();
            _assetPlacer.PCGPointSampler = _pointSampler;
            _assetPlacer.Settings = _settings;
            _assetPlacer.PCGAssetSource = GetAssetSource();
        }
        private IEnumerable<string> GetAssetSource()
        {
            if (_useGraph && _useGraphDataToFindAssets)
            {
                UpdateHGraphSceneNodeIfNull();
                ConfigureAssetFilter();


                var pcgGraph = PCGGraphManager.Instance.PCGGraph;
                if (pcgGraph.Nodes.TryGetValue(_node.HGraphId.Value, out var node))
                {
                    return _assetFilter.FilterAssets(_assetDataBase.GetAssetPaths(), node);
                }
                else
                {
                    Debug.LogError($"Node with id: {_node?.HGraphId?.Value} does not exists! Filter is not applied!");
                    return _assetDataBase.GetAssetPaths();
                }
            }
            else
            {
                return _assetDataBase.GetAssetPaths();
            }
        }
        private void UpdateHGraphSceneNodeIfNull()
        {
            if (_node == null)
                _node = GetComponent<HGraphSceneNode>();
        }

        public AssetPlacerBehavior()
        {
            _generatorTypes = new string[]
            {
            "assets"
            };
        }

    }
}