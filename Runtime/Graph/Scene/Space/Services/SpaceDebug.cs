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

using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Interpolation;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.PCG.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Timers;
using Object = UnityEngine.Object;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services
{
    [ExecuteInEditMode]
    public class SpaceDebug : MonoBehaviour
    {
        public HGraphSpaceSearchSettings _spaceSearchSettings = HGraphSpaceSearchSettings.Default;

        public HGraphNodeSpace currentSpace;
        public List<NodeInformation> nearbyNodes = new();
        private FeatureVector interpolatedFeatures;
        private FeatureVector normalizedInterpolatedFeatures;
        public string interpolatedFeatureInformation;
        public string interpolatedFeatureInformation_normalized;

        public List<AssetInformation> recommendedAssets = new();
        public PCGAssetDatabase assetDatabase;
        public HGraphInterpolationConfiguration interpolationConfiguration;
        public SpatialDistanceMeasureConfiguration spatialDistanceMeasureConfiguration;
        public FeatureDistanceMeasureConfiguration featureDistanceConfiguration;
        public ActivationFunctionConfiguration activationFunctionConfiguration;

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireSphere(transform.position, _spaceSearchSettings.MaxDistance);
            foreach (var pair in nearbyNodes)
            {
                Gizmos.DrawLine(transform.position, pair.Node.transform.position);
                Handles.Label(Vector3.Lerp(transform.position, pair.Node.transform.position, 0.5f), pair.SpatialDistance.ToString());
            }
        }
        public void Run()
        {
            var spaceSearch = new HGraphSpaceSearch(_spaceSearchSettings);
            currentSpace = spaceSearch.FindSpace(transform.position);
            nearbyNodes.Clear();
            interpolatedFeatureInformation = string.Empty;
            interpolatedFeatureInformation_normalized = string.Empty;
            recommendedAssets.Clear();

            if (currentSpace != null)
            {
                var result = spaceSearch.FindNearbyNodes(transform.position);

                var interpolationMethod = interpolationConfiguration.Create();
                var spatialDistanceMeasure = spatialDistanceMeasureConfiguration.Create();
                var featureDistanceMeasure = featureDistanceConfiguration.Create();
                interpolationMethod.NodeSource = result.Nodes;
                interpolatedFeatures = interpolationMethod.InterpolateFeatures(transform.position, spatialDistanceMeasure);
                normalizedInterpolatedFeatures = interpolatedFeatures.Normalized();
                interpolatedFeatureInformation = interpolatedFeatures.ToString();
                interpolatedFeatureInformation_normalized = normalizedInterpolatedFeatures.ToString();
                var activationFunction = activationFunctionConfiguration.Create();


                for (int i = 0; i < result.Nodes.Count; ++i)
                {
                    var otherNode = result.Nodes[i];
                    var normalizedNodeFeatures = FeatureVectorUtil.CreateFeatureVector(otherNode, normalized: true);
                    var featureDistance = featureDistanceMeasure.FeatureDistance(normalizedInterpolatedFeatures, normalizedNodeFeatures);
                    nearbyNodes.Add(
                        new NodeInformation(
                            otherNode.SceneNode.Value,
                            result.Distances[i],
                            (float)featureDistance,
                            normalizedNodeFeatures.ToString(),
                            -1f));
                }
                // apply ActivationFunction
                var distances = nearbyNodes.Select(n => n.FeatureDistance).ToArray();
                distances = activationFunction.Apply(distances);
                for (int i = 0; i < distances.Length; ++i)
                {
                    var node = nearbyNodes[i];
                    node.ActivatedFeatureDistance = distances[i];
                    nearbyNodes[i] = node;
                }

                LoadAndSortAsset();
            }


        }
        private void LoadAndSortAsset()
        {
            var assetRegistry = HGraphAssetRegistry.Instance;
            var featureDistanceMeasure = featureDistanceConfiguration.Create();
            foreach (var assetPath in assetDatabase.GetAssetPaths())
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                assetRegistry.Assets.TryGetValue(guid, out var assetData);
                var assetInformation = new AssetInformation();
                assetInformation.Asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                var normalizedAssetfeatures = FeatureVectorUtil.CreateFeatureVector(assetData, normalized: true);
                var distance = featureDistanceMeasure.FeatureDistance(normalizedInterpolatedFeatures, normalizedAssetfeatures);
                var normalizedDistance = featureDistanceMeasure.FeatureDistance(normalizedInterpolatedFeatures, normalizedAssetfeatures, normalized: true);
                assetInformation.FeatureDistance = (float)distance;
                assetInformation.ActivatedFeatureDistance = -1f;
                assetInformation.FeatureVector = normalizedAssetfeatures.ToString();
                recommendedAssets.Add(assetInformation);
            }
            // apply ActivationFunction
            var activationFunction = activationFunctionConfiguration.Create();
            var distances = recommendedAssets.Select(a => a.FeatureDistance).ToArray();
            distances = activationFunction.Apply(distances);
            for (int i = 0; i < distances.Length; ++i)
            {
                var assetInfo = recommendedAssets[i];
                assetInfo.ActivatedFeatureDistance = distances[i];
                recommendedAssets[i] = assetInfo;
            }
            recommendedAssets.Sort((a, b) =>
            {
                return a.FeatureDistance.CompareTo(b.FeatureDistance);
            });
        }

        private ManualTimer _manualTimer;

        private void Update()
        {
            _manualTimer.Update(Time.deltaTime);
        }
        private void OnEnable()
        {
            if (Application.isPlaying)
                return;

            _manualTimer = new ManualTimer(1f);
            _manualTimer.Elapsed += () =>
            {
                Run();
            };
            _manualTimer.StartTimer();
        }
    }
    [CustomEditor(typeof(SpaceDebug))]
    public class SpaceSearchTestCameraEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Test"))
            {
                var test = (SpaceDebug)target;
                test.Run();
            }
        }
    }
    [Serializable]
    public struct NodeInformation
    {
        public HGraphSceneNode Node;
        public float SpatialDistance;
        public float FeatureDistance;
        public float ActivatedFeatureDistance;
        public string NormalizedFeatureVector;
        public NodeInformation(HGraphSceneNode node, float spatialDistance, float featureDistance,
            string normalizdeFeatureVector, float activatedFeatureDistance)
        {
            Node = node;
            SpatialDistance = spatialDistance;
            FeatureDistance = featureDistance;
            NormalizedFeatureVector = normalizdeFeatureVector;
            ActivatedFeatureDistance = activatedFeatureDistance;
        }
    }

    [Serializable]
    public struct AssetInformation
    {
        public Object Asset;
        public string FeatureVector;
        public float FeatureDistance;
        public float ActivatedFeatureDistance;
    }
}

