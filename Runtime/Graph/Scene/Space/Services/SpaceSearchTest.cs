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

using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services
{
    [ExecuteInEditMode]
    public class SpaceSearchTest : MonoBehaviour
    {
        public HGraphSpaceSearchSettings SearchSettings = HGraphSpaceSearchSettings.Default;
        public HGraphSceneNode[] FoundNodes;
        public void Run()
        {
            var search = new HGraphSpaceSearch(SearchSettings);
            var result = search.FindNearbyNodes(GetComponent<HGraphSceneNode>().NodeData.Value);
            FoundNodes = result.Nodes.Select(f => f.SceneNode.Value).ToArray();
        }

    }
    [CustomEditor(typeof(SpaceSearchTest))]
    public class SpaceSearchTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Run"))
            {
                var t = (SpaceSearchTest)target;
                t.Run();
            }
        }
    }
}
