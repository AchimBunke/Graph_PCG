using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services
{
    [ExecuteInEditMode]
    public class SpaceSearchTest : MonoBehaviour
    {
        public HGraphSpaceSearchSettings SearchSettings = HGraphSpaceSearchSettings.Default;
        public HGraphSceneNode[] FoundNodes;
        public void Run()
        {
            var search = new HGraphSpaceSearch(SearchSettings);
            var result = search.FindNearbyNodes(GetComponent<HGraphSceneNode>().NodeData.Value);
            FoundNodes = result.Nodes.Select(f => f.SceneNode.Value).ToArray();
        }

    }
    [CustomEditor(typeof(SpaceSearchTest))]
    public class SpaceSearchTestEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Run"))
            {
                var t = (SpaceSearchTest)target;
                t.Run();
            }
        }
    }
}
