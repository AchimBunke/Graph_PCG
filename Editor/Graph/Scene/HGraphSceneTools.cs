using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using ALib.Extensions;
using System;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.Overlays;
using UnityEditor.Toolbars;
using UnityEngine.UIElements;


namespace Achioto.Gamespace_PCG.Editor.Graph
{
    /// <summary>
    /// Scene Editor Toolbar overlay
    /// </summary>
    [Overlay(typeof(SceneView), "HGraph Tools", false)]
    class HGraphSceneTools : ToolbarOverlay
    {
        HGraphSceneTools() : base(
            CreateRelation.id,
            ShowAllRelations.id,
            AutoConnectSuperNode.id)
        { }
    }

    /// <summary>
    /// Provides Creation button between 2 selected HGraphNodes
    /// </summary>
    [EditorToolbarElement(id, typeof(SceneView))]
    class CreateRelation : EditorToolbarButton, IAccessContainerWindow
    {
        public const string id = "HGraphTools/CreateRelation";

        public EditorWindow containerWindow { get; set; }

        public CreateRelation()
        {
            // A toolbar element can be either text, icon, or a combination of the two. Keep in mind that if a toolbar is
            // docked horizontally the text will be clipped, so usually it's a good idea to specify an icon.
            text = "Create Relation";
            //icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CreateCubesIcon.png");

            tooltip = "Create Relation between the 2 connected nodes.";
            clicked += OnClick;
            Selection.selectionChanged += OnSelectionChanged;
        }
        private HGraphSceneNode _selectedSource;
        private HGraphSceneNode _selectedTarget;
        private void OnSelectionChanged()
        {
            var selection = Selection.gameObjects;
            if (selection.Length == 2 &&
                selection.All(s => s.GetComponent<HGraphSceneNode>() != null))
            {
                HGraphSceneNode source = selection[0].GetComponent<HGraphSceneNode>();
                HGraphSceneNode target = selection[1].GetComponent<HGraphSceneNode>();
                if (source.IsRegistered.Value && source.NodeData.Value != null
                    && target.IsRegistered.Value && target.NodeData.Value != null
                    && !source.NodeData.Value.HasRelationTo(target.NodeData.Value))
                {
                    _selectedSource = source;
                    _selectedTarget = target;
                    style.display = DisplayStyle.Flex;
                    return;
                }
                style.display = DisplayStyle.None;
            }
            else
            {
                style.display = DisplayStyle.None;
            }
        }

        void OnClick()
        {
            if (_selectedSource.NodeData.Value.Relations.Intersect(_selectedTarget.NodeData.Value.Relations).Any())
                return;
            var relation = HGraphRelation.Construct(HGraphResources.CreateRelationId(_selectedSource.NodeData.Value, _selectedTarget.NodeData.Value),
                _selectedSource.HGraphId.Value,
                _selectedTarget.HGraphId.Value);
            HGraph.Instance.Relations.Add(relation.HGraphId.Value, relation);
            _selectedSource.NodeData.Value.Relations.Add(relation.HGraphId.Value);
            _selectedTarget.NodeData.Value.Relations.Add(relation.HGraphId.Value);
        }
    }

    /// <summary>
    /// Provides Button to display all Relations
    /// </summary>
    [EditorToolbarElement(id, typeof(SceneView))]
    class AutoConnectSuperNode : EditorToolbarToggle, IAccessContainerWindow
    {
        public const string id = "HGraphTools/AutoConnectSuperNode";

        public AutoConnectSuperNode()
        {
            // A toolbar element can be either text, icon, or a combination of the two. Keep in mind that if a toolbar is
            // docked horizontally the text will be clipped, so usually it's a good idea to specify an icon.
            text = "Auto Connect Super Node";
            //icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CreateCubesIcon.png");

            tooltip = "Automatically assings the best overlapping Super Node.";

            this.RegisterValueChangedCallback(Toggle);
        }
        IDisposable sceneRelationSubscriber;

        public EditorWindow containerWindow { get; set; }

        void Toggle(ChangeEvent<bool> val)
        {
            HGraphUtility.AutoConnectSuperNode = val.newValue;
        }



    }
    /// <summary>
    /// Provides Button to display all Relations
    /// </summary>
    [EditorToolbarElement(id, typeof(SceneView))]
    class ShowAllRelations : EditorToolbarToggle, IAccessContainerWindow
    {
        public const string id = "HGraphTools/ShowAllRelations";

        public EditorWindow containerWindow { get; set; }

        public ShowAllRelations()
        {
            // A toolbar element can be either text, icon, or a combination of the two. Keep in mind that if a toolbar is
            // docked horizontally the text will be clipped, so usually it's a good idea to specify an icon.
            text = "Show All Relations";
            //icon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/CreateCubesIcon.png");

            tooltip = "Shows all relations in the graph.";

            this.RegisterValueChangedCallback(Toggle);
        }
        IDisposable sceneRelationSubscriber;
        void Toggle(ChangeEvent<bool> val)
        {
            if (value)
            {
                // Adds all relations to RelationViewSource
                HGraphSceneRelationView.Instance.RelationViewSource.AddRange(HGraph.Instance.SceneRelations.Values);
                sceneRelationSubscriber = new CompositeDisposable(
                    HGraph.Instance.SceneRelations.ObserveReset().Subscribe(x =>
                    {
                        HGraphSceneRelationView.Instance.RelationViewSource.Clear();
                    }),
                    HGraph.Instance.SceneRelations.ObserveAdd().Subscribe(newVal =>
                    {
                        HGraphSceneRelationView.Instance.RelationViewSource.Add(newVal.Value);
                    }),
                    HGraph.Instance.SceneRelations.ObserveReplace().Subscribe(repl =>
                    {
                        HGraphSceneRelationView.Instance.RelationViewSource.Remove(repl.OldValue);
                        HGraphSceneRelationView.Instance.RelationViewSource.Add(repl.NewValue);
                    }),
                    HGraph.Instance.SceneRelations.ObserveRemove().Subscribe(oldVal =>
                    {
                        HGraphSceneRelationView.Instance.RelationViewSource.Remove(oldVal.Value);
                    })
                    );
            }
            else
            {
                // Removes all relations from RelationViewSource
                HGraphSceneRelationView.Instance.RelationViewSource.RemoveRange(HGraph.Instance.SceneRelations.Values);
                sceneRelationSubscriber?.Dispose();
            }
        }
    }



}