using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Editor.Graph.Drawer
{

    [CustomPropertyDrawer(typeof(HGraphAttributeRelation))]
    public class HGraphAttributeRelationDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            //var attributeRelation = (property.boxedValue as HGraphAttributeRelation);
            var data_prop = property.FindPropertyRelative("_data");
            if (data_prop == null)
                Debug.LogError($"Property null in {property}");
            var graphId_prop = property.FindPropertyRelative("_hGraphId");
            if (graphId_prop == null)
                Debug.LogError($"Property null in {property}");
            var e = new VisualElement();
            var pf_id = new PropertyField(graphId_prop, "HGraph Id");
            pf_id.BindProperty(graphId_prop);
            pf_id.SetEnabled(false);
            e.Add(pf_id);
            var pf_cat = new PropertyField(property.FindPropertyRelative("_category"), "Category");
            pf_cat.BindProperty(property.FindPropertyRelative("_category"));
            pf_cat.SetEnabled(false);
            e.Add(pf_cat);
            var type_prop = data_prop.FindPropertyRelative("type");
            if (type_prop == null)
                Debug.LogError($"Property null in {property}");
            var enumField = new EnumField("Data Type", (HGraphAttributeRelationType)type_prop.enumValueIndex);
            enumField.ToggleInClassList("unity-property-field__inspector-property");
            enumField.RegisterValueChangedCallback(v =>
            {
                var newContent = ((HGraphAttributeRelationType)v.newValue).CreateData();
                data_prop.serializedObject?.Update();
                data_prop.boxedValue = newContent;
                data_prop.serializedObject.ApplyModifiedProperties();
            });
            e.Add(enumField);

            var pf_data = new PropertyField(data_prop, "Data");
            pf_data.BindProperty(data_prop);
            e.Add(pf_data);
            return e;
        }
    }

}