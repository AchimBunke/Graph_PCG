using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator
{
    public class ManualGenerator : HGraphGenerator
    {
        [SerializeField] string _seed = string.Empty;

        private void Awake()
        {
            _allowAutoInvocation = false;
            _allowHierarchicalInvocation = false;
        }
        protected override void Clear()
        {
        }

        protected override void Generate()
        {
        }

        protected override void PrepareGeneration()
        {
            if (!string.IsNullOrEmpty(_seed))
                Random.InitState(_seed.GetHashCode());
        }
    }
}
