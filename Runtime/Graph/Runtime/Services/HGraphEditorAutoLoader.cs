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

using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    [ExecuteInEditMode]
    public class HGraphEditorAutoLoader : MonoBehaviour
    {
        [SerializeField, ReadOnlyField] string graphPath;
        [SerializeField, Tooltip("Prevents unloading currently active graph")] bool keepCurrentGraph = false;
        [SerializeField, Tooltip("Loading will update existing values inside the graph")] bool updateData = true;
        private void OnEnable()
        {
            graphPath = HGraphUtility.CreateGraphFilePathFromScene(gameObject.scene);
            LoadGraph();
        }

        public void LoadGraph()
        {
            var data = HGraphSerializationController.DeserializeFromFile(graphPath);
            HGraphController.Load(data, keepCurrentGraph, updateData);
        }
    }
}