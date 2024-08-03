using Achioto.Gamespace_PCG.Runtime.PCG.Database;
using Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator
{
    public class RandomAssetPlacementGenerator : HGraphGenerator
    {
        public PCGAssetDatabase _assetDataBase;
        [SerializeField] private GameObject _generatedContent;
        [Range(0f, 360f)]
        [SerializeField] float _maxAssetTiltAngle;
        [SerializeField] PCGPointSampler _pointSampler;

        [SerializeField] protected HideFlags _contentFlags = HideFlags.None;


        private GameObject GetRandomAsset(IEnumerable<string> assets)
        {
            var assetPath = assets.ElementAt(Random.Range(0, assets.Count()));
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
        private Quaternion GetRandomRotation(Vector3 surfaceNormal)
        {
            Quaternion randomTilt = Quaternion.AngleAxis(Random.Range(0f, _maxAssetTiltAngle), Random.onUnitSphere);
            Quaternion randomYRotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            return Quaternion.FromToRotation(Vector3.up, surfaceNormal) * randomTilt * randomYRotation;
        }

        protected override void Clear()
        {
            if (_generatedContent != null)
                DestroyImmediate(_generatedContent);
        }

        protected override void Generate()
        {
            var assets = _assetDataBase.GetAssetPaths();
            if (assets.Count() == 0)
            {
                Debug.LogError("Generator does not have any Assets to place!");
                return;
            }
            var sampledPoints = _pointSampler.SamplePoints();
            var transformedPoints = TransformPoints(sampledPoints);
            foreach (var p in transformedPoints)
            {
                var asset = GetRandomAsset(assets);
                var instance = Instantiate(asset, p.Position, p.Rotation, _generatedContent.transform);
                instance.transform.localScale = p.Scale;
                instance.hideFlags = _contentFlags;
            }
        }
        private IEnumerable<PCGPoint> TransformPoints(IEnumerable<PCGPoint> points)
        {
            return points.Select(p =>
            {
                p.Scale = Vector3.one;
                p.Rotation = GetRandomRotation(p.Normal);
                return p;
            });
        }

        protected override void PrepareGeneration()
        {
            Clear();
            _generatedContent = new GameObject("generatedContent");
            _generatedContent.transform.SetParent(transform, false);
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