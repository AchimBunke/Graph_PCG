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

using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using UnityEditor;
using UnityEngine;


namespace Achioto.Gamespace_PCG.Editor.Graph.Assets
{
    public class HGraphAssetPostprocessor : AssetPostprocessor
    {
        public static bool IsPrefabAsset(string path) => path.EndsWith(".prefab");
        public static bool TryGetHGraphNodeId(string path, out string id)
        {
            var go = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject));
            var sceneNode = go != null ? GetComponent<HGraphSceneNode>(go) : null;
            if (sceneNode != null && HGraphResources.IsHGraphIdValid(sceneNode.HGraphId.Value))
            {
                id = sceneNode.HGraphId.Value;
                return true;
            }
            id = default;
            return false;
        }
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {
            foreach (string path in importedAssets)
            {
                if (IsPrefabAsset(path))
                {
                    if (TryGetHGraphNodeId(path, out var id))
                    {
                        HGraphAssetRegistry.Instance.PrefabAssets[AssetDatabase.AssetPathToGUID(path)] = id;
                    }
                }

                var importer = AssetImporter.GetAtPath(path);
                if (HGraphSerializationController.TryDeserializeAssetData(importer.userData, out var assetData))
                {
                    HGraphAssetRegistry.Instance.Assets[AssetDatabase.AssetPathToGUID(path)] = assetData;
                }

            }
            foreach (string str in deletedAssets)
            {
                //Debug.Log("Deleted Asset: " + str);
            }

            for (int i = 0; i < movedAssets.Length; i++)
            {
                // Debug.Log("Moved Asset: " + movedAssets[i] + " from: " + movedFromAssetPaths[i]);
            }

            if (didDomainReload)
            {
                //Debug.Log("Domain has been reloaded");
            }
        }

        void OnPostprocessPrefab(GameObject root)
        {
            HGraphSceneNode sceneNode = root.GetComponent<HGraphSceneNode>();
            if (sceneNode != null)
            {
                if (HGraphController.TryFindNode(sceneNode.HGraphId.Value, out var node))
                {
                    SetUserData(node, AssetDatabase.GetAssetPath(root) ?? string.Empty);
                }
            }
        }
        static void ReadUserData(string asset)
        {
            Debug.Log("Read data for imported Asset: " + asset);
            var importer = AssetImporter.GetAtPath(asset);
            Debug.Log("Importer: " + importer.name);
            Debug.Log("UserData: " + importer.userData);
        }
        void SetUserData(HGraphNode node, string assetGUID)
        {
            var assetData = HGraphSerializationController.CreateAssetDataFromNode(node);
            assetImporter.userData = HGraphSerializationController.Serialize(assetData);
        }

        /// <summary>
        /// From Visual Scripting API
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="o"></param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException"></exception>
        private static T GetComponent<T>(UnityEngine.Object o)
        {
            if (o is GameObject)
            {
                return ((GameObject)o).GetComponent<T>();
            }
            else if (o is Component)
            {
                return ((Component)o).GetComponent<T>();
            }
            else
            {
                throw new NotSupportedException();
            }
        }
    }
}

