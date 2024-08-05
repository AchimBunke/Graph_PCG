using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Heightmap
{
    public abstract class NoiseCreator : MonoBehaviour
    {
        [SerializeField] protected Texture2D _noise;

        public abstract float[,] GetNoise(int size, bool normalized, out float minValue, out float maxValue);
    }
}
