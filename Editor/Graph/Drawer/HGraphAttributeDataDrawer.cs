using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Editor.Graph.Drawer
{
    [CustomPropertyDrawer(typeof(HGraphAttributeData))]
    public class HGraphAttributeDataDrawer : PropertyDrawer
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var hGraph = HGraph.Instance;
            //HGraphAttributeData data = property.boxedValue as HGraphAttributeData;
            var root = new VisualElement();
            var foldout = new Foldout();
            var categoryProperty = property.FindPropertyRelative("category");
            if (categoryProperty == null)
                Debug.LogError($"Property null in {property}");
            foldout.text = categoryProperty.stringValue;
            root.Add(foldout);
            var categoryField = new DropdownField("Category");
            var categories = hGraph.Categories.Values.Select(c => c.Name.Value).ToList();
            var selected = categories.IndexOf(categoryProperty.stringValue);
            categoryField.choices = categories;
            categoryField.index = selected;
            var contentProperty = property.FindPropertyRelative("data");
            if (contentProperty == null)
                Debug.LogError($"Property null in {property}");
            var attributeContentField = new PropertyField(contentProperty, "content");
            attributeContentField.BindProperty(contentProperty);
            categoryField.RegisterValueChangedCallback(evt =>
            {
                var newVal = evt.newValue;
                categoryProperty.stringValue = newVal;
                if (categoryProperty == null)
                    Debug.LogError($"Property null in {property}");
                var category = hGraph.Categories[newVal];
                contentProperty.managedReferenceValue = (category.DefaultData?.Copy() ?? category.Type.Value.CreateData());
                property.serializedObject.ApplyModifiedProperties();
                property.serializedObject.Update();
                attributeContentField.BindProperty(contentProperty);
            });
            // categoryField.choices = hGraph.Categories.Values.Select(c => c.Name.Value).ToList();
            foldout.Add(categoryField);


            foldout.Add(attributeContentField);
            return root;
        }
    }
}
