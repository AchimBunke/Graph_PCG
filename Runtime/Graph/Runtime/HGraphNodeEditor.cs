using UnityEditor;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Runtime
{

    [CustomEditor(typeof(HGraphNode))]
    public class HGraphNodeEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
        public override UnityEngine.UIElements.VisualElement CreateInspectorGUI()
        {
            return null;
        }
    }
}

