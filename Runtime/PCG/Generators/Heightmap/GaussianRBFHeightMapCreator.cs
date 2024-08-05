using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityUtilities.UnityBase;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Heightmap
{
    public class GaussianRBFHeightMapCreator
    {

        [Serializable]
#if USE_HGRAPH_MODULES
        [PCGGraphModule(registerAllMembers: true)]
#endif
        public struct GaussianRBFHeightMapCreatorSettings
        {
            public float epsilon;
            public static GaussianRBFHeightMapCreatorSettings Default => new GaussianRBFHeightMapCreatorSettings()
            {
                epsilon = 10f
            };
        }

        public GaussianRBFHeightMapCreatorSettings Settings { get; set; }
        public GaussianRBFHeightMapCreator()
        {
            Settings = GaussianRBFHeightMapCreatorSettings.Default;
        }

        public float[,] GenerateHeightmap(Vector3[] controlPoints, int resolution, int size, int height)
        {
            //Texture2D heightmap = new Texture2D(width, height);

            // Calculate the heightmap values using RBF interpolation
            //Color[] pixels = new Color[resolution * resolution];
            float[,] heights = new float[resolution, resolution];
            float scale = (float)resolution / size;
            Parallel.For(0, resolution, x =>
            {
                for (int y = 0; y < resolution; ++y)
                {
                    float value = GaussianRBFInterpolation(x / scale, y / scale, height, controlPoints);
                    heights[x, y] = value;
                }
            });
            return heights;
            //return heightmap;
        }
        float GaussianRBFInterpolation(float x, float y, int height, Vector3[] controlPoints)
        {
            float sum = 0.0f;
            float sumWeights = 0.0f;

            for (int i = 0; i < controlPoints.Count(); i++)
            {
                float distance = Vector2.Distance(new Vector2(y, x), controlPoints[i].ToVector2XZ());
                float weight = Mathf.Exp(-Mathf.Pow(distance / Settings.epsilon, 2)); // Gaussian function

                sum += weight * Mathf.Clamp01(controlPoints[i].y / height);
                sumWeights += weight;
            }

            return sumWeights == 0 ? 0 : sum / sumWeights;
        }
        private int GetPixelIndex(int x, int y, int resolution) => y * resolution + x;
    }
}