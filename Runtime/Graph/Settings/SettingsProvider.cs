using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Settings
{
    static class HGraphSettingsData
    {
        public const string PackageRuntimeDirPath = "Packages/com.achioto.gamespace-pcg/Runtime/";
        public const string PackageEditorDirPath = "Packages/com.achioto.gamespace-pcg/Editor/";



        public const string k_HGraphSettingsAssetPath = PackageRuntimeDirPath + "Graph/Resources/HGraphSettings.asset";
        public const string k_HGraphSettingsName = "HGraph";
        public const string k_HGraphProjectSettingsPath = "Project/" + k_HGraphSettingsName;
        public const string k_HGraphProjectSettingsStyleSheetPath = "Assets/Graph/Settings/hGraphSettings_ui.uss";


        public const string k_HGraphDefaultNodeIconPath = PackageRuntimeDirPath + "Graph/Resources/NodeIcon.png";
        public const string k_HGraphErrorNodeIconPath = PackageRuntimeDirPath + "Graph/Resources/ErrorNodeIcon.png";
        public const string k_HGraphHasUserDataIconPath = PackageRuntimeDirPath + "Graph/Resources/HasUserDataIcon.png";

        public const string k_HGraphDefaultRelationPath = "Assets/Graph/Relation.prefab";
        public const string k_HGraphDefaultHGraphNodeInspectorPath = "Assets/Graph/HGraphNodeInspector.uxml";
        public const string k_HGraphDefaultHGraphNodeDrawerPath = "Assets/Graph/HGraphNodeDrawer.uxml";
        public const string k_HGraphDefaultHGraphRelationInspectorPath = "Assets/Graph/HGraphRelationInspector.uxml";

        public const string k_HGraphManagerWindowPath = "Assets/Editor/HGraph/HGraphManagerWindow.uxml";
        public const string k_HGraphAssetRecommenderWindowPath = "Assets/Editor/HGraph/HGraphAssetRecommenderWindow.uxml";
        public const string k_HGraphAssetManagerWindowPath = "Assets/Editor/HGraph/HGraphAssetManagerWindow.uxml";



    }


    // Use IMGUI for now
#if true

    // Register a SettingsProvider using IMGUI for the drawing framework:
    static class HGraphSettingsIMGUIRegister
    {
        [SettingsProvider]
        public static SettingsProvider CreateHGraphSettingsProvider()
        {
            // First parameter is the path in the Settings window.
            // Second parameter is the scope of this setting: it only appears in the Project Settings window.
            var provider = new SettingsProvider(HGraphSettingsData.k_HGraphProjectSettingsPath, SettingsScope.Project)
            {
                // By default the last token of the path is used as display name if no label is provided.
                label = HGraphSettingsData.k_HGraphSettingsName,
                // Create the SettingsProvider and initialize its drawing (IMGUI) function in place:
                guiHandler = (searchContext) =>
                {
                    var settings = HGraphSettings.GetSerializedSettings();
                    EditorGUI.BeginChangeCheck();
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultHNodeIcon"), new GUIContent("Default HNode Icon"));
                    EditorGUILayout.PropertyField(settings.FindProperty("errorHNodeIcon"), new GUIContent("Error HNode Icon"));
                    EditorGUILayout.PropertyField(settings.FindProperty("hasUserDataIcon"), new GUIContent("Has User Data Icon"));

                    EditorGUILayout.PropertyField(settings.FindProperty("defaultRelation"), new GUIContent("Default Relation"));
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultHGraphNodeInspector"), new GUIContent("Default HGraphNode Inspector"));
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultHGraphNodeDrawer"), new GUIContent("Default HGraphNode Drawer"));
                    EditorGUILayout.PropertyField(settings.FindProperty("defaultHGraphRelationInspector"), new GUIContent("Default HGraphRelation Inspector"));

                    EditorGUILayout.PropertyField(settings.FindProperty("hGraphManagerWindow"), new GUIContent("HGraphManagerWindow"));
                    EditorGUILayout.PropertyField(settings.FindProperty("hGraphAssetRecommenderWindow"), new GUIContent("HGraphAssetRecommenderWindow"));
                    EditorGUILayout.PropertyField(settings.FindProperty("hGraphAssetManagerWindow"), new GUIContent("HGraphAssetManagerWindow"));
                    EditorGUILayout.PropertyField(settings.FindProperty("autoUpdatePCGGraph"), new GUIContent("Auto Update PCGGraph","Marks the PCGGraph as dirty each few seconds to allow generation with latest data."));


                    EditorGUILayout.PropertyField(settings.FindProperty("edgeColors"), new GUIContent("Edge Colors"), true);

                    var r = settings.ApplyModifiedPropertiesWithoutUndo();
                    if (r)
                    {
                        HGraphSettings.SaveSettings();
                    }
                    settings.Dispose();
                },

                // Populate the search keywords to enable smart search filtering and label highlighting:
                keywords = new HashSet<string>(new[] { "HGraph", "Default HNode Icon", "Some String", "Relation" })
            };

            return provider;
        }

    }

#endif

    // Ignore UIToolkit until default styles are easily accessible
#if false

// Register a SettingsProvider using UIElements for the drawing framework:
static class HGraphSettingsUIElementsRegister
{
    [SettingsProvider]
    public static SettingsProvider CreateHGraphSettingsProvider()
    {
        // First parameter is the path in the Settings window.
        // Second parameter is the scope of this setting: it only appears in the Settings window for the Project scope.
        var provider = new SettingsProvider(HGraphSettingsData.k_HGraphProjectSettingsPath, SettingsScope.Project)
        {
            label = HGraphSettingsData.k_HGraphSettingsName,
            // activateHandler is called when the user clicks on the Settings item in the Settings window.
            activateHandler = (searchContext, rootElement) =>
            {
                var settings = HGraphSettings.GetSerializedSettings();

                // rootElement is a VisualElement. If you add any children to it, the OnGUI function
                // isn't called because the SettingsProvider uses the UIElements drawing framework.
                var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(HGraphSettingsData.k_HGraphProjectSettingsStyleSheetPath);
                rootElement.styleSheets.Add(styleSheet);
                var title = new Label()
                {
                    text = HGraphSettingsData.k_HGraphSettingsName
                };
                title.AddToClassList("title");
                rootElement.Add(title);

                var properties = new VisualElement()
                {
                    style =
                    {
                        flexDirection = FlexDirection.Column
                    }
                };
                properties.AddToClassList("property-list");
                rootElement.Add(properties);

                properties.Add(new PropertyField(settings.FindProperty("defaultHNodeIcon"), "Default HNode Icon"));
                properties.Add(new PropertyField(settings.FindProperty("m_SomeString")));

                rootElement.Bind(settings);
            },

            // Populate the search keywords to enable smart search filtering and label highlighting:
            keywords = new HashSet<string>(new[] { "Default Node Icon", "Some String" })
        };

        return provider;
    }
}
#endif
}