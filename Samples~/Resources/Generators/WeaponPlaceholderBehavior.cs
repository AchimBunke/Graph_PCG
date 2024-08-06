using Achioto.Gamespace_PCG.Runtime.Graph.PCG.Generator;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using UnityEditor;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
#if USE_HGRAPH_MODULES
    [PCGGraphModule]
#endif
    public class WeaponPlaceholderBehavior : HGraphGenerator
    {
        [SerializeField, PCGGraphAttribute(name: "WeaponType")] WeaponType _weaponType;
        [SerializeField] GameObject[] _availableWeapons;

        [SerializeField, ReadOnlyField] GameObject _instatiatedAsset;
        WeaponPlaceholder placeholderAlgorithm = new();

        protected override void Clear()
        {
            if (_instatiatedAsset != null)
                DestroyImmediate(_instatiatedAsset);
        }

        protected override void Generate()
        {
            //Undo.RecordObject(transform, "Generate Weapon");
            var instance = placeholderAlgorithm.CreateAssetInstance(transform);
            Undo.RegisterCreatedObjectUndo(instance, "Generate Weapon");
            Undo.RecordObject(this, "Generate Weapon");
            _instatiatedAsset = instance;
            EditorUtility.SetDirty(this);

        }

        protected override void PrepareGeneration()
        {
            Clear();
            var sceneNode = GetComponent<HGraphSceneNode>();
            if (sceneNode != null)
            {
                var pcgGraph = PCGGraphManager.Instance.PCGGraph;
                if (pcgGraph.Nodes.TryGetValue(sceneNode.HGraphId.Value, out var node))
                {
                    PCGGraphModuleManager.LoadGraphAttributesIntoMembers(this, node);
                }
            }

            placeholderAlgorithm.WeaponType = _weaponType;
            placeholderAlgorithm.AvailableWeapons = _availableWeapons;
        }

        public WeaponPlaceholderBehavior()
        {
            _generatorTypes = new string[]
            {
                 "assets",
                 "loot"
            };
        }
    }
}
