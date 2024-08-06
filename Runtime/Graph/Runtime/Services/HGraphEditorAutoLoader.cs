using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Serialization.Services;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services
{
    [ExecuteInEditMode]
    public class HGraphEditorAutoLoader : MonoBehaviour
    {
        [SerializeField, ReadOnlyField] string graphPath;
        [SerializeField, Tooltip("Prevents unloading currently active graph")] bool keepCurrentGraph = false;
        [SerializeField, Tooltip("Loading will update existing values inside the graph")] bool updateData = true;
        private void OnEnable()
        {
            graphPath = HGraphUtility.CreateGraphFilePathFromScene(gameObject.scene);
            LoadGraph();
        }

        public void LoadGraph()
        {
            var data = HGraphSerializationController.DeserializeFromFile(graphPath);
            HGraphController.Load(data, keepCurrentGraph, updateData);
        }
    }
}