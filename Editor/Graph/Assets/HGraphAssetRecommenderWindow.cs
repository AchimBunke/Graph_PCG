using Achioto.Gamespace_PCG.Runtime.Graph.Distance;
using Achioto.Gamespace_PCG.Runtime.Graph.FeatureEncoding;
using Achioto.Gamespace_PCG.Runtime.Graph.Interpolation;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Editor.Graph.Assets
{
    public class HGraphAssetRecommenderWindow : EditorWindow
    {
        static SerializedObject serializedSelf;

        [MenuItem("Window/HGraph/HGraph Asset Recommender", priority = 4000, secondaryPriority = 0)]
        public static void ShowWindow()
        {
            EditorWindow.GetWindow<HGraphAssetRecommenderWindow>("HGraph Asset Recommender");
        }

        private List<AssetData> items;

        [SerializeField] private HGraphInterpolationConfiguration interpolationConfiguration;
        [SerializeField] private SpatialDistanceMeasureConfiguration spatialDistanceConfiguration;
        [SerializeField] private FeatureDistanceMeasureConfiguration featureDistanceConfiguration;
        [SerializeField] private HGraphSpaceSearchSettings spaceSearchSettings = HGraphSpaceSearchSettings.Default;

        private VisualElement MakeAssetEntry()
        {
            var v = new VisualElement();
            v.AddToClassList("assetEntry");
            var image = new VisualElement()
            {
                name = "image"
            };
            image.AddToClassList("assetEntry-image");
            v.Add(image);
            var title = new VisualElement();
            title.AddToClassList("assetEntry-title");
            var icon = new VisualElement()
            {
                name = "icon",
            };
            title.Add(icon);
            icon.AddToClassList("assetEntry-icon");
            var label = new Label()
            {
                name = "label"
            };
            label.AddToClassList("assetEntry-label");
            title.Add(label);
            v.Add(title);
            var distance = new Label()
            {
                name = "distance",
                text = "NaN",
            };
            label.AddToClassList("assetEntry-label");
            v.Add(distance);

            return v;
        }
        private void BindAssetEntry(VisualElement e, int index)
        {
            var item = items[index];
            e.Q("image").style.backgroundImage = item.AssetPreview;
            e.Q<Label>("label").text = item.AssetName;
            e.Q("icon").style.backgroundImage = item.AssetMiniThumbnail;
            e.Q<Label>("distance").text = "Dist: " + item.CurrentDistance.ToString();
        }
        private struct AssetData
        {
            public Texture2D AssetPreview;
            public Texture2D AssetMiniThumbnail;
            public string AssetPath;
            public Object Asset;
            public string AssetName;
            public HGraphAssetData assetHGraphData;
            public double CurrentDistance;
            public static AssetData Create(Object obj)
            {
                var path = AssetDatabase.GetAssetPath(obj);
                var userData = AssetImporter.GetAtPath(path).userData;
                if (!HGraphSerializationController.TryDeserializeAssetData(userData, out var assetData))
                {
                    assetData = new HGraphAssetData() { attributes = new HGraphAttributeData[] { } };
                }
                var data = new AssetData
                {
                    AssetPreview = UnityEditor.AssetPreview.GetAssetPreview(obj),
                    AssetMiniThumbnail = UnityEditor.AssetPreview.GetMiniThumbnail(obj),
                    Asset = obj,
                    AssetPath = path,
                    AssetName = obj.name,
                    assetHGraphData = assetData,
                };
                return data;
            }
        }

        private ListView assetListView;
        private void CreateGUI()
        {
            VisualTreeAsset visualTree = HGraphSettings.GetOrCreateSettings().HGraphAssetRecommenderWindow;
            visualTree.CloneTree(rootVisualElement);

            serializedSelf = new SerializedObject(this);
            serializedSelf.Update();

            items = new List<AssetData>();
            LoadAssets();
            SortAssets();

            //var lvContainer = rootVisualElement.Q("MultiColumnListViewContainer");
            assetListView = rootVisualElement.Q<ListView>("AssetListView");
            assetListView.itemsSource = items;
            assetListView.makeItem = MakeAssetEntry;
            assetListView.bindItem = BindAssetEntry;

            assetListView.selectionChanged += AssetListView_selectionChanged;
            assetListView.itemsChosen += AssetListView_itemsChosen;
            assetListView.RefreshItems();

            assetListView.RegisterCallback<MouseDownEvent>(e =>
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.objectReferences = assetListView.selectedItems.Cast<AssetData>().Select(a => a.Asset).ToArray();
                Selection.objects = DragAndDrop.objectReferences;
                DragAndDrop.StartDrag("Dragging");
            });

            rootVisualElement.Q<Button>("Reload").clicked += () =>
            {
                items.Clear();
                LoadAssets();
                SortAssets();
                assetListView.RefreshItems();
            };

            rootVisualElement.Q<Button>("Test").clicked += () =>
            {
                var measure = new EuclideanFeatureDistance();
                var vals = HGraph.Instance.Nodes.Values.ToArray();
                for (int i = 0; i < vals.Length; ++i)
                {
                    for (int z = i + 1; z < vals.Length; ++z)
                    {
                        var v1 = FeatureVectorUtil.CreateFeatureVector(vals[i], normalized: true);
                        var v2 = FeatureVectorUtil.CreateFeatureVector(vals[z], normalized: true);
                        Debug.Log(v1.ToString() + " <--> " + v2.ToString() + " = " + measure.FeatureDistance(v1, v2));
                    }
                }
            };

            var configField = rootVisualElement.Q<PropertyField>("InterpolationConfig");
            configField.BindProperty(serializedSelf.FindProperty(nameof(interpolationConfiguration)));

            var spatialDistField = rootVisualElement.Q<PropertyField>("SpatialDistanceConfig");
            spatialDistField.BindProperty(serializedSelf.FindProperty(nameof(spatialDistanceConfiguration)));

            var featureDistField = rootVisualElement.Q<PropertyField>("FeatureDistanceConfig");
            featureDistField.BindProperty(serializedSelf.FindProperty(nameof(featureDistanceConfiguration)));

            var spaceSearchField = rootVisualElement.Q<PropertyField>("SpaceSearchSettings");
            spaceSearchField.BindProperty(serializedSelf.FindProperty(nameof(spaceSearchSettings)));
        }


        private void LoadAssets()
        {
            foreach (var assetKv in HGraphAssetRegistry.Instance.Assets)
            {
                var go = AssetDatabase.LoadAssetAtPath<Object>(AssetDatabase.GUIDToAssetPath(assetKv.Key));
                var data = AssetData.Create(go);
                items.Add(data);
            }
        }
        private void SortAssets()
        {
            var spaceSearch = new HGraphSpaceSearch(spaceSearchSettings);
            var interpolationMethodImpl = interpolationConfiguration.Create();
            var spatialDistanceMeasureImpl = spatialDistanceConfiguration.Create();
            var featureDistanceMeasureImpl = featureDistanceConfiguration.Create();

            var currentPosition = HGraphAssetManagerCameraListener.LastCameraPosition;
            var nearbyNodes = spaceSearch.FindNearbyNodes(currentPosition);
            interpolationMethodImpl.NodeSource = nearbyNodes.Nodes;
            var normalizedInterpolatedFeature = interpolationMethodImpl.InterpolateFeatures(currentPosition, spatialDistanceMeasureImpl, true);
            var un_normalizedInterpolatedFeature = interpolationMethodImpl.InterpolateFeatures(currentPosition, spatialDistanceMeasureImpl, false);
            for (int i = 0; i < items.Count(); ++i)
            {
                var item = items[i];
                if (item.assetHGraphData != null)
                    item.CurrentDistance = featureDistanceMeasureImpl.FeatureDistance(normalizedInterpolatedFeature, FeatureVectorUtil.CreateFeatureVector(item.assetHGraphData, true));
                else
                    item.CurrentDistance = double.MaxValue;
                items[i] = item;
            }
            items.Sort((a, b) =>
            {
                return a.CurrentDistance.CompareTo(b.CurrentDistance);
            });
        }

        private void AssetListView_selectionChanged(IEnumerable<object> obj)
        {
            Debug.Log("SelectionChanged; Object-count: " + obj.Count());
        }

        private void AssetListView_itemsChosen(IEnumerable<object> obj)
        {
            Debug.Log("Chosen; Object-count: " + obj.Count());
        }
    }
}