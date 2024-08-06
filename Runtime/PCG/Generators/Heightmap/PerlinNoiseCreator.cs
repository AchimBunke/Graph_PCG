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

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.Attributes;
using Random = UnityEngine.Random;
namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Heightmap
{
    public class PerlinNoiseCreator : NoiseCreator
    {
        [Serializable]
        public struct PerlinNoiseSettings
        {
            public float frequency;
            public float amplitude;
            public float persistence;
            public float lacunarity;
            [Min(1)]
            public int octaves;
            public bool fixedOffset;
            [ShowIfBool(nameof(fixedOffset))] public Vector2 offset;
            public static PerlinNoiseSettings Default => new PerlinNoiseSettings()
            {
                frequency = 1.0f,
                amplitude = 1.0f,
                persistence = 0.5f,
                lacunarity = 2.0f,
                octaves = 4,
                fixedOffset = false,
                offset = Vector2.zero,
            };
        }

        [SerializeField] PerlinNoiseSettings _settings = PerlinNoiseSettings.Default;

        public override float[,] GetNoise(int size, bool normalized, out float minValue, out float maxValue)
        {
            if (_noise == null || _noise.width != size)
                _noise = new Texture2D(size, size);
            Color[] colors = new Color[size * size];
            float[,] noiseMap = new float[size, size];
            float offsetX, offsetZ;
            if (_settings.fixedOffset)
            {
                offsetX = _settings.offset.x;
                offsetZ = _settings.offset.y;
            }
            else
            {
                offsetX = Random.Range(0f, 1000f);
                offsetZ = Random.Range(0f, 1000f);
            }
            float maxNoiseHeight = float.MinValue;
            float minNoiseHeight = float.MaxValue;
            // Creates noise between -1 and 1
            Parallel.For(0, size, x =>
            {
                for (int z = 0; z < size; ++z)
                {
                    var noise = GeneratePerlinNoise(x, z, offsetX, offsetZ, size);
                    noiseMap[x, z] = noise;
                    maxNoiseHeight = noise > maxNoiseHeight ? noise : maxNoiseHeight;
                    minNoiseHeight = noise < minNoiseHeight ? noise : minNoiseHeight;
                }
            });
            //Normalize and fill preview texture
            if (normalized)
            {
                Parallel.For(0, size, x =>
                {
                    for (int z = 0; z < size; ++z)
                    {
                        float normalizedNoise = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
                        noiseMap[x, z] = normalizedNoise;
                        colors[z * size + x] = new Color(normalizedNoise, normalizedNoise, normalizedNoise);
                    }
                });
            }
            else// normalize for texture
            {
                Parallel.For(0, size, x =>
                {
                    for (int z = 0; z < size; ++z)
                    {
                        float normalizedNoise = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, z]);
                        colors[z * size + x] = new Color(normalizedNoise, normalizedNoise, normalizedNoise);
                    }
                });
            }
            _noise.SetPixels(colors);
            _noise.Apply();
            minValue = minNoiseHeight;
            maxValue = maxNoiseHeight;
            return noiseMap;
        }

        float GeneratePerlinNoise(int x, int y, float offsetX, float offsetZ, int size)
        {
            float total = 0f;
            float frequency = _settings.frequency;
            float amplitude = _settings.amplitude;
            //float maxValue = 0f;
            float persistence = _settings.persistence;
            float lacunarity = _settings.lacunarity;

            for (int i = 0; i < _settings.octaves; i++)
            {
                float sampleX = (x + offsetX) * frequency / size;
                float sampleY = (y + offsetZ) * frequency / size;

                float perlinValue = (Mathf.PerlinNoise(sampleX, sampleY) * 2f) - 1;// [-1, 1]
                total += perlinValue * amplitude;

                //maxValue += amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }
            return total;
        }
    }
}
