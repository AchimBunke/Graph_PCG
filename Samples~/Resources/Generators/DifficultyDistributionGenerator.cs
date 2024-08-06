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
using System.Linq;
using UnityEngine;
using UnityUtilities.UnityBase;

namespace Achioto.Gamespace_PCG.Samples.Resources
{

    public class DifficultyDistributionGenerator : HGraphGenerator
    {
        [SerializeField] Bounds _bounds;
        [SerializeField, Min(0)] float _resolution = 1f;
        [SerializeField] DifficultyDistribution.InterpolationType _interpolationType;
        [SerializeField] bool _debug = false;
        [SerializeField] float _debugHeight = 100f;
        [SerializeField] Terrain _terrain;


        private DifficultyDistribution difficultyDistribution;
        public DifficultyDistribution DifficultyDistribution => difficultyDistribution;
        private float[,] _difficultyMap;
        Vector2[] _positions;
        Vector3[] _difficulties;
        float _minDifficulty;
        float _maxDifficulty;
        protected override void Clear()
        {
            _positions = null;
            _difficulties = null;
            if (_terrain != null)
            {
                ClearAlphaMap(_terrain);
            }
        }
        private void ClearAlphaMap(Terrain terrain)
        {
            // Get the terrain data
            TerrainData terrainData = terrain.terrainData;

            // Get the alpha map dimensions
            int alphaMapWidth = terrainData.alphamapWidth;
            int alphaMapHeight = terrainData.alphamapHeight;
            int alphaMapLayers = terrainData.alphamapLayers;

            // Create a new alpha map array
            float[,,] newAlphaMap = new float[alphaMapHeight, alphaMapWidth, alphaMapLayers];

            // Set the first texture layer to 1 and the rest to 0
            for (int y = 0; y < alphaMapHeight; y++)
            {
                for (int x = 0; x < alphaMapWidth; x++)
                {
                    newAlphaMap[y, x, 0] = 1.0f; // Set the default texture layer
                    for (int layer = 1; layer < alphaMapLayers; layer++)
                    {
                        newAlphaMap[y, x, layer] = 0.0f; // Set all other layers to 0
                    }
                }
            }

            // Apply the new alpha map to the terrain
            terrainData.SetAlphamaps(0, 0, newAlphaMap);

            Debug.Log("Terrain alpha map cleared and default texture set!");
        }

        private void OnDrawGizmos()
        {
            if (!_debug)
                return;
            if (_difficulties != null && _terrain == null)
            {
                foreach (var p in _difficulties)
                {
                    Gizmos.color = Color.Lerp(Color.blue, Color.red, Mathf.InverseLerp(_minDifficulty, _maxDifficulty, p.y));
                    Gizmos.DrawCube(new Vector3(p.x, _debugHeight, p.z), Vector3.one * _resolution / 2f);
                }
            }
        }

        protected override void Generate()
        {
            if (_resolution == 0f)
                return;
            var pcgGraph = PCGGraphManager.Instance.PCGGraph;
            var difficultyModule = new DifficultyModule();
            var category = PCGGraphModuleManager.GetMemberAttributeName(difficultyModule, nameof(DifficultyModule.Difficulty));
            difficultyDistribution.DifficultyPositions = pcgGraph.Nodes.Values
                .Where(n => n.attributes.Any(a => a.category == category))
                .Select(n =>
                {
                    PCGGraphModuleManager.LoadGraphAttributesIntoMembers(difficultyModule, n);
                    return new DifficultyDistribution.DifficultyPositionPair()
                    {
                        Position = n.spaceData.nodePosition.ToVector2XZ(),
                        Difficulty = difficultyModule.Difficulty
                    };
                }).ToArray();
            InitializeDifficultyMap();
            _positions = GetPositions();
            difficultyDistribution.Triangulate();
            var difficulties = difficultyDistribution.FindDifficultiesForPositions(_positions);
            UpdateDifficulties(difficulties);
            if (_debug)
                PaintTerrain();

        }
        private void InitializeDifficultyMap()
        {
            int width = Mathf.CeilToInt(_bounds.size.x / _resolution);
            int depth = Mathf.CeilToInt(_bounds.size.z / _resolution);
            _difficultyMap = new float[depth, width];
            for (int z = 0; z < _difficultyMap.GetLength(0); ++z)
            {
                for (int x = 0; x < _difficultyMap.GetLength(1); ++x)
                {
                    _difficultyMap[z, x] = 0f;
                }
            }
        }
        private Vector2[] GetPositions()
        {
            Vector2[] positions = new Vector2[_difficultyMap.GetLength(0) * _difficultyMap.GetLength(1)];
            int index = 0;
            for (int z = 0; z < _difficultyMap.GetLength(0); ++z)
            {
                for (int x = 0; x < _difficultyMap.GetLength(1); ++x)
                {
                    positions[index] = new Vector2(x * _resolution + _bounds.min.x, z * _resolution + _bounds.min.z);
                    ++index;
                }
            }
            return positions;
        }
        private void UpdateDifficulties(float[] difficulties)
        {
            _difficulties = new Vector3[_difficultyMap.GetLength(0) * _difficultyMap.GetLength(1)];

            for (int i = 0; i < difficulties.Length; ++i)
            {
                var pos = _positions[i];
                var difficulty = difficulties[i];
                if (difficulty > _maxDifficulty)
                    _maxDifficulty = difficulty;
                if (difficulty < _minDifficulty)
                    _minDifficulty = difficulty;
                _difficulties[i] = new Vector3(pos.x, difficulty, pos.y);
                ++i;

            }
        }

        protected override void PrepareGeneration()
        {
            Clear();
            if (difficultyDistribution == null)
                difficultyDistribution = new DifficultyDistribution();
            difficultyDistribution.Type = _interpolationType;
        }

        private void PaintTerrain()
        {
            if (_terrain == null)
                return;

            TerrainData terrainData = _terrain.terrainData;
            int terrainWidth = terrainData.alphamapWidth;
            int terrainHeight = terrainData.alphamapHeight;

            // Get the terrain's splat map
            float[,,] splatmapData = terrainData.GetAlphamaps(0, 0, terrainWidth, terrainHeight);

            foreach (Vector3 difficulty in _difficulties)
            {
                // Convert world position to terrain position
                float terrainX = difficulty.x / terrainData.size.x * terrainWidth;
                float terrainZ = difficulty.z / terrainData.size.z * terrainWidth;

                // Calculate the corresponding position in the splat map
                int mapX = Mathf.FloorToInt(terrainX);
                int mapZ = Mathf.FloorToInt(terrainZ);

                //var color = Color.Lerp(Color.green, Color.red, );
                var alpha = Mathf.InverseLerp(_minDifficulty, _maxDifficulty, difficulty.y);

                splatmapData[mapZ, mapX, 1] = alpha;
                // Apply the color to the splat map
                //for (int i = 0; i < terrainData.terrainLayers.Length; i++)
                //{

                //}
            }

            // Set the updated splat map to the terrain
            terrainData.SetAlphamaps(0, 0, splatmapData);
        }

        public float GetDifficultyForPosition(Vector3 position)
        {
            if (difficultyDistribution == null)
                return -1;
            return difficultyDistribution.FindDifficultyForPosition(new Vector2(position.x, position.z));
        }

        public DifficultyDistributionGenerator()
        {
            _generatorTypes = new string[]
            {
            "enemies"
            };
        }
    }

#if USE_HGRAPH_MODULES
    [PCGGraphModule]
#endif
    public class DifficultyModule
    {
        [PCGGraphAttribute]
        public float Difficulty { get; set; }
    }

}