using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;


namespace Achioto.Gamespace_PCG.Editor.Graph.Drawer
{
    [CustomPropertyDrawer(typeof(EnumHGraphAttribute), true)]
    public class HGraphEnumContentPropertyDrawer : PropertyDrawer
    {
        VisualElement container;
        VisualElement valueSelector;
        // If Serialized Error returns try to Not cache
        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {
            container = new VisualElement();
            var enumDropDown = new DropdownField("Enum");
            enumDropDown.choices = HGraph.Instance.EnumDefinitions.Keys.ToList();
            var enumProperty = property.FindPropertyRelative("Enum");
            if (enumProperty == null)
                Debug.LogError($"Property null in {property}");
            enumDropDown.RegisterValueChangedCallback(v =>
            {
                enumProperty.stringValue = v.newValue;
                enumProperty.serializedObject.ApplyModifiedProperties();
            });
            enumDropDown.index = enumDropDown.choices.IndexOf(enumProperty.stringValue);
            container.Add(enumDropDown);

            if (!HGraph.Instance.EnumDefinitions.TryGetValue(enumProperty.stringValue, out var hEnum))
            {
                return container;
            }

            var valueProperty = property.FindPropertyRelative("Value");
            if (valueProperty == null)
                Debug.LogError($"Property null in {property}");

            enumDropDown.RegisterValueChangedCallback(v =>
            {
                if (v.newValue == v.previousValue)//this happens!!
                    return;
                Refresh(valueProperty, enumProperty, property);
                valueProperty.intValue = 0;
                valueProperty.serializedObject.ApplyModifiedProperties();
            });

            Refresh(valueProperty, enumProperty, property);

            return container;
        }
        private List<string> GetValueOptions(SerializedProperty property)
        {
            var hEnumid = property.FindPropertyRelative("Enum").stringValue;
            if (hEnumid == null)
                Debug.LogError($"Property null in {property}");
            if (HGraphResources.IsHGraphIdValid(hEnumid))
            {
                if (HGraph.Instance.EnumDefinitions.TryGetValue(hEnumid, out var hEnum))
                {
                    return hEnum.Entries.Select(e => e.Name).ToList();
                }
                else
                    throw new Exception("Could not resolve HGraphEnum");
            }
            return new();
        }

        private string[] GetFlags(int value, HGraphEnum hEnum)
        {
            return hEnum.GetFlags(value);
        }
        private void Refresh(SerializedProperty valueProperty, SerializedProperty enumProperty, SerializedProperty property)
        {
            if(valueSelector != null)
                container.Remove(valueSelector);// clear current
            if (!HGraph.Instance.EnumDefinitions.TryGetValue(enumProperty.stringValue, out var hEnum))
            {
                return;
            }
            if (hEnum.Flags)
            {
                int initialValue = valueProperty.intValue;
                string[] initialFlags = GetFlags(initialValue, hEnum);

                var toolbarMenu = new ToolbarMenu() { text = string.Join(", ", GetFlags(initialValue, hEnum)) };
                valueSelector = toolbarMenu;
                toolbarMenu.AddToClassList("unity-base-field__aligned");
                EnumEntry[] enumFlags = hEnum.Entries.ToArray();
                foreach (var entry in enumFlags)
                {
                    toolbarMenu.menu.AppendAction(entry.Name, (a) =>
                    {
                        var toggledFlag = hEnum.GetValue(a.name);
                        if (toggledFlag == 0)// Reset to 0 if toggled flags is 0.
                        {
                            valueProperty.intValue = 0;
                        }
                        else if (!hEnum.IsCombinedValue(toggledFlag))// XOR to toggle only the provided flag.
                        {
                            var previousValue = valueProperty.intValue;
                            valueProperty.intValue = previousValue ^ toggledFlag;
                        }
                        else
                        {
                            int val = valueProperty.intValue;
                            if (val == toggledFlag)// Reset to 0 if toggled flags is equal to current set flags
                            {
                                valueProperty.intValue = 0;
                            }
                            else
                            {
                                //if (hEnum.IsExclusiveFlag(val))// Set to toggled flag if
                                //{

                                //    valueProperty.intValue = toggledFlag;
                                //    valueProperty.serializedObject.ApplyModifiedProperties();
                                //}
                                //else
                                //{
                                //    valueProperty.intValue = toggledFlag;
                                //    valueProperty.serializedObject.ApplyModifiedProperties();
                                //}
                                valueProperty.intValue = toggledFlag;
                            }
                        }
                        valueProperty.serializedObject.ApplyModifiedProperties();
                        toolbarMenu.text = string.Join(", ", GetFlags(valueProperty.intValue, hEnum));

                    },
                    (a) =>
                    {
                        var selectedValue = valueProperty.intValue;
                        string[] selectedFlags = GetFlags(selectedValue, hEnum);
                        if (selectedValue == 0)
                            return DropdownMenuAction.Status.Normal;
                        if (selectedFlags.Contains(a.name))
                            return DropdownMenuAction.Status.Checked;
                        return DropdownMenuAction.Status.Normal;
                    });
                }
                container.Add(toolbarMenu);
            }
            else
            {
                var valueField = new DropdownField("Value");
                valueSelector = valueField;
                valueField.choices = GetValueOptions(property);
                valueField.index = valueProperty.intValue;
                valueField.BindProperty(valueProperty);
                container.Add(valueField);
            }
        }
    }
}
