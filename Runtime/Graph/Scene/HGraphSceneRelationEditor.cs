using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityUtilities.Reactive;


namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    /// <summary>
    /// Editor for HNodeRelations.
    /// </summary>
    [CustomEditor(typeof(HGraphSceneRelation))]
    [CanEditMultipleObjects]
    public class HGraphSceneRelationEditor : Editor
    {
        string _hGraphId;
        private void OnSceneGUI()
        {
            var relation = (HGraphSceneRelation)target;

            Handles.color = Color.red;
            // Draw line between Source and Target
            if (relation.Source.Value != null && relation.Target.Value != null)
            {
                Handles.DrawLine(relation.Source.Value.transform.position, relation.Target.Value.transform.position, 2);
                Handles.color = Color.black;
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.black;
                Handles.Label((relation.Source.Value.transform.position + relation.Target.Value.transform.position) / 2.0f, relation.HGraphId.Value, style);
                DrawAttributeRelations(relation);
            }
        }

        private void DrawAttributeRelations(HGraphSceneRelation relation)
        {
            if (relation.Source.Value == null || relation.Target.Value == null || relation.RelationData.Value == null)
                return;
            float distance = 2f;
            int numConnections = relation.RelationData.Value.AttributeRelations.Count;
            var start = relation.Source.Value.transform.position;
            var end = relation.Target.Value.transform.position;
            var t1 = start + (end - start) * 0.33f;
            var t2 = start + (end - start) * 0.66f;
            var normal = (end - start).normalized;
            Vector3 lineDirection = (end - start).normalized;
            Vector3 perpendicular = Vector3.Cross(normal, Vector3.up).normalized;
            Quaternion rotation = Quaternion.AngleAxis(360.0f / numConnections, lineDirection);

            foreach (var attRel in relation.RelationData.Value.AttributeRelations.Values)
            {
                var pt1 = t1 + (perpendicular * distance);
                var pt2 = t2 + (perpendicular * distance);
                perpendicular = rotation * perpendicular;
                Color lineColor = Color.black;
                if (HGraph.Instance.Categories.TryGetValue(attRel.Category.Value, out var cat))
                {
                    lineColor = cat.DisplayColor;
                }

                Handles.color = lineColor;
                Handles.DrawBezier(start, end, pt1, pt2, lineColor, null, 2);
            }
        }



        bool _attributeFoldout;
        int _selectedAttribute;


        VisualElement currentInspector;
        CompositeDisposable disposable;
        private void OnEnable()
        {
            HGraphSceneRelation relation = (HGraphSceneRelation)target;
            disposable = new CompositeDisposable(
                relation.RelationData.Subscribe((d) =>
                {
                    RebuildAttributeRelations(currentInspector);
                    UpdateNewAttributeRelationDropDownOptions(currentInspector);
                })
            );
            if (relation.RelationData.Value != null)
            {
                disposable.Add(relation.RelationData.Value.AttributeRelations.ObserveAnyChange().Subscribe(_ =>
                {
                    RebuildAttributeRelations(currentInspector);
                    UpdateNewAttributeRelationDropDownOptions(currentInspector);
                }));
            }

        }

        private void OnDisable()
        {
            disposable?.Dispose();
        }
        public override VisualElement CreateInspectorGUI()
        {
            HGraphSceneRelation relation = (HGraphSceneRelation)target;
            currentInspector = new VisualElement();
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().DefaultHGraphRelationInspector;
            if (visualTree == null)
                return base.CreateInspectorGUI();
            visualTree.CloneTree(currentInspector);

            currentInspector.Q("HGraphId").SetEnabled(false);

            var newAttributeRelationDropdown = currentInspector.Q<DropdownField>("NewAttributeRelationDropDown");
            var newAttributeRelationButton = currentInspector.Q<Button>("NewAttributeRelationButton");
            newAttributeRelationButton.clicked += () =>
            {
                var categoryName = newAttributeRelationDropdown.value;
                var categoryId = HGraphResources.CreateCategoryId(categoryName);
                var defaultContent = HGraphAttributeRelationType.None.CreateData();
                var attributeRelation = HGraphAttributeRelation.Construct(HGraphResources.CreateAttributeRelationId(relation.HGraphId.Value, categoryId), categoryId, defaultContent);
                relation.RelationData.Value.AttributeRelations.Add(categoryName, attributeRelation);
                UpdateNewAttributeRelationDropDownOptions(currentInspector);
            };
            newAttributeRelationDropdown.RegisterValueChangedCallback(v =>
            {
                newAttributeRelationButton.SetEnabled(!string.IsNullOrWhiteSpace(v.newValue));
            });
            UpdateNewAttributeRelationDropDownOptions(currentInspector);

            RebuildAttributeRelations(currentInspector);
            newAttributeRelationButton.SetEnabled(!string.IsNullOrWhiteSpace(newAttributeRelationDropdown.value));



            return currentInspector;
        }
        private void RebuildAttributeRelations(VisualElement inspector)
        {
            if (currentInspector == null)
                return;
            HGraphSceneRelation relation = (HGraphSceneRelation)target;
            var foldout = inspector.Q<Foldout>("AttributeRelationFoldout");
            while (foldout.childCount > 0)
                foldout.RemoveAt(0);
            if (relation.RelationData.Value != null)
            {
                foreach (var attributeRelationKV in relation.RelationData.Value?.AttributeRelations)
                {
                    var attributeRelationSO = ScriptableObject.CreateInstance<HGraphAttributeRelationSO>();
                    attributeRelationSO.AttributeRelation = attributeRelationKV.Value;
                    var serializedAttributeRelation = new SerializedObject(attributeRelationSO);
                    var pf = new PropertyField();
                    pf.label = attributeRelationKV.Key;
                    pf.bindingPath = "AttributeRelation";
                    pf.BindProperty(serializedAttributeRelation);

                    var e = new VisualElement();
                    var arFoldout = new Foldout();
                    arFoldout.value = false;
                    arFoldout.text = attributeRelationKV.Key;
                    arFoldout.Add(pf);
                    e.Add(arFoldout);

                    var b = new Button() { text = "-" };
                    b.style.marginRight = 15;
                    b.clicked += () =>
                    {
                        relation.RelationData.Value.AttributeRelations.Remove(attributeRelationKV.Key);
                    };
                    e.Add(b);
                    b.style.position = Position.Absolute;
                    b.style.top = 0;
                    b.style.right = 0;
                    b.style.maxHeight = 18;
                    b.style.maxWidth = 18;

                    foldout.Add(e);
                }
            }
        }
        private void UpdateNewAttributeRelationDropDownOptions(VisualElement inspector)
        {
            if (currentInspector == null)
                return;
            HGraphSceneRelation relation = (HGraphSceneRelation)target;
            var newAttributeRelationDropdown = inspector.Q<DropdownField>("NewAttributeRelationDropDown");
            newAttributeRelationDropdown.index = -1;
            if (relation.RelationData.Value == null)
            {
                newAttributeRelationDropdown.choices.Clear();
                return;
            }
            var commonAttributes = GetCommonAttributes();
            var missingAttributeRelationsArray = commonAttributes.Except(relation.RelationData.Value.AttributeRelations.Keys).ToList();
            newAttributeRelationDropdown.choices = missingAttributeRelationsArray;

        }


        //public override void OnInspectorGUI()
        //{
        //    HGraphSceneRelation relation =  (HGraphSceneRelation)target;
        //    EditorGUI.BeginDisabledGroup(true);
        //    EditorGUILayout.PropertyField(serializedObject.FindProperty("_hGraphId"));
        //    EditorGUI.EndDisabledGroup();
        //    var commonAttributes = GetCommonAttributes();
        //    var missingAttributeRelationsArray = commonAttributes.Except(relation.RelationData.Value.AttributeRelations.Keys).ToArray();
        //    _selectedAttribute = EditorGUILayout.Popup("Attribute", _selectedAttribute, missingAttributeRelationsArray);
        //    EditorGUI.BeginDisabledGroup(!(_selectedAttribute < missingAttributeRelationsArray.Length));
        //    if(GUILayout.Button("Add Attribute Relation"))
        //    {
        //        var categoryName = missingAttributeRelationsArray[_selectedAttribute];
        //        var categoryId = HGraphResources.CreateCategoryId(categoryName);
        //        var defaultContent = HGraphAttributeRelationType.None.CreateData();
        //        var attributeRelation = HGraphAttributeRelation.Construct(HGraphResources.CreateAttributeRelationId(relation.HGraphId.Value, categoryId), categoryId, defaultContent);
        //        relation.RelationData.Value.AttributeRelations.Add(categoryName, attributeRelation);
        //    }
        //    EditorGUI.EndDisabledGroup();
        //    _attributeFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(_attributeFoldout, "Attributes");
        //    if (_attributeFoldout && relation.RelationData.Value != null)
        //    {
        //        EditorGUI.indentLevel++;
        //        foreach (var attributeRelationKV in relation.RelationData.Value?.AttributeRelations)
        //        {
        //            var attributeRelationSO = ScriptableObject.CreateInstance<HGraphAttributeRelationSO>();
        //            attributeRelationSO.AttributeRelation = attributeRelationKV.Value;
        //            var serializedAttributeRelation = new SerializedObject(attributeRelationSO);
        //            EditorGUILayout.PropertyField(serializedAttributeRelation.FindProperty("AttributeRelation"),new GUIContent(attributeRelationKV.Key));

        //        }
        //        EditorGUI.indentLevel--;
        //    }
        //    EditorGUILayout.EndFoldoutHeaderGroup();
        //}
        private IEnumerable<string> GetCommonAttributes()
        {
            HGraphSceneRelation relation = (HGraphSceneRelation)target;
            var sourceNode = relation.Source.Value;
            var targetNode = relation.Target.Value;
            var sAtt = sourceNode.NodeData.Value.Attributes.Select(a => a.Key);
            var tAtt = targetNode.NodeData.Value.Attributes.Select(a => a.Key);
            return sAtt.Intersect(tAtt);

        }
    }
}