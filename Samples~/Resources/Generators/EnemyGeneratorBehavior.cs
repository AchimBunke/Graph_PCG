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
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    public class EnemyGeneratorBehavior : HGraphGenerator
    {
        [SerializeField] List<Enemy> _enemyAssets;
        [SerializeField] List<Transform> _spawnPositions;
        [SerializeField] DifficultyDistributionGenerator _difficultyDistributionGenerator;

        private EnemyGenerator _enemyGenerator = new();
        [SerializeField, ReadOnlyField]
        private List<Enemy> _spawnedEnemies = new();
        protected override void Clear()
        {
            foreach (Enemy enemy in _spawnedEnemies)
            {
                if (enemy == null)
                    continue;
                DestroyImmediate(enemy.gameObject);
            }
            _spawnedEnemies.Clear();
        }

        protected override void Generate()
        {

            if (_difficultyDistributionGenerator.DifficultyDistribution == null || !_difficultyDistributionGenerator.DifficultyDistribution.HasResult)
            {
                _difficultyDistributionGenerator.Generate(HGraphGenerationCallerType.Data);
                if (_difficultyDistributionGenerator.DifficultyDistribution == null || !_difficultyDistributionGenerator.DifficultyDistribution.HasResult)
                    return;
            }

            _enemyGenerator.DifficultyDistribution = _difficultyDistributionGenerator.DifficultyDistribution;
            var spawnedEnemies = _enemyGenerator.SpawnEnemies();
            foreach (var enemy in spawnedEnemies)
            {
                Undo.RegisterCreatedObjectUndo(enemy.gameObject, "Spawn Enemy");
            }
            Undo.RecordObject(this, "Spawn Enemies");
            _spawnedEnemies = spawnedEnemies;
            EditorUtility.SetDirty(this);

        }

        protected override void PrepareGeneration()
        {
            Clear();
            _enemyGenerator.Enemies = _enemyAssets;
            _enemyGenerator.SpawnPositions = _spawnPositions;
        }
        public EnemyGeneratorBehavior()
        {
            _generatorTypes = new string[]
            {
            "enemies"
            };
        }
    }
}