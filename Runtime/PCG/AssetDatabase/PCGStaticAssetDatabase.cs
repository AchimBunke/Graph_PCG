﻿/*
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

using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Achioto.Gamespace_PCG.Runtime.PCG.Database
{
    public class PCGStaticAssetDatabase : PCGAssetDatabase
    {
        [SerializeField] private List<Object> _assets;
        public List<Object> Assets { get => _assets; set { _assets = value; } }

        public override IEnumerable<string> GetAssetPaths()
        {
            if (_assets == null)
                return Enumerable.Empty<string>();
            return Assets.Select(a => AssetDatabase.GetAssetPath(a));
        }
    }

    [CustomEditor(typeof(PCGStaticAssetDatabase))]
    public class PCGStaticAssetDatabaseEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var pcgDatabase = (PCGStaticAssetDatabase)target;
            if (GUILayout.Button("Take From Directory"))
            {
                var directory = EditorUtility.OpenFolderPanel("Asset Folder", Application.dataPath, "");
                var files = Directory.GetFiles(directory);
                Undo.RecordObject(pcgDatabase, "Set PCG Database Assets from directory");
                pcgDatabase.Assets.Clear();
                foreach (var file in files)
                {
                    var relativeFilePath = "Assets" + file.Substring(Application.dataPath.Length);
                    var asset = AssetDatabase.LoadAssetAtPath(relativeFilePath, typeof(Object));
                    if (asset == null)
                        continue;
                    pcgDatabase.Assets.Add(asset);
                }
            }

        }
    }
}
