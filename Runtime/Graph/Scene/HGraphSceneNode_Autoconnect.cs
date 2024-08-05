using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    public class HGraphSceneNode_Autoconnect : HGraphSceneNode
    {
        [SerializeField] HGraphSceneNode _autoconnectSuperNode;
        public HGraphSceneNode AutoconnectSuperNode => _autoconnectSuperNode;
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
                if (_autoconnectSuperNode != null)
                    NodeData.Value.SuperNode.Value = _autoconnectSuperNode.HGraphId.Value;
            }
        }
        protected override void OnValidate()
        {
            base.OnValidate();
        }
    }
}

