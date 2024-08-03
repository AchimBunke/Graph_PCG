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