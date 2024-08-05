using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Settings;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using UniRx;
using UnityEditor;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{

    /// <summary>
    /// Represents a Node in the HGraph.
    /// This class runs in editor and will Add/Remove from the HGraph when loaded/unloaded.
    /// </summary>
    [ExecuteAlways]
    public class HGraphSceneNode : HGraphSceneComponent
    {
        [field: NonSerialized]
        private ReactiveProperty<HGraphSceneNode> _superNode = new ReactiveProperty<HGraphSceneNode>();
        public IReadOnlyReactiveProperty<HGraphSceneNode> SuperNode => _superNode;

        [field: NonSerialized]
        private ReactiveProperty<HGraphNode> _nodeData = new();
        public IReadOnlyReactiveProperty<HGraphNode> NodeData => _nodeData;

        public bool IsHGraphConnected => NodeData.Value != null;

        public HGraphNodeSpace NodeSpace { get; set; }


        #region Gizmo

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            var settings = AssetDatabase.LoadAssetAtPath<HGraphSettings>(HGraphSettingsData.k_HGraphSettingsAssetPath);
            if (settings == null)
                Debug.LogError($"No HGraph settings found in: {HGraphSettingsData.k_HGraphSettingsAssetPath}. To fix open '{HGraphSettingsData.k_HGraphSettingsName}' in ProjectSettings.");
            Gizmos.DrawIcon(transform.position, GetGizmoIconPath(settings), true);
            Handles.Label(transform.position, GetGizmoLabelText(settings), GetGizmoLabelStyle(settings));
        }


        /// <summary>
        /// Get the gizmo-icon for this node
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        private string GetGizmoIconPath(HGraphSettings settings)
        {
            if (!IsRegistered.Value || !IsHGraphConnected)
                return AssetDatabase.GetAssetPath(settings.ErrorHNodeIcon);
            return AssetDatabase.GetAssetPath(settings.DefaultHNodeIcon);
        }
        /// <summary>
        /// Get the displayed name.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private string GetGizmoLabelText(HGraphSettings _)
        {
            if (NodeData.Value == null)
                return "<not connected>";
            var name = NodeData.Value?.Name?.Value;
            return string.IsNullOrEmpty(name) ? $"<{NodeData.Value.HGraphId.Value}>" : name;
        }
        /// <summary>
        /// Get the label style for this node.
        /// </summary>
        /// <param name="_"></param>
        /// <returns></returns>
        private GUIStyle GetGizmoLabelStyle(HGraphSettings _)
        {
            GUIStyle style = new GUIStyle();
            if (IsHGraphConnected)
                style.normal.textColor = Color.black;
            else
                style.normal.textColor = Color.red;
            return style;
        }
#endif

        #endregion

        #region GraphAttributes

#if UNITY_EDITOR

        public int GetDepth() => (SuperNode.Value?.GetDepth() ?? -1) + 1;
        private bool IsDecendantOf(HGraphSceneNode node, HashSet<HGraphSceneNode> visitedNodes)
        {
            HGraphSceneNode superNode = SuperNode.Value;
            while (superNode != null)
            {
                if (superNode == node)
                    return true;
                if (visitedNodes.Contains(superNode))
                    return false;
                visitedNodes.Add(superNode);
                superNode = superNode.SuperNode.Value;
            }
            return false;
        }
        public bool IsDescendantOf(HGraphSceneNode node)
        {
            return IsDecendantOf(node, new HashSet<HGraphSceneNode>());
        }
        protected override void OnValidate()
        {
            base.OnValidate();
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            NodeSpace = GetComponent<HGraphNodeSpace>();
            disableDisposables = new CompositeDisposable
            {
                NodeData.Subscribe(OnNodeDataChanged)
            };
            // HGraph Initialization
            var hgraph = HGraph.Instance;
            disableDisposables.Add(hgraph.Nodes.ObserveAdd().Where(kv => kv.Key == HGraphId.Value).Select(kv => kv.Value).Merge(
                    hgraph.Nodes.ObserveRemove().Where(kv => kv.Key == HGraphId.Value).Select(_ => (HGraphNode)null),
                    hgraph.Nodes.ObserveReplace().Where(kv => kv.Key == HGraphId.Value).Select(kv => kv.NewValue),
                    hgraph.Nodes.ObserveReset().Select(_ => (HGraphNode)null))
                .Subscribe(SetNodeData));
        }
        private void OnDisable()
        {
            DisposeSubscribers();
        }
        protected override void OnHGraphIdChanged(string oldId, string newId)
        {
            var hGraph = HGraph.Instance;
            if (HGraphResources.IsHGraphIdValid(oldId))
                hGraph.SceneNodes.Remove(oldId);

            if (HGraphResources.IsHGraphIdValid(newId))
            {
                // Duplicate Node
                if (hGraph.SceneNodes.ContainsKey(newId))
                {
                    _isRegistered.Value = false;
                    _isDuplicate.Value = true;
                    SetNodeData(null);
                    return;
                }
                hGraph.SceneNodes.Add(newId, this);
                _isRegistered.Value = true;
                if (hGraph.Nodes.TryGetValue(newId, out var node))
                    SetNodeData(node);
                else
                    SetNodeData(null);
            }
            else
            {
                _isRegistered.Value = false;
                SetNodeData(null);
            }
            _isDuplicate.Value = false;
        }

        CompositeDisposable disableDisposables;
        private void DisposeSubscribers()
        {
            disableDisposables?.Dispose();
        }

        CompositeDisposable _nodeDataSubscriber;
        private void DisposeNodeDataSubscribers()
        {
            _nodeDataSubscriber?.Dispose();
        }

        protected void SetNodeData(HGraphNode data)
        {
            _nodeData.Value?.Disconnect();
            _nodeData.Value = data;
        }

        private void OnNodeDataChanged(HGraphNode newData)
        {
            var hgraph = HGraph.Instance;
            if (newData == null)
            {
                DisposeNodeDataSubscribers();
                return;
            }
            newData.Connect(this);
            _nodeDataSubscriber = new CompositeDisposable()
        {
            newData.SuperNode.Subscribe(newSuperNode =>
            {
                if(HGraphResources.IsHGraphIdValid(newSuperNode) && hgraph.SceneNodes.ContainsKey(newSuperNode))
                    _superNode.Value = hgraph.SceneNodes[newData.SuperNode.Value];
                else
                    _superNode.Value = null;
            })
        };
        }


        /// <summary>
        /// Because this is running in Editor there are
        /// 2 Options: Destroyed by User or Scene unloaded
        /// </summary>
        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Remove from HGraph
            DisposeSubscribers();
            if (HGraphResources.IsHGraphIdValid(_hGraphId.Value))
            {
                HGraph.Instance.SceneNodes.Remove(_hGraphId.Value);
                _isRegistered.Value = false;
            }
            NodeData.Value?.Disconnect();
            // Only remove Relations if destroyed by user
            // TODO: This does currently not work for immediate Destroy->Undo->Redo because isLoaded is false on Redo. Produces error and reference errors.
            if (gameObject.scene.isLoaded)
            {
                //var toRemove = Relations.ToArray();
                //foreach (var rel in toRemove)
                //{
                //    //rel.Source.Relations.Remove(rel);
                //    //rel.Target.Relations.Remove(rel);

                //    Undo.DestroyObjectImmediate(rel.gameObject);
                //}
            }
        }

        public IEnumerable<HGraphSceneNode> GetCollidingNodes()
        {
            var hitColliders = Physics.OverlapSphere(transform.position, 0.1f, Physics.AllLayers, QueryTriggerInteraction.Collide);
            return hitColliders.Where(c => c.gameObject != gameObject && (c.GetComponent<HGraphSceneNode>()?.IsHGraphConnected ?? false)).Select(c => c.GetComponent<HGraphSceneNode>());
        }

        /// <summary>
        /// Creates and connects as a new node if not already connected
        /// </summary>
        public void ConnectAsNewNode()
        {
            if (this.IsHGraphConnected)
                return;
            var newNodeData = HGraphNode.Construct(HGraphId.Value);
            HGraph.Instance.Nodes.Add(newNodeData.HGraphId.Value, newNodeData);
            if (HGraphUtility.AutoConnectSuperNode)
            {
                var parentNode = HGraphUtility.FindSuperNode(this);
                if (parentNode != null)
                    NodeData.Value.SuperNode.Value = parentNode.HGraphId.Value;
            }
        }



#endif

        #endregion
    }
}

