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
