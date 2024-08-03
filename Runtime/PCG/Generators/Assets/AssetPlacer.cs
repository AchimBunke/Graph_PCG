using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.PCG.PointSamplers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;
using Random = UnityEngine.Random;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets
{
    public class AssetPlacer
    {
        public PCGPointSampler PCGPointSampler { get; set; }
        /// <summary>
        /// List of asset paths
        /// </summary>
        public IEnumerable<string> PCGAssetSource { get; set; }
        public AssetPlacerSettings Settings { get; set; }

        public void PlaceAssets(Transform parent)
        {
            var assets = PCGAssetSource;
            if (assets == null || assets.Count() == 0)
            {
                Debug.LogError("Generator does not have any Assets to place!");
                return;
            }
            var sampledPoints = PCGPointSampler.SamplePoints();
            var transformedPoints = TransformPoints(sampledPoints);
            foreach (var p in transformedPoints)
            {
                var asset = GetRandomAsset(assets);
                var instance = GameObject.Instantiate(asset, p.Position, p.Rotation, parent);
                instance.transform.localScale = p.Scale;
                instance.hideFlags = Settings.contentFlags;
            }
        }

        private IEnumerable<PCGPoint> TransformPoints(IEnumerable<PCGPoint> points)
        {
            return points.Select(p =>
            {
                p.Scale = RandomizeScale();
                p.Rotation = GetRandomRotation(p.Normal);
                p.Position = ApplyOffset(p.Position, p.Rotation);
                return p;
            });
        }
        private GameObject GetRandomAsset(IEnumerable<string> assets)
        {
            var assetPath = assets.ElementAt(Random.Range(0, assets.Count()));
            return AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        }
        private Vector3 RandomizeScale()
        {
            return Settings.randomizeScale ? Vector3.Lerp(Settings.minScale, Settings.maxScale, Random.Range(0f, 1f)) : Vector3.one;
        }
        private Quaternion GetRandomRotation(Vector3 surfaceNormal)
        {
            Quaternion randomTilt = Settings.randomizeTilt ? Quaternion.AngleAxis(Random.Range(0f, Settings.maxAssetTiltAngle), Random.onUnitSphere) : Quaternion.identity;
            Quaternion randomYRotation = Settings.randomizeRotation ? Quaternion.Euler(0f, Random.Range(0f, 360f), 0f) : Quaternion.identity;
            return Quaternion.FromToRotation(Vector3.up, Settings.absolutRotation ? Vector3.up : surfaceNormal) * randomTilt * randomYRotation;
        }
        private Vector3 ApplyOffset(Vector3 position, Quaternion rotation)
        {
            if (Settings.absolutRotation)
            {
                return position + Settings.offset;
            }
            else
            {
                return position + (rotation * Settings.offset);
            }
        }

        [PCGGraphModule(registerAllMembers: true)]
        [Serializable]
        public struct AssetPlacerSettings
        {
            public bool randomizeRotation;

            public bool randomizeTilt;
            [ShowIfBool(nameof(randomizeTilt)),
                Range(0f, 360f)]
            public float maxAssetTiltAngle;

            public HideFlags contentFlags;

            public bool randomizeScale;

            [ShowIfBool(nameof(randomizeScale))]
            public Vector3 minScale;
            [ShowIfBool(nameof(randomizeScale))]
            public Vector3 maxScale;

            public bool absolutRotation;

            public Vector3 offset;

            public static AssetPlacerSettings Default => new AssetPlacerSettings
            {
                randomizeRotation = true,
                randomizeTilt = false,
                maxAssetTiltAngle = 0f,
                contentFlags = HideFlags.None,
                randomizeScale = false,
                minScale = Vector3.one,
                maxScale = Vector3.one,
                absolutRotation = false,
                offset = Vector3.zero,
            };
        }

    }
}
