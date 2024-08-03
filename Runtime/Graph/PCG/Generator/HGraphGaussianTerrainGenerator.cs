using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator
{
    public class HGraphGaussianTerrainGenerator : HGraphGenerator
    {
        private bool _autoGenerateOnGraphChanged_check;
        private float _autoGenerateThrottleTime_check;
        [SerializeField] float _autoGenerateThrottleTime = 2;
        [SerializeField] string _heightAttributeName = "Height";
        [SerializeField] float _spread = 50f;
        [SerializeField] Terrain terrain;
        [SerializeField] float _worldHeight = 100f;
        [SerializeField] Color _lowColor = Color.blue;
        [SerializeField] Color _highColor = Color.magenta;

        HGraphAttributeFilterView _filteredNodeView;
        HGraphAttributeFilterView FilteredNodeView
        {
            get
            {
                if (_filteredNodeView == null)
                    _filteredNodeView = CreateNodeView();
                return _filteredNodeView;
            }
        }
        private HGraphAttributeFilterView CreateNodeView()
        {
            HGraphAttributeFilter settings = new HGraphAttributeFilter()
            {
                Categories = new HashSet<string>(){
                "Height"
            }
            };
            return new HGraphAttributeFilterView(settings);
        }
        float[,,] terrainAlphaMap;

        private IDisposable _graphChangedDisposable;
        protected override void Generate()
        {
            Debug.Log("Generate Gaussian Terrain", this);
            if (terrain == null)
            {
                Debug.LogWarning("No Terrain context!", this);
                return;
            }
            if (_filteredNodeView.FilteredNodes.Count() == 0)
            {
                Debug.LogWarning("No Nodes provided!", this);
                return;
            }
            var settings = new GaussianSettings()
            {
                AttributeCategoryName = _heightAttributeName,
                Spread = _spread,
                WorldHeight = _worldHeight
            };
            var terrainData = terrain.terrainData;
            var terrainPosition = terrain.transform.position;
            var heightmapResolution = new Vector2(terrain.terrainData.heightmapResolution, terrain.terrainData.heightmapResolution);
            var terrainSize = terrain.terrainData.size;

            var heightMap = GaussianTerrain.CreateHeightMap(settings, terrainData, (x, y) =>
            {
                float xNormalized = x / (heightmapResolution.x - 1);
                float yNormalized = y / (heightmapResolution.y - 1);

                // Calculate world position based on pre-cached values
                float xWorld = terrainPosition.x + xNormalized * terrainSize.x;
                float zWorld = terrainPosition.z + yNormalized * terrainSize.z;

                return new Vector2(xWorld, zWorld);
            });
            terrainData.SetHeights(0, 0, heightMap);

        }

        private void Awake()
        {

            _autoGenerateOnGraphChanged_check = AllowAutoInvocation;
            _autoGenerateThrottleTime_check = _autoGenerateThrottleTime;
            ResetViewChangedListener();
        }
        private void ResetViewChangedListener()
        {
            _graphChangedDisposable?.Dispose();
            if (AllowAutoInvocation)
            {
                _graphChangedDisposable =
                    FilteredNodeView.NodesChanged.Select(_ => UniRx.Unit.Default)
                    .Merge(FilteredNodeView.AttributeChanged.Select(_ => UniRx.Unit.Default))
                    .Throttle(TimeSpan.FromSeconds(_autoGenerateThrottleTime))
                    .Subscribe(_ => Generate());
            }
        }

        private void OnValidate()
        {
            if (_autoGenerateOnGraphChanged_check != AllowAutoInvocation)
            {
                _autoGenerateOnGraphChanged_check = AllowAutoInvocation;
                ResetViewChangedListener();
            }
            if (_autoGenerateThrottleTime_check != _autoGenerateThrottleTime)
            {
                _autoGenerateThrottleTime_check = _autoGenerateThrottleTime;
                if (AllowAutoInvocation)
                {
                    ResetViewChangedListener();
                }
            }

        }
        private void OnDestroy()
        {
            _filteredNodeView?.Dispose();
        }

        protected override void PrepareGeneration()
        {
        }

        protected override void Clear()
        {

        }
    }

    public struct GaussianSettings
    {
        public float Spread;
        public string AttributeCategoryName;
        public float WorldHeight;
    }
    public static class GaussianTerrain
    {
        struct GaussianData
        {
            public Vector3[] Heights;
            public Func<int, int, Vector2> GetWorldPosition;
            public float MaxHeight;
            public float MinHeight;
        }
        public static float[,] CreateHeightMap(GaussianSettings settings, TerrainData terrainData, Func<int, int, Vector2> GetWorldPosition)
        {
            float[,] heightMap = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            var data = new GaussianData()
            {
                Heights = HGraph.Instance.Nodes.Values
                    .Where(n => n.Attributes.ContainsKey(settings.AttributeCategoryName) && n.SceneNode.Value != null)
                    .Select(n =>
                    {
                        var pos = n.SceneNode.Value.transform.position;
                        return new Vector3(pos.x, (n.Attributes[settings.AttributeCategoryName].Data as FloatHGraphAttribute)?.Value ?? 0, pos.z);
                    }).ToArray(),
                GetWorldPosition = GetWorldPosition
            };
            data.MaxHeight = settings.WorldHeight;
            data.MinHeight = 0;

            FillHeightMap(settings, data, ref heightMap);
            NormalizeHeightMap(data, ref heightMap);
            return heightMap;
        }
        private static void FillHeightMap(GaussianSettings settings, GaussianData data, ref float[,] heightMap)
        {
            for (int x = 0; x < heightMap.GetLength(0); ++x)
            {
                for (int y = 0; y < heightMap.GetLength(1); ++y)
                {
                    var currentPos = data.GetWorldPosition(y, x);
                    for (int i = 0; i < data.Heights.Length; ++i)
                    {
                        Vector3 point = data.Heights[i];
                        float distanceX = currentPos.x - point.x;
                        float distanceZ = currentPos.y - point.z;
                        float gaussianHeight = Mathf.Exp(-(distanceX * distanceX + distanceZ * distanceZ) / (2 * settings.Spread * settings.Spread));
                        heightMap[x, y] += gaussianHeight * point.y;
                    }

                }
            }
        }
        private static void NormalizeHeightMap(GaussianData data, ref float[,] heightMap)
        {
            for (int x = 0; x < heightMap.GetLength(0); ++x)
            {
                for (int y = 0; y < heightMap.GetLength(1); ++y)
                {
                    heightMap[x, y] /= data.MaxHeight;
                }
            }
        }
    }

}