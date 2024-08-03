using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using UnityEditor.SceneManagement;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    public class HGraphSceneNode_Autoconnect : HGraphSceneNode
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            string id;
            if (PrefabStageUtility.GetCurrentPrefabStage() != null)
                return;
            if (HGraphResources.IsHGraphIdValid(HGraphId.Value) && !IsDuplicate.Value)
            {
                // already initialized.. 
                id = HGraphId.Value;
            }
            else
            {
                id = GetInstanceID().ToString();
            }
            var name = gameObject.name;
            HGraphId.Value = id.ToString();
            if (!IsHGraphConnected)
            {
                //Debug.Log("Node not connected to graph.. creating new node");
                ConnectAsNewNode();
                NodeData.Value.Name.Value = name;
            }
        }
        protected override void OnValidate()
        {
            base.OnValidate();
        }
    }
}

