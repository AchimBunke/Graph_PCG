using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Interpolation;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.PCG.Database;
using Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers;
using ALib.Collections;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;
using static UnityUtilities.Attributes.ShowIfAttribute;
using Random = UnityEngine.Random;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator
{
    public class HGraphAssetPlacementGenerator : HGraphGenerator
    {
        [SerializeField] HGraphSceneNode _regionNode;
        [SerializeField] PCGAssetDatabase _assetDataBase;
        [SerializeField] GameObject _generatedContent;
        [SerializeField] PCGPointSampler _pointSampler;
        [SerializeField] protected HideFlags _contentFlags = HideFlags.None;

        [SerializeField] bool _randomizeTilt = false;
        [SerializeField, ShowIfBool(nameof(_randomizeTilt), HideInInspector = true), Range(0f, 360f)] float _maxAssetTiltAngle;

        [SerializeField] bool _randomizeScale = false;
        [SerializeField, ShowIfBool(nameof(_randomizeScale), HideInInspector = true)] Vector3 _minScale = Vector3.one;
        [SerializeField, ShowIfBool(nameof(_randomizeScale), HideInInspector = true)] Vector3 _maxScale = Vector3.one;


        [SerializeField] HGraphAssetSelectionMode _generationMode;
        [SerializeField,
            ShowIf(EvaluationModeType.OR,
            HideInInspector: true,
            nameof(IsRandomTopK),
            nameof(IsWeightedTopK))]
        int _topK = 1;

        [SerializeField] HGraphInterpolationConfiguration _interpolationConfiguration;
        [SerializeField] SpatialDistanceMeasureConfiguration _spatialDistanceConfiguration;
        [SerializeField] FeatureDistanceMeasureConfiguration _featureDistanceConfiguration;
        [SerializeField] HGraphSpaceSearchSettings _spaceSearchSettings = HGraphSpaceSearchSettings.Default;
        [SerializeField] ActivationFunctionConfiguration _activationFunctionConfiguration;

        private bool IsRandomTopK() => _generationMode == HGraphAssetSelectionMode.RandomTopK;
        private bool IsWeightedTopK() => _generationMode == HGraphAssetSelectionMode.WeightedTopK;
        public enum HGraphAssetSelectionMode
        {
            RandomTopK,
            WeightedTopK
        }

        private List<string> _assetPaths;

        private FeatureDistanceMeasure _featureDistanceMeasure;
        private HGraphAttributeInterpolationMethod _interpolationMethod;
        private SpatialDistanceMeasure _spatialDistanceMeasure;
        private ActivationFunction _activationFunction;
        private HGraphSpaceSearch _spaceSearch = new HGraphSpaceSearch();

        #region RandomTopK
        List<(string guid, HGraphAssetData data)> _sortedRegionAssets;
        //private FeatureVector _regionFeatureVector;

        private GameObject GetRandomGameObjectForPoint_RandomTopK(PCGPoint pcgPoint)
        {
            if (_topK == 0)
                return null;
            var searchResult = _spaceSearch.FindNearbyNodes(_regionNode.NodeData.Value, pcgPoint.Position);
            var nodeSourceForPosition = searchResult.Nodes;
            _interpolationMethod.NodeSource = nodeSourceForPosition;

            var interpolatedPointFeatures = _interpolationMethod.InterpolateFeatures(pcgPoint.Position, _spatialDistanceMeasure, true);
            _sortedRegionAssets.Sort((a, b) =>
            {
                var va = FeatureVectorUtil.CreateFeatureVector(a.data, normalized: true);
                var vb = FeatureVectorUtil.CreateFeatureVector(b.data, normalized: true);
                return _featureDistanceMeasure.FeatureDistance(interpolatedPointFeatures, va).CompareTo(_featureDistanceMeasure.FeatureDistance(interpolatedPointFeatures, vb));
            });
            var random = Random.Range(0, (int)_topK);
            var index = Mathf.Clamp(random, 0, _sortedRegionAssets.Count);
            var guid = _sortedRegionAssets[index].guid;
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            return obj;
        }
        private void PrepareRandomTopK()
        {
            var assetRegistry = HGraphAssetRegistry.Instance;
            _interpolationMethod = _interpolationConfiguration.Create();
            _spaceSearch.Settings = _spaceSearchSettings;
            //var regionNeighborhood = _spaceSearch.FindNearbyNodes
            //_interpolationMethod.NodeSource = regionNeighborhood;

            _featureDistanceMeasure = _featureDistanceConfiguration.Create();
            _spatialDistanceMeasure = _spatialDistanceConfiguration.Create();
            //_regionFeatureVector = FeatureVectorUtil.CreateFeatureVector(_regionNode.NodeData.Value, normalized: true);

            _sortedRegionAssets = _assetPaths.Select(path =>
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                assetRegistry.Assets.TryGetValue(guid, out var assetData);
                return (guid, assetData);
            }).ToList();

        }
        #endregion

        #region WeightedTopK
        private WeightedRandomAccessCollection<(string guid, HGraphAssetData data)> _weightedRegionAssets;
        private GameObject GetRandomGameObjectForPoint_WeightedTopK(PCGPoint pcgPoint)
        {
            if (_topK == 0)
                return null;
            var searchResult = _spaceSearch.FindNearbyNodes(_regionNode.NodeData.Value, pcgPoint.Position);
            var nodeSourceForPosition = searchResult.Nodes;
            _interpolationMethod.NodeSource = nodeSourceForPosition;


            var interpolatedPointFeatures = _interpolationMethod.InterpolateFeatures(pcgPoint.Position, _spatialDistanceMeasure, true);
            _sortedRegionAssets.Sort((a, b) =>
            {
                var va = FeatureVectorUtil.CreateFeatureVector(a.data, normalized: true);
                var vb = FeatureVectorUtil.CreateFeatureVector(b.data, normalized: true);
                return _featureDistanceMeasure.FeatureDistance(interpolatedPointFeatures, va, true).CompareTo(_featureDistanceMeasure.FeatureDistance(interpolatedPointFeatures, vb, true));
            });
            int index = 0;
            float[] weights = new float[_sortedRegionAssets.Count];
            foreach (var a in _sortedRegionAssets)
            {
                var va = FeatureVectorUtil.CreateFeatureVector(a.data, normalized: true);
                var weight = _featureDistanceMeasure.FeatureDistance(interpolatedPointFeatures, va, normalized: true);
                weights[index] = (float)weight;

                index++;
            }
            weights = _activationFunction.Apply(weights);
            for (int i = 0; i < weights.Count(); ++i)
            {
                _weightedRegionAssets.SetWeight(_sortedRegionAssets[i], i < _topK ? weights[i] : 0);
            }

            if (!_weightedRegionAssets.TryNext(out var randomAsset))
            {
                return null;
            }

            var path = AssetDatabase.GUIDToAssetPath(randomAsset.guid);
            var obj = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
            return obj;
        }
        private void PrepareWeightedTopK()
        {
            var assetRegistry = HGraphAssetRegistry.Instance;
            _interpolationMethod = _interpolationConfiguration.Create();
            _activationFunction = _activationFunctionConfiguration.Create();

            _spaceSearch.Settings = _spaceSearchSettings;

            _featureDistanceMeasure = _featureDistanceConfiguration.Create();
            _spatialDistanceMeasure = _spatialDistanceConfiguration.Create();

            _sortedRegionAssets = _assetPaths.Select(path =>
            {
                var guid = AssetDatabase.AssetPathToGUID(path);
                assetRegistry.Assets.TryGetValue(guid, out var assetData);
                return (guid, assetData);
            }).ToList();
            _weightedRegionAssets = new WeightedRandomAccessCollection<(string guid, HGraphAssetData data)>(_sortedRegionAssets);
        }
        #endregion

        private GameObject GetRandomGameObjectForPoint(PCGPoint pcgPoint)
        {
            var currentRegion = _regionNode;
            if (currentRegion == null)
                return null;
            switch (_generationMode)
            {
                case HGraphAssetSelectionMode.RandomTopK:
                    return GetRandomGameObjectForPoint_RandomTopK(pcgPoint);
                case HGraphAssetSelectionMode.WeightedTopK:
                    return GetRandomGameObjectForPoint_WeightedTopK(pcgPoint);
                default:
                    return null;
            }
        }

        protected override void Clear()
        {
            if (_generatedContent != null)
                DestroyImmediate(_generatedContent);
        }
        protected override void Generate()
        {
            if (_assetPaths == null || _assetPaths.Count() == 0)
            {
                Debug.LogError("Generator does not have any Assets to place!");
                return;
            }
            var sampledPoints = _pointSampler.SamplePoints();
            var transformedPoints = TransformPoints(sampledPoints);
            foreach (var p in transformedPoints)
            {
                var asset = GetRandomGameObjectForPoint(p);
                if (asset == null)
                {
                    Debug.LogWarning($"Chosen asset {asset} cannot be instantiated in scene.");
                    continue;
                }
                var instance = Instantiate(asset, p.Position, p.Rotation, _generatedContent.transform);
                instance.transform.localScale = p.Scale;
                instance.hideFlags = _contentFlags;
            }
        }
        private IEnumerable<PCGPoint> TransformPoints(IEnumerable<PCGPoint> points)
        {
            return points.Select(p =>
            {
                p.Scale = GetRandomScale();
                p.Rotation = GetRandomRotation(p.Normal);
                return p;
            });
        }
        private Quaternion GetRandomRotation(Vector3 surfaceNormal)
        {
            Quaternion randomTilt = Quaternion.identity;
            if (_randomizeTilt)
                randomTilt = Quaternion.AngleAxis(Random.Range(0f, _maxAssetTiltAngle), Random.onUnitSphere);
            Quaternion randomYRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            return Quaternion.FromToRotation(Vector3.up, surfaceNormal) * randomTilt * randomYRotation;
        }
        private Vector3 GetRandomScale()
        {
            if (!_randomizeScale)
                return Vector3.one;
            return Vector3.Lerp(_minScale, _maxScale, Random.Range(0f, 1f));
        }
        protected override void PrepareGeneration()
        {
            Clear();
            _generatedContent = new GameObject("generatedContent");
            _generatedContent.transform.SetParent(transform, false);
            _assetPaths = _assetDataBase.GetAssetPaths().ToList();
            switch (_generationMode)
            {
                case HGraphAssetSelectionMode.RandomTopK:
                    PrepareRandomTopK();
                    break;
                case HGraphAssetSelectionMode.WeightedTopK:
                    PrepareWeightedTopK();
                    break;
            }
        }
        private void Update()
        {
            if (_generatedContent != null)
            {
                if (_generatedContent.transform.childCount > 0 && _generatedContent.transform.GetChild(0).gameObject.hideFlags != _contentFlags)
                {
                    //_generatedContent.hideFlags = _contentFlags;
                    for (int i = 0; i < _generatedContent.transform.childCount; ++i)
                    {
                        _generatedContent.transform.GetChild(i).gameObject.hideFlags = _contentFlags;
                    }
                }
            }

        }
    }
}