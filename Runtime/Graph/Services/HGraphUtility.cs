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
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    public static class HGraphUtility
    {
        public static bool AutoConnectSuperNode { get; set; } = true;

        public static HGraphNode FindSuperNode(HGraphSceneNode node)
            => FindSuperSpace(node)?.SceneNode?.NodeData.Value;
        public static HGraphNodeSpace FindSuperSpace(HGraphSceneNode node)
        {
            var nodeSpace = node.GetComponent<HGraphNodeSpace>();
            if(nodeSpace == null || nodeSpace != null)// maybe change space search if searching for parent space of a space and not of a position
            {
                var superSpace = HGraphSpaceUtility.FindSpace(node);
                return superSpace;
            }
            return null;
        }

        public static string CreateGraphFilePathFromCurrentEditorScene()
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            var scenePath = EditorSceneManager.GetActiveScene().path;
            string sceneFullPath = Path.Combine(Application.dataPath, "..", scenePath);
            string sceneDirectory = Path.GetDirectoryName(sceneFullPath);
            return Path.Combine(sceneDirectory, Path.ChangeExtension(sceneName, "graph.json"));
        }
        public static string CreateGraphFilePathFromScene(UnityEngine.SceneManagement.Scene scene)
        {
            var sceneName = scene.name;
            var scenePath = scene.path;
            string sceneFullPath = Path.Combine(Application.dataPath, "..", scenePath);
            string sceneDirectory = Path.GetDirectoryName(sceneFullPath);
            return Path.Combine(sceneDirectory, Path.ChangeExtension(sceneName, "graph.json"));
        }

    }
}
