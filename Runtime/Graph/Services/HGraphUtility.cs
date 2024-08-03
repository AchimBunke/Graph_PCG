using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space.Services;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    public static class HGraphUtility
    {
        public static bool AutoConnectSuperNode { get; set; } = true;

        public static HGraphNode FindSuperNode(HGraphSceneNode node)
            => FindSuperSpace(node)?.SceneNode?.NodeData.Value;
        public static HGraphNodeSpace FindSuperSpace(HGraphSceneNode node)
        {
            var nodeSpace = node.GetComponent<HGraphNodeSpace>();
            if(nodeSpace == null || nodeSpace != null)// maybe change space search if searching for parent space of a space and not of a position
            {
                var superSpace = HGraphSpaceUtility.FindSpace(node);
                return superSpace;
            }
            return null;
        }

        public static string CreateGraphFilePathFromCurrentEditorScene()
        {
            var sceneName = EditorSceneManager.GetActiveScene().name;
            var scenePath = EditorSceneManager.GetActiveScene().path;
            string sceneFullPath = Path.Combine(Application.dataPath, "..", scenePath);
            string sceneDirectory = Path.GetDirectoryName(sceneFullPath);
            return Path.Combine(sceneDirectory, Path.ChangeExtension(sceneName, "graph.json"));
        }
        public static string CreateGraphFilePathFromScene(UnityEngine.SceneManagement.Scene scene)
        {
            var sceneName = scene.name;
            var scenePath = scene.path;
            string sceneFullPath = Path.Combine(Application.dataPath, "..", scenePath);
            string sceneDirectory = Path.GetDirectoryName(sceneFullPath);
            return Path.Combine(sceneDirectory, Path.ChangeExtension(sceneName, "graph.json"));
        }

    }
}
