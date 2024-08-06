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
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using UniRx;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Editor.Graph.Drawer
{
    [CustomPropertyDrawer(typeof(HGraphNode))]
    public class HGraphNodeDrawer : PropertyDrawer, IDisposable
    {
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            var hNode = fieldInfo.GetValue(property.serializedObject.targetObject) as HGraphNode;
            var hGraph = HGraph.Instance;
            var inspector = new VisualElement();
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().DefaultHGraphNodeDrawer;
            visualTree.CloneTree(inspector);


            var infoPanel = new VisualElement();
            var nameField = inspector.Q<TextField>("Name");
            nameField.AddToClassList("unity-base-field__aligned");
            nameField.RegisterValueChangedCallback(evt =>
            {
                hNode.Name.Value = evt.newValue;
            });

            var superNodeFields = inspector.Q("SuperNodeFields");
            var superNodeObjectField = inspector.Q<ObjectField>("SuperNodeObject");
            var superNodeTextField = inspector.Q<TextField>("SuperNodeText");
            superNodeObjectField.AddToClassList("unity-base-field__aligned");
            superNodeObjectField.RegisterValueChangedCallback(evt =>
            {
                var newNode = (HGraphSceneNode)evt.newValue;
                if (newNode == null)
                {
                    hNode.SuperNode.Value = null;
                    return;
                }

                if (!newNode.IsRegistered.Value)
                {
                    hNode.SuperNode.Value = null;
                    superNodeObjectField.value = evt.previousValue;
                    return;
                }
                hNode.SuperNode.Value = newNode.HGraphId.Value;


            });

            superNodeTextField.RegisterValueChangedCallback(evt =>
            {
                hNode.SuperNode.Value = evt.newValue;
            });

            var addAttributeButton = inspector.Q<Button>("AddAttributeButton");
            addAttributeButton.SetEnabled(false);

            var categorySearchField = inspector.Q<ToolbarPopupSearchField>("CategorySearchField");
            categorySearchField.RegisterValueChangedCallback<string>(val =>
            {
                FilterAttributes(inspector);
                if (HGraph.Instance.Categories.ContainsKey(val.newValue) && !hNode.LocalAttributes.ContainsKey(val.newValue))
                    addAttributeButton.SetEnabled(true);
                else
                    addAttributeButton.SetEnabled(false);
            });
            foreach (var c in HGraph.Instance.Categories.Keys)
            {
                categorySearchField.menu.AppendAction(c, (s) => categorySearchField.value = s.name);
            }
            addAttributeButton.clicked += () =>
            {
                hNode.LocalAttributes.Add(categorySearchField.value, HGraph.Instance.Categories[categorySearchField.value].CreateAttribute());
                addAttributeButton.SetEnabled(false);
            };
            DisplayAttributes(inspector, hNode);



            disposables = new CompositeDisposable(
                hNode.Name.Subscribe(newName => nameField.value = newName),
                hNode.SuperNode.Subscribe(newSuperNode =>
                {
                    if (HGraphResources.IsHGraphIdValid(newSuperNode))
                    {
                        hGraph.SceneNodes.TryGetValue(newSuperNode, out var sceneSuperNode);
                        superNodeObjectField.SetValueWithoutNotify(sceneSuperNode);
                    }
                    else
                        superNodeObjectField.SetValueWithoutNotify(null);
                    superNodeTextField.SetValueWithoutNotify(newSuperNode);
                }),
                hNode.Attributes.ObserveAnyChange().ThrottleFrame(1).Subscribe(_ => RefreshAttributes(inspector, hNode))
            );


            return inspector;
        }

        CompositeDisposable disposables;
        public void Dispose()
        {
            disposables?.Dispose();
        }

        private void RemoveAttributes(VisualElement inspector, HGraphNode _)
        {
            var attributesFoldout = inspector.Q<Foldout>("Attributes");
            var attributeControl = attributesFoldout.Q("AttributeControl");
            var children = attributesFoldout.contentContainer.Children().ToArray();
            foreach (var c in children)
            {
                if (c == attributeControl)
                    continue;
                attributesFoldout.Remove(c);
            }
        }
        private void RefreshAttributes(VisualElement inspector, HGraphNode hNode)
        {
            if (inspector == null)
                return;
            RemoveAttributes(inspector, hNode);
            DisplayAttributes(inspector, hNode);
            FilterAttributes(inspector);
        }
        private void FilterAttributes(VisualElement inspector)
        {
            var attributesFoldout = inspector.Q<Foldout>("Attributes");
            var attributeControl = attributesFoldout.Q("AttributeControl");
            var categorySearchField = inspector.Q<ToolbarPopupSearchField>("CategorySearchField");
            var children = attributesFoldout.contentContainer.Children().ToArray();
            foreach (var c in children)
            {
                if (c == attributeControl)
                    continue;
                c.style.display = FilterAttribute(c.userData as string, categorySearchField.value) ? DisplayStyle.Flex : DisplayStyle.None;
            }
        }
        private void DisplayAttributes(VisualElement inspector, HGraphNode hNode)
        {
            var attributesFoldout = inspector.Q<Foldout>("Attributes");

            foreach (var attributeKV in hNode.Attributes)
            {
                var attributeSO = ScriptableObject.CreateInstance<HGraphAttributeSO>();
                attributeSO.Attribute = attributeKV.Value;
                var serializedAttribute = new SerializedObject(attributeSO);
                serializedAttribute.Update();
                VisualElement e = new VisualElement();
                e.style.flexDirection = FlexDirection.Row;
                var attProp = serializedAttribute.FindProperty(nameof(HGraphAttributeSO.Attribute));
                if (attProp == null)
                    Debug.LogError($"Property null in {inspector}");

                var pf = new PropertyField(attProp);
                pf.BindProperty(serializedAttribute);
                pf.style.flexGrow = 1;
                pf.style.flexShrink = 1;
                e.Add(pf);
                HGraph.Instance.Categories.TryGetValue(attributeKV.Value.Category.Value, out var cat);
                if (!hNode.Attributes.IsFallbackKey(attributeKV.Key))
                {
                    if (cat != null)
                    {
                        pf.label = cat.Name.Value;
                        e.userData = cat.Name.Value;
                    }
                    else
                        pf.label = $"[No Category]";

                    var removeButton = CreateRemoveAttributeButton(inspector, hNode, attributeKV.Value);
                    removeButton.style.position = Position.Absolute;
                    removeButton.style.top = 0;
                    removeButton.style.right = 0;
                    removeButton.style.maxHeight = 18;
                    removeButton.style.maxWidth = 18;
                    e.contentContainer.Add(removeButton);
                }
                else
                {
                    e.userData = "(inherited) " + attributeKV.Key;
                    if (cat != null)
                        pf.label = $"(inherited) {cat.Name.Value}";
                    else
                        pf.label = $"(inherited) [No Category]";
                    pf.SetEnabled(false);
                }
                attributesFoldout.contentContainer.Add(e);
            }
        }

        private Button CreateRemoveAttributeButton(VisualElement inspector, HGraphNode hNode, HGraphAttribute attribute)
        {
            var b = new Button() { text = "-" };
            b.style.marginRight = 15;
            b.clicked += () => RemoveAttributeClicked(inspector, hNode, attribute);
            return b;
        }
        private void RemoveAttributeClicked(VisualElement inspector, HGraphNode hNode, HGraphAttribute attribute)
        {
            hNode.LocalAttributes.Remove(attribute.Category.Value);
        }
        private bool FilterAttribute(string text, string filter)
        {
            if (string.IsNullOrWhiteSpace(filter))
                return true;
            return Regex.IsMatch(text, filter, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        }

    }

}