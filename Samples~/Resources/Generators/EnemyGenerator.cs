using System.Collections.Generic;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    public class EnemyGenerator
    {
        public List<Enemy> Enemies { get; set; }
        public List<Transform> SpawnPositions { get; set; }
        public DifficultyDistribution DifficultyDistribution { get; set; }


        public List<Enemy> SpawnEnemies()
        {
            List<Enemy> instances = new List<Enemy>();
            foreach (var p in SpawnPositions)
            {
                var randomEnemy = Enemies[Random.Range(0, Enemies.Count)];
                var spawnedEnemy = GameObject.Instantiate(randomEnemy, p.position, p.rotation, p);

                var difficultyLevel = DifficultyDistribution.FindDifficultyForPosition(new Vector2(p.position.x, p.position.z));

                spawnedEnemy.Health = difficultyLevel * 10f;
                spawnedEnemy.DifficultyLevel = difficultyLevel;
                instances.Add(spawnedEnemy);
            }
            return instances;
        }

    }
}
