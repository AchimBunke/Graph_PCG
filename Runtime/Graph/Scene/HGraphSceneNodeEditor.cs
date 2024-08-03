using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{

    /// <summary>
    /// Editor for HGraphNode.
    /// </summary>
    [CustomEditor(typeof(HGraphSceneNode), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class HGraphSceneNodeEditor : Editor
    {
        private IDisposable disposables;
        private void OnEnable()
        {
            var hNode = target as HGraphSceneNode;
            var d1 = hNode.HGraphId.Subscribe(newVal =>
            {
                UpdateDuplicateNodeInfo(hNode);
                UpdateCreateButton(hNode);
            });
            var d2 = hNode.NodeData.Subscribe(newData =>
            {
                UpdateMissingGamespaceInfo(hNode);
                UpdateNodeDataFields(hNode);
            });
            disposables = StableCompositeDisposable.Create(d1, d2);
        }
        private void OnDisable()
        {
            disposables.Dispose();
        }

        public void OnSceneGUI()
        {
            var hNode = target as HGraphSceneNode;
            if (hNode.SuperNode.Value != null)
            {
                Handles.color = Color.red;
                Handles.DrawDottedLine(hNode.transform.position, hNode.SuperNode.Value.transform.position, 2);
            }
        }

        VisualElement inspector;
        VisualElement infoPanel;
        VisualElement missingGamespaceNodeInfo;
        VisualElement duplicateInfo;
        Button createNodeButton;
        PropertyField nodeDataField;
        public override VisualElement CreateInspectorGUI()
        {
            var hNode = (HGraphSceneNode)target;
            inspector = new VisualElement();
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().DefaultHGraphNodeInspector;
            if (visualTree == null)
                return base.CreateInspectorGUI();
            visualTree.CloneTree(inspector);

            //Debug
            //VisualElement inspectorFoldout = inspector.Q("Default_Inspector");
            //InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);

            infoPanel = inspector.Q("InfoPanel");
            infoPanel.Add(missingGamespaceNodeInfo = new HelpBox("Gamespace Graph does not contain information for this node!", HelpBoxMessageType.Error));
            UpdateMissingGamespaceInfo(hNode);
            infoPanel.Add(duplicateInfo = new HelpBox("There already exists a node with this HGraphId!", HelpBoxMessageType.Error));
            UpdateDuplicateNodeInfo(hNode);
            createNodeButton = new Button();
            createNodeButton.text = "Add to Gamespace Graph";
            createNodeButton.clicked += () =>
            {
                hNode.ConnectAsNewNode();
                UpdateCreateButton(hNode);

                //serializedObject.Update();
            };
            UpdateCreateButton(hNode);
            infoPanel.Add(createNodeButton);
            nodeDataField = inspector.Q<PropertyField>("NodeData");
            UpdateNodeDataFields(hNode);
            return inspector;
        }
        private void UpdateMissingGamespaceInfo(HGraphSceneNode hNode)
        {
            if (missingGamespaceNodeInfo != null)
                missingGamespaceNodeInfo.style.display = hNode.NodeData.Value == null ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void UpdateDuplicateNodeInfo(HGraphSceneNode hNode)
        {
            if (duplicateInfo != null)
                duplicateInfo.style.display = hNode.IsDuplicate.Value ? DisplayStyle.Flex : DisplayStyle.None;
        }
        private void UpdateCreateButton(HGraphSceneNode hNode)
        {
            if (createNodeButton != null)
                createNodeButton.SetEnabled(
                    HGraphResources.IsHGraphIdValid(hNode.HGraphId.Value) &&
                    !hNode.IsDuplicate.Value &&
                    !hNode.IsHGraphConnected);

        }
        private void UpdateNodeDataFields(HGraphSceneNode hNode)
        {

            if (nodeDataField == null)
                return;
            nodeDataField.style.display = hNode.NodeData.Value != null ? DisplayStyle.Flex : DisplayStyle.None;
            if (hNode.NodeData.Value == null)
            {
                return;
            }
            var dummy = ScriptableObject.CreateInstance<HGraphNodeSO>();
            dummy.Node = hNode.NodeData.Value;
            var serializedDummy = new SerializedObject(dummy);
            var p = serializedDummy.FindProperty("Node");
            nodeDataField.bindingPath = "Node";
            nodeDataField.Bind(serializedDummy);
        }
    }
}