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
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.Attributes;
using static Achioto.Gamespace_PCG.Runtime.PCG.Generators.Heightmap.GaussianRBFHeightMapCreator;
using Color = UnityEngine.Color;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Heightmap
{
    public class HeightMapGenerator : HGraphGenerator
    {
        [SerializeField] GaussianRBFHeightMapCreatorSettings _settings = GaussianRBFHeightMapCreatorSettings.Default;
        [SerializeField, ReadOnlyField] Texture2D _heightMapPreview;
        private float[,] _heightMap;
        [SerializeField] int _size = 512;
        [SerializeField] int _height = 512;
        [SerializeField] int _generationResolution = 513;

        [Serializable]
        struct ScaledNoise
        {
            public float scale;
            public NoiseCreator noise;
        }

        [SerializeField] ScaledNoise[] _noises;


        GaussianRBFHeightMapCreator _heightMapCreater;
        Terrain _terrain;
        void ApplyHeightmap(float[,] heightmap)
        {
            // Apply the heightmap to a plane or terrain
            //Renderer renderer = GetComponent<Renderer>();
            //if (renderer != null)
            //{
            //    renderer.material.mainTexture = heightmap;
            //}
            Color[] colors = new Color[_generationResolution * _generationResolution];
            Parallel.For(0, _generationResolution, x =>
            {
                for (int z = 0; z < _generationResolution; z++)
                {
                    float height = heightmap[x, z];
                    colors[x * _generationResolution + z] = new Color(height, height, height);
                }
            });
            _heightMapPreview.SetPixels(colors);
            _heightMapPreview.Apply();
            //Alternatively, apply the heightmap to a terrain
            if (_terrain != null)
            {
                var terrainSize = _terrain.terrainData.size;
                _terrain.terrainData.heightmapResolution = _generationResolution;
                _terrain.terrainData.size = terrainSize;
                _terrain.terrainData.SetHeights(0, 0, heightmap);
            }
        }

        protected override void PrepareGeneration()
        {
            _heightMapCreater = new GaussianRBFHeightMapCreator();
            var sceneNode = GetComponent<HGraphSceneNode>();
            if (sceneNode != null)
            {
                var graph = PCGGraphManager.Instance.PCGGraph;
                if (graph.Nodes.TryGetValue(sceneNode.HGraphId.Value, out var nodeData))
                    _settings = (GaussianRBFHeightMapCreatorSettings)PCGGraphModuleManager.LoadGraphAttributesIntoMembers(_settings, nodeData);
            }
            _heightMapCreater.Settings = _settings;
            _terrain = GetComponent<Terrain>();
        }

        protected override void Generate()
        {
            if (_heightMap == null || _heightMap.GetLength(0) != _generationResolution)
            {
                //_heightMap = new float[_generationResolution, _generationResolution];
                _heightMapPreview = new Texture2D(_generationResolution, _generationResolution);
            }
            var pcgGraph = PCGGraphManager.Instance.PCGGraph;
            var controlPoints = pcgGraph.Nodes.Values
                .Where(n => n.spaceData != null)
                .Where(n =>
                {
                    var heightmapData = PCGGraphModuleManager.LoadGraphAttributesIntoMembers(new HeightMapAttributesModule(), n);
                    return heightmapData.CarveHeightmaps;
                })
                .Select(n => _terrain.transform.InverseTransformPoint(n.spaceData.nodePosition)).ToArray();
            _heightMap = _heightMapCreater.GenerateHeightmap(controlPoints, _generationResolution, _size, _height);

            if (_noises != null)
            {
                float totalScale = 1;
                foreach (var n in _noises)
                {
                    totalScale += n.scale;
                    AddNoise(n.noise.GetNoise(_generationResolution, false, out _, out _), n.scale);
                }
                Normalize(totalScale);
            }
            ApplyHeightmap(_heightMap);
        }
        private void AddNoise(float[,] noise, float scale)
        {
            Parallel.For(0, _generationResolution, x =>
            {
                for (int z = 0; z < _generationResolution; z++)
                {
                    int index = x * _generationResolution + z;

                    // Get the height and noise values
                    float heightValue = _heightMap[x, z];
                    float noiseValue = noise[x, z] / 2f * scale;

                    float newHeightValue = heightValue + noiseValue;

                    // Update the pixel value
                    _heightMap[x, z] = newHeightValue;
                }
            });
        }
        private void Normalize(float totalWeight)
        {
            Parallel.For(0, _generationResolution, x =>
            {
                for (int z = 0; z < _generationResolution; ++z)
                {
                    int index = z * _generationResolution + x;
                    float noiseValue = Mathf.Clamp01(_heightMap[x, z] / totalWeight);
                    _heightMap[x, z] = noiseValue;
                }
            });
        }

        protected override void Clear()
        {
            //Terrain terrain = GetComponent<Terrain>();
        }

        public HeightMapGenerator()
        {
            _generatorTypes = new string[]
            {
            "terrain"
            };
        }

    }

#if USE_HGRAPH_MODULES
    [PCGGraphModule]
#endif
    public class HeightMapAttributesModule
    {
        [PCGGraphAttribute]
        public bool CarveHeightmaps { get; set; } = false;
    }

}