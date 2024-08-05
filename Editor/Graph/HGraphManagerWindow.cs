using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UniRx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using UnityUtilities.Reactive;

namespace Achioto.Gamespace_PCG.Editor.Graph
{
    /// <summary>
    /// An EditorWindow for working with HGraphs.
    /// </summary>
    public class HGraphManagerWindow : EditorWindow
    {
        [MenuItem("Window/HGraph/HGraph Manager", priority = 4000, secondaryPriority = 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<HGraphManagerWindow>("HGraph");
           
        }

        string graphFilePath="";
        string currentGraphDirectoryPath = "";
        
        string newCategoryName="";
        string newEnumName="";
        HGraphAttributeType newCategoryType = HGraphAttributeType.Nominal;

        bool keepCurrentGraph = false;
        bool loadUpdate = false;

        IDisposable disposable;
        bool saveModules = false;

        private void CreateGUI()
        {
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().HGraphManagerWindow;
            visualTree.CloneTree(rootVisualElement);

            rootVisualElement.Q<Button>("RefreshSceneButton").clicked += () =>
            {
                EditorSceneManager.OpenScene(EditorSceneManager.GetActiveScene().path);
            };
            rootVisualElement.Q<Button>("ClearGraphButton").clicked += () =>
            {
                HGraphController.ClearGraph();
            };
            rootVisualElement.Q<Toggle>("KeepGraphToggle").RegisterValueChangedCallback(v =>
            {
                keepCurrentGraph = v.newValue;
            });
            rootVisualElement.Q<Toggle>("LoadUpdateToggle").RegisterValueChangedCallback(v =>
            {
                loadUpdate = v.newValue;
            });
            rootVisualElement.Q<Toggle>("Save_Modules").RegisterValueChangedCallback(v =>
            {
                saveModules = v.newValue;
            });
            var loadGraphButton = rootVisualElement.Q<Button>("LoadGraphButton");
            var saveGraphButton = rootVisualElement.Q<Button>("SaveGraphButton");
            var pathField = rootVisualElement.Q<TextField>("PathField");
            pathField.RegisterValueChangedCallback(path =>
            {
                graphFilePath = path.newValue;
                loadGraphButton.SetEnabled(File.Exists(graphFilePath));
                saveGraphButton.SetEnabled(File.Exists(graphFilePath));
            });
            rootVisualElement.Q<Button>("PathButton").clicked += () =>
            {
                var nextGraphFilePath = EditorUtility.OpenFilePanel("Graph File", currentGraphDirectoryPath, "json,xml");
                graphFilePath = nextGraphFilePath;
                pathField.SetValueWithoutNotify(graphFilePath);
                loadGraphButton.SetEnabled(File.Exists(graphFilePath));
                saveGraphButton.SetEnabled(File.Exists(graphFilePath));
            };
            rootVisualElement.Q<Button>("LoadGraphForCurrentScene").clicked += () =>
            {
                graphFilePath = HGraphUtility.CreateGraphFilePathFromCurrentEditorScene();
                pathField.SetValueWithoutNotify(graphFilePath);
                loadGraphButton.SetEnabled(File.Exists(graphFilePath));
                saveGraphButton.SetEnabled(File.Exists(graphFilePath));
                LoadGraph();
            };
            rootVisualElement.Q<Button>("NewGraphButton").clicked += () =>
            {
                CreateNewGraphFromCurrentScene();
                pathField.SetValueWithoutNotify(graphFilePath);
                loadGraphButton.SetEnabled(File.Exists(graphFilePath));
                saveGraphButton.SetEnabled(File.Exists(graphFilePath));
            };
            loadGraphButton.clicked += () =>
            {
                LoadGraph();
            };
            saveGraphButton.clicked += () =>
            {
                HGraphSerializationController.SerializeToFile(HGraph.Instance, graphFilePath, saveModules);
                PCGGraphManager.Instance.SetPCGGraphDirty();
                HGraphSerializationController.SerializeToFile(PCGGraphManager.Instance.PCGGraph, Path.ChangeExtension(graphFilePath, ".pcg.json"));
            };
// ERROR IN CATEGORIES !!! 
            UpdateCategoriesFoldout(rootVisualElement.Q("CategoriesFoldout"));
            EditorSceneManager.sceneOpened += (_,_)=> UpdateCategoriesFoldout(rootVisualElement.Q("CategoriesFoldout"));

            var newCatButton = rootVisualElement.Q<Button>("NewCategoryButton");
            rootVisualElement.Q<EnumField>("NewCategoryTypePopup").RegisterValueChangedCallback(v =>
            {
                newCategoryType = (HGraphAttributeType)v.newValue;
                UpdateCreateCategoryButtonState();
                UpdateNewCategoryConfigurationFields(newCategoryType);
            });

            UpdateNewCategoryConfigurationFields(newCategoryType);

            newCatButton.clicked += () =>
            {
                var newCategory = HGraphCategory.Construct(newCategoryName);
                newCategory.Name.Value = newCategoryName;
                newCategory.Type.Value = newCategoryType;
                var minMax = GetMinMaxValues();
                newCategory.MinValue = minMax.min;
                newCategory.MaxValue = minMax.max;
                newCategory.DefaultData = newCategoryType.CreateData();

                HGraph.Instance.Categories.Add(HGraphResources.CreateCategoryId(newCategoryName), newCategory);
                UpdateCategoriesFoldout(rootVisualElement.Q("CategoriesFoldout"));
            };
            rootVisualElement.Q<TextField>("NewCategoryNameField").RegisterValueChangedCallback(v =>
            {
                newCategoryName = v.newValue;
                UpdateCreateCategoryButtonState();
            });

            rootVisualElement.Q<TextField>("NewEnumField").RegisterValueChangedCallback(v =>
            {
                newEnumName = v.newValue;
                UpdateCreateEnumButtonState();
            });
            var newEnumButton = rootVisualElement.Q<Button>("NewEnumButton");
            newEnumButton.clicked += () =>
            {
                var newEnum = HGraphEnum.Construct(newEnumName);
                HGraph.Instance.EnumDefinitions.Add(newEnumName, newEnum);
                UpdateCreateEnumButtonState();
            };

            var ruleDataField = rootVisualElement.Q<ObjectField>("RuleDataField");
            ruleDataField.RegisterValueChangedCallback(v =>
            {
                HGraph.Instance.Rules.Value = v.newValue as HGraphRuleCollection;
            });

            disposable = new CompositeDisposable(
                HGraph.Instance.Categories.ObserveAnyChange().ThrottleFrame(1).Subscribe(_ =>
                {
                    UpdateCategoriesFoldout(rootVisualElement.Q("CategoriesFoldout"));
                    UpdateCreateCategoryButtonState();
                }),
                 HGraph.Instance.EnumDefinitions.ObserveAnyChange().ThrottleFrame(1).Subscribe(_ =>
                 {
                     UpdateEnumFoldout(rootVisualElement.Q("EnumFoldout"));
                     //UpdateCategoriesFoldout(rootVisualElement.Q("CategoriesFoldout"));
                     UpdateCreateEnumButtonState();
                 }),
                 HGraph.Instance.Rules.Subscribe(v =>
                 {
                     ruleDataField.value = v;
                 })
                );

        }
        private void UpdateNewCategoryConfigurationFields(HGraphAttributeType type)
        {
            UpdateMinMaxFields(type);
        }
        private (float min, float max) GetMinMaxValues()
        {
            var minFloat = rootVisualElement.Q<FloatField>("NewCategoryMinFloatField");
            var maxFloat = rootVisualElement.Q<FloatField>("NewCategoryMaxFloatField");
            var minInt = rootVisualElement.Q<IntegerField>("NewCategoryMinIntField");
            var maxInt = rootVisualElement.Q<IntegerField>("NewCategoryMaxIntField");
            return (minFloat.style.display == DisplayStyle.Flex ? minFloat.value : minInt.style.display == DisplayStyle.Flex ? minInt.value : float.NaN,
                maxFloat.style.display == DisplayStyle.Flex ? maxFloat.value : maxInt.style.display == DisplayStyle.Flex ? maxInt.value : float.NaN);
        }
        private void UpdateMinMaxFields(HGraphAttributeType type)
        {
            var minFloat = rootVisualElement.Q<FloatField>("NewCategoryMinFloatField");
            var maxFloat = rootVisualElement.Q<FloatField>("NewCategoryMaxFloatField");
            var minInt = rootVisualElement.Q<IntegerField>("NewCategoryMinIntField");
            var maxInt = rootVisualElement.Q<IntegerField>("NewCategoryMaxIntField");
            minFloat.style.display = DisplayStyle.None;
            maxFloat.style.display = DisplayStyle.None;
            minInt.style.display = DisplayStyle.None;
            maxInt.style.display = DisplayStyle.None;

            if (type.IsNumericValueType(out var valueType))
            {
                if (valueType == typeof(float))
                {
                    minFloat.style.display = DisplayStyle.Flex;
                    maxFloat.style.display = DisplayStyle.Flex;
                }
                else if (valueType == typeof(int) || valueType == typeof(uint))
                {
                    minInt.style.display = DisplayStyle.Flex;
                    maxInt.style.display = DisplayStyle.Flex;
                }
            }
        }
        private void UpdateCategoriesFoldout(VisualElement foldout)
        {
            foldout.Clear();
            for (int i = 0; i < HGraph.Instance.Categories.Count; ++i)
            {
                var categoryKV = HGraph.Instance.Categories.ElementAt(i);

                var categorySO = ScriptableObject.CreateInstance<HGraphCategorySO>();
                categorySO.Category = categoryKV.Value;

                var serializedCategory = new SerializedObject(categorySO);
                var catProp = serializedCategory.FindProperty(nameof(HGraphCategorySO.Category));

                var propertyField = new PropertyField(catProp, categoryKV.Key);
                propertyField.BindProperty(catProp);
                foldout.Add(propertyField);

            }
        }
        private void UpdateEnumFoldout(VisualElement foldout)
        {
            while (foldout.childCount > 0)
                foldout.RemoveAt(0);
            for (int i = 0; i < HGraph.Instance.EnumDefinitions.Count; ++i)
            {
                var enumKV = HGraph.Instance.EnumDefinitions.ElementAt(i);
                var enumSO = ScriptableObject.CreateInstance<HGraphEnumSO>();
                enumSO.Enum = enumKV.Value;
                var serializedEnum = new SerializedObject(enumSO);
                serializedEnum.Update();
                var enumProp = serializedEnum.FindProperty(nameof(HGraphEnumSO.Enum));
                var pf = new PropertyField(enumProp, enumKV.Value.HGraphId.Value);
                pf.BindProperty(enumProp);
                foldout.Add(pf);
            }
        } 
        private void UpdateCreateCategoryButtonState()
        {
            var b = HGraphResources.IsHGraphIdValid(newCategoryName) && !HGraph.Instance.Categories.ContainsKey(newCategoryName);
            rootVisualElement.Q<Button>("NewCategoryButton")?.SetEnabled(b);
        }
        private void UpdateCreateEnumButtonState()
        {
            var b = HGraphResources.IsHGraphIdValid(newEnumName) && !HGraph.Instance.EnumDefinitions.ContainsKey(newEnumName);
            rootVisualElement.Q<Button>("NewEnumButton")?.SetEnabled(b);
        }
        //private void UpdateCreateRuleButtonState()
        //{
        //    var b = HGraphResources.IsHGraphIdValid(newRuleName) && !HGraph.Instance.Rules.ContainsKey(newRuleName);
        //    rootVisualElement.Q<Button>("NewRuleButton")?.SetEnabled(b);
        //}
        private void CreateNewGraphFromCurrentScene()
        {
            graphFilePath = HGraphUtility.CreateGraphFilePathFromCurrentEditorScene();
            HGraphSerializationController.SerializeToFile(HGraph.Instance, graphFilePath, saveModules);
            PCGGraphManager.Instance.SetPCGGraphDirty();
            HGraphSerializationController.SerializeToFile(PCGGraphManager.Instance.PCGGraph, Path.ChangeExtension(graphFilePath, ".pcg.json"));
            AssetDatabase.Refresh();
        }
        private void LoadGraph()
        {
            var data = HGraphSerializationController.DeserializeFromFile(graphFilePath);
            HGraphController.Load(data, keepCurrentGraph, loadUpdate);
        }

    }

}