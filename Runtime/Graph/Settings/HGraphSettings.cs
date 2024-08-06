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

using Achioto.Gamespace_PCG.Runtime.Graph.Scene;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Settings
{
    public class HGraphSettings : ScriptableObject
    {
        [SerializeField]
        private Sprite defaultHNodeIcon;
        public Sprite DefaultHNodeIcon => defaultHNodeIcon;
        [SerializeField]
        private Sprite errorHNodeIcon;
        public Sprite ErrorHNodeIcon => errorHNodeIcon;

        [SerializeField]
        private HGraphSceneRelation defaultRelation;
        public HGraphSceneRelation DefaultRelation => defaultRelation;

        [SerializeField]
        private VisualTreeAsset defaultHGraphNodeInspector;
        public VisualTreeAsset DefaultHGraphNodeInspector => defaultHGraphNodeInspector;
        [SerializeField]
        private VisualTreeAsset defaultHGraphNodeDrawer;
        public VisualTreeAsset DefaultHGraphNodeDrawer => defaultHGraphNodeDrawer;
        [SerializeField]
        private VisualTreeAsset defaultHGraphRelationInspector;
        public VisualTreeAsset DefaultHGraphRelationInspector => defaultHGraphRelationInspector;

        [SerializeField]
        private VisualTreeAsset hGraphManagerWindow;
        public VisualTreeAsset HGraphManagerWindow => hGraphManagerWindow;


        [SerializeField]
        private VisualTreeAsset hGraphAssetManagerWindow;
        public VisualTreeAsset HGraphAssetManagerWindow => hGraphAssetManagerWindow;

        [SerializeField]
        private VisualTreeAsset hGraphAssetRecommenderWindow;
        public VisualTreeAsset HGraphAssetRecommenderWindow => hGraphAssetRecommenderWindow;

        [SerializeField]
        private Texture2D hasUserDataIcon;
        public Texture2D HasUserDataIcon => hasUserDataIcon;

        [SerializeField]
        private bool autoUpdatePCGGraph = true;
        public bool AutoUpdatePCGGraph => autoUpdatePCGGraph;

        [Serializable]
        public struct StringColor
        {
            public string id;
            public Color color;
        }

        [SerializeField]
        private List<StringColor> edgeColors;
        public List<StringColor> EdgeColors => edgeColors;

        static HGraphSettings instance;

        public static HGraphSettings GetOrCreateSettings()
        {
            if (instance == null)
                instance = AssetDatabase.LoadAssetAtPath<HGraphSettings>(HGraphSettingsData.k_HGraphSettingsAssetPath);
            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<HGraphSettings>();
                instance.defaultHNodeIcon = AssetDatabase.LoadAssetAtPath<Sprite>(HGraphSettingsData.k_HGraphDefaultNodeIconPath);
                instance.errorHNodeIcon = AssetDatabase.LoadAssetAtPath<Sprite>(HGraphSettingsData.k_HGraphErrorNodeIconPath);

                instance.defaultRelation = AssetDatabase.LoadAssetAtPath<HGraphSceneRelation>(HGraphSettingsData.k_HGraphDefaultRelationPath);
                instance.defaultHGraphNodeInspector = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HGraphSettingsData.k_HGraphDefaultHGraphNodeInspectorPath);
                instance.defaultHGraphNodeDrawer = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HGraphSettingsData.k_HGraphDefaultHGraphNodeDrawerPath);
                instance.defaultHGraphRelationInspector = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HGraphSettingsData.k_HGraphDefaultHGraphRelationInspectorPath);
                instance.hGraphManagerWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HGraphSettingsData.k_HGraphManagerWindowPath);
                instance.hGraphAssetRecommenderWindow = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(HGraphSettingsData.k_HGraphAssetRecommenderWindowPath);
                instance.hasUserDataIcon = AssetDatabase.LoadAssetAtPath<Texture2D>(HGraphSettingsData.k_HGraphHasUserDataIconPath);


                AssetDatabase.CreateAsset(instance, HGraphSettingsData.k_HGraphSettingsAssetPath);
                AssetDatabase.SaveAssets();
            }
            return instance;
        }
        public static void SaveSettings()
        {
            if (instance == null)
                GetOrCreateSettings();
            EditorUtility.SetDirty(instance);
            AssetDatabase.SaveAssets();
        }
        internal static SerializedObject GetSerializedSettings()
        {
            return new SerializedObject(GetOrCreateSettings());
        }
    }

}