using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{

    /// <summary>
    /// Editor for HGraphNode.
    /// </summary>
    [CustomEditor(typeof(HGraphSceneNode_Autoconnect), editorForChildClasses: true)]
    public class HGraphSceneNode_AutoconnectEditor : HGraphSceneNodeEditor
    {
        public override VisualElement CreateInspectorGUI()
        {
            var v = base.CreateInspectorGUI();
            var parentProperty = serializedObject.FindProperty("_autoconnectSuperNode");
            var parentField = new PropertyField(parentProperty);
            parentField.BindProperty(parentProperty);
            v.Insert(0, parentField);
            return v;
        }
    }
}