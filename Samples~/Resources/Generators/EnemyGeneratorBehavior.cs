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