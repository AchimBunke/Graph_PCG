using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System.Collections.Specialized;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Achioto.Gamespace_PCG.Editor.Graph
{
    /// <summary>
    /// Tracks selection of HGraph content.
    /// </summary>
    [InitializeOnLoad]
    public class HGraphSelectionEditor : UnityEditor.Editor
    {
        static HGraphSelectionEditor()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.sceneClosing += OnSceneClosing;
            Selection.selectionChanged += OnSelectionChanged;
        }

        private static void OnSceneClosing(Scene scene, bool removingScene)
        {
            // Clear RelationView of selected relations
            HGraphSceneRelationView.Instance.RelationViewSource.Clear();
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
        }

        /// <summary>
        /// Current Object selection.
        /// </summary>
        static GameObject[] currentSelection = new GameObject[] { };
        /// <summary>
        /// Editor Selection changed.
        /// </summary>
        private static void OnSelectionChanged()
        {
            // cleanup old selection
            foreach (var go in currentSelection.Except(Selection.gameObjects))
            {
                // destroyed since last selection
                if (go == null)
                    continue;
                // Handle HGraphNode selection
                if (go.TryGetComponent<HGraphSceneNode>(out var hNode))
                {
                    if (hNode.NodeData.Value == null)
                        continue;
                    // Remove Relations of this node from the RelationView
                    foreach (var relation in hNode.NodeData.Value.Relations)
                    {
                        if (HGraph.Instance.SceneRelations.TryGetValue(relation, out var sceneRelation))
                        {
                            HGraphSceneRelationView.Instance.RelationViewSource.Remove(sceneRelation);
                        }
                    }
                    // Unsubscribe from Relation changes of this old node.
                    //hNode.NodeData.CollectionChanged -= Relations_CollectionChanged;
                }
            }
            // handle new selection
            foreach (var go in Selection.gameObjects.Except(currentSelection))
            {
                // Handle HGraphNode selection
                if (go.TryGetComponent<HGraphSceneNode>(out var hNode))
                {
                    if (hNode.NodeData.Value == null)
                        continue;
                    // Add Relations of this node to the RelationView
                    foreach (var relation in hNode.NodeData.Value.Relations)
                    {
                        if (HGraph.Instance.SceneRelations.TryGetValue(relation, out var sceneRelation))
                        {
                            HGraphSceneRelationView.Instance.RelationViewSource.Add(sceneRelation);
                        }
                    }
                    // Subscribe to Relation changes of this selected node.
                    //hNode.Relations.CollectionChanged += Relations_CollectionChanged;
                }
            }
            currentSelection = Selection.gameObjects.Where(g => g.TryGetComponent<HGraphSceneNode>(out _)).ToArray();
        }

        /// <summary>
        /// Handle Relation changes inside nodes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void Relations_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (HGraphSceneRelation p in e.OldItems)
                {
                    HGraphSceneRelationView.Instance.RelationViewSource.Remove(p);
                }
            }
            if (e.NewItems != null)
            {
                foreach (HGraphSceneRelation p in e.NewItems)
                {
                    HGraphSceneRelationView.Instance.RelationViewSource.Add(p);
                }
            }
        }
    }
}
