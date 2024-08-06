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
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using UniRx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;


namespace Achioto.Gamespace_PCG.Editor.Graph
{
    /// <summary>
    /// Handles automatic creation of relations depending on the HGraph.
    /// </summary>
    [InitializeOnLoad]
    public class HGraphSceneRelationCreatorEditor : UnityEditor.Editor
    {
        static HGraphSceneRelationCreatorEditor()
        {
            HGraph.Instance.Relations.ObserveAdd().Subscribe(OnAdd);
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            foreach (var relation in HGraph.Instance.Relations)
            {
                if (HGraph.Instance.SceneRelations.ContainsKey(relation.Key))
                    continue;
                CreateNewSceneRelation(relation.Value);
            }
        }

        static void OnAdd(DictionaryAddEvent<string, HGraphRelation> evt)
        {
            if (HGraph.Instance.SceneRelations.ContainsKey(evt.Value.HGraphId.Value))
                return;
            CreateNewSceneRelation(evt.Value);
        }
        // TODO Evaluate if should delete
        static void OnReplace(DictionaryReplaceEvent<string, HGraphRelation> evt)
        {
            //Handled in SceneRelation
        }
        static void OnRemove(DictionaryRemoveEvent<string, HGraphRelation> evt)
        {
            //var old = HGraph.Instance.SceneRelations.Remove(evt.Value.HGraphId.Value);
        }
        static void OnReset(Unit _)
        {
            //HGraph.Instance.SceneRelations.
        }
        static HGraphSceneRelation GetRelationPrefab()
        {
            return HGraphSettings.GetOrCreateSettings().DefaultRelation;
        }
        static HGraphSceneRelation CreateNewSceneRelation(HGraphRelation relation)
        {
            var root = HGraph.Instance.SceneRoot;
            var sceneRelation = PrefabUtility.InstantiatePrefab(GetRelationPrefab(), root.transform) as HGraphSceneRelation;
            sceneRelation.HGraphId.Value = relation.HGraphId.Value;
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            return sceneRelation;
        }
    }

}