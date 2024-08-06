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
