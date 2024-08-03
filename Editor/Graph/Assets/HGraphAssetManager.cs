using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using UnityEditor;

namespace Achioto.Gamespace_PCG.Editor.Graph.Assets
{
    [InitializeOnLoad]
    public class HGraphAssetManager
    {
        static HGraphAssetManager()
        {
            _instance = new HGraphAssetManager();
            _instance.ReloadAllAssets();
        }
        private static HGraphAssetManager _instance;
        public static HGraphAssetManager Instance => _instance ?? (_instance = new HGraphAssetManager());

        public void ReloadAllAssets()
        {
            HGraphAssetRegistry.Instance.PrefabAssets.Clear();
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (HGraphAssetPostprocessor.IsPrefabAsset(path))
                {
                    if (HGraphAssetPostprocessor.TryGetHGraphNodeId(path, out var id))
                    {
                        HGraphAssetRegistry.Instance.PrefabAssets[AssetDatabase.AssetPathToGUID(path)] = id;
                    }
                }
            }
            string[] allAssets = AssetDatabase.GetAllAssetPaths();
            foreach (var assetPath in allAssets)
            {
                var userData = AssetImporter.GetAtPath(assetPath)?.userData;
                if (userData == null)
                    continue;
                
                if (HGraphSerializationController.TryDeserializeAssetData(userData, out var assetData))
                    HGraphAssetRegistry.Instance.RegisterAsset(AssetDatabase.AssetPathToGUID(assetPath), assetData);
            }
        }

     
    }
}
