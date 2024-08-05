using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Generators.Assets
{
    public abstract class AssetPlaceholder
    {
        public GameObject CreateAssetInstance(Transform parent)
        {
            var asset = GetAsset();
            if (asset == null)
            {
                Debug.LogWarning("Asset to instantiate is null.");
                return null;
            }
            if (PrefabUtility.IsPartOfPrefabAsset(asset))
                return (GameObject)PrefabUtility.InstantiatePrefab(asset, parent);
            else
                return GameObject.Instantiate(asset, parent);
        }
        public abstract GameObject GetAsset();
    }
}