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