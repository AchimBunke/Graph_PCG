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
