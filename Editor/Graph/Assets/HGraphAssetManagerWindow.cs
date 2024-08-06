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

using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using ObjectField = UnityEditor.UIElements.ObjectField;

namespace Achioto.Gamespace_PCG.Editor.Graph.Assets
{
    public class HGraphAssetManagerWindow : EditorWindow
    {
        [MenuItem("Window/HGraph/HGraph Asset Manager", priority = 4000, secondaryPriority = 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<HGraphAssetManagerWindow>("HGraph Asset Manager");
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItemGUI;
        }
        static void OnProjectWindowItemGUI(string guid, Rect selectionRect)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrWhiteSpace(assetPath))
                return;
            bool hasUserData = !string.IsNullOrWhiteSpace(AssetImporter.GetAtPath(assetPath).userData);
            if (!hasUserData)
                return;
            // Calculate icon position
            float iconWidth = 16f;
            float iconHeight = 16f;
            float iconX = selectionRect.x + selectionRect.width - iconWidth - 4f;
            float iconY = selectionRect.y + 2f;
            Rect iconRect = new Rect(iconX, iconY, iconWidth, iconHeight);

            // Draw icon based on user data
            Texture2D icon = HGraphSettings.GetOrCreateSettings().HasUserDataIcon;
            GUI.DrawTexture(iconRect, icon);
        }


        private void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
        }
        private void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
        }
        private void OnSelectionChanged()
        {
            if (Selection.assetGUIDs.Length == 0)
                return;
            string assetPath = AssetDatabase.GUIDToAssetPath(Selection.assetGUIDs.Last());
            if (_selectedObjectField != null)
                _selectedObjectField.value = AssetDatabase.LoadAssetAtPath(assetPath, typeof(Object));
        }


        private Object _selectedObject;
        private AssetDataSO _selectedScriptableObject;
        private SerializedObject _selectedSerializedObject;
        private HGraphAssetData _assetData;
        private string _userData;
        ObjectField _selectedObjectField;


        private void CreateGUI()
        {
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().HGraphAssetManagerWindow;
            visualTree.CloneTree(rootVisualElement);
            _selectedObjectField = rootVisualElement.Q<ObjectField>("SelectedObject");
            _selectedObjectField.RegisterValueChangedCallback(evt =>
            {
                _selectedObject = evt.newValue;
                OnNewObject();
            });
            var pField = rootVisualElement.Q<PropertyField>("AssetDataInspector");
            pField.bindingPath = nameof(AssetDataSO.AssetData);
            rootVisualElement.Q<Button>("Save").clicked += () =>
            {
                SaveUserData();
            };
            rootVisualElement.Q<Button>("Remove").clicked += () =>
            {
                RemoveUserData();
            };

        }
        public class AssetDataSO : ScriptableObject
        {
            public HGraphAssetData AssetData;
        }
        public void OnNewObject()
        {
            _assetData = GetAssetData();
            _selectedScriptableObject = ScriptableObject.CreateInstance<AssetDataSO>();
            _selectedScriptableObject.AssetData = _assetData;

            _selectedSerializedObject = new SerializedObject(_selectedScriptableObject);
            _selectedSerializedObject.Update();

            var pField = rootVisualElement.Q<PropertyField>("AssetDataInspector");
            pField.Unbind();
            var serializedProp = _selectedSerializedObject.FindProperty("AssetData");
            if(serializedProp == null)
            {
                Debug.LogError("serialized property is null");
            }
            pField.BindProperty(serializedProp);

            var rawField = rootVisualElement.Q<TextField>("Raw");
            if (rawField != null)
                rawField.value = _userData;
        }
        private HGraphAssetData GetAssetData()
        {
            if (_selectedObject == null)
                return default;
            var assetPath = AssetDatabase.GetAssetPath(_selectedObject);
            _userData = AssetImporter.GetAtPath(assetPath).userData;
            return ParseUserData(_userData);
        }
        private HGraphAssetData ParseUserData(string userData)
        {
            if (string.IsNullOrWhiteSpace(userData))
                return new HGraphAssetData();
            HGraphSerializationController.TryDeserializeAssetData(userData, out var data);
            return data;
        }
        public void SaveUserData()
        {
            var assetPath = AssetDatabase.GetAssetPath(_selectedObject);
            if (_assetData == null)
                _userData = string.Empty;
            else
                _userData = HGraphSerializationController.Serialize(_assetData);
            AssetImporter.GetAtPath(assetPath).userData = _userData;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            rootVisualElement.Q<TextField>("Raw").value = _userData;
            var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (!HGraphAssetRegistry.Instance.Assets.ContainsKey(assetGUID))
            {
                HGraphAssetRegistry.Instance.RegisterAsset(assetGUID, _assetData);
            }
            else
            {
                HGraphAssetRegistry.Instance.Assets[assetGUID] = _assetData;
            }
        }
        public void RemoveUserData()
        {
            var assetPath = AssetDatabase.GetAssetPath(_selectedObject);
            _userData = string.Empty;
            AssetImporter.GetAtPath(assetPath).userData = _userData;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            rootVisualElement.Q<TextField>("Raw").value = _userData;
            var assetGUID = AssetDatabase.AssetPathToGUID(assetPath);
            if (HGraphAssetRegistry.Instance.Assets.ContainsKey(assetGUID))
            {
                HGraphAssetRegistry.Instance.Assets.Remove(assetGUID);
            }
        }
    }
}