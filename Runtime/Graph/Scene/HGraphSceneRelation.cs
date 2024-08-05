using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using Achioto.Gamespace_PCG.Runtime.Graph.Runtime.Services;
using Achioto.Gamespace_PCG.Runtime.Graph.Services;
using System;
using UniRx;
using UnityEngine;
using UnityUtilities.Reactive;
using UnityUtilities.UnityBase;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    /// <summary>
    /// Represents a Relation between 2 HGraphNodes.
    /// Direction does not matter.
    /// TODO: A Relation should have different attributes-edges each connecting the 2 nodes in different ways.
    /// Runs in EditMode
    /// </summary>
    [ExecuteInEditMode]
    public class HGraphSceneRelation : HGraphSceneComponent
    {
        private ReactiveProperty<HGraphSceneNode> _source = new();

        /// <summary>
        /// Source node.
        /// </summary>
        public IReadOnlyReactiveProperty<HGraphSceneNode> Source => _source;

        /// <summary>
        /// Serialized target node.
        /// </summary>
        private ReactiveProperty<HGraphSceneNode> _target = new();
        /// <summary>
        /// Target node.
        /// </summary>
        public IReadOnlyReactiveProperty<HGraphSceneNode> Target => _target;


        [NonSerialized]
        private ReactiveProperty<HGraphRelation> _relationData = new();
        public IReadOnlyReactiveProperty<HGraphRelation> RelationData => _relationData;


        /// <summary>
        /// Should be Editor Only!
        /// Displays connection by rendering lines. Advantage: User can click line to select relation.
        /// </summary>
        private LineRenderer lineRenderer;

        protected override void OnEnable()
        {
            //        HGraphId = GlobalObjectIdExtensions.GetGlobalObjectIdString(gameObject);
            //        // Add to HGraph
            //        HGraph_V1.Instance.Relations.Add(HGraphId, this);
#if UNITY_EDITOR
            // Get or Create LineRenderer and setup. Disable if PlayMode.
            if (lineRenderer == null)
                lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
            lineRenderer.positionCount = 2;
            // Enabling will enable and update LineRenderer
            if (lineRenderer != null)
            {
                UpdateLineSource();
                UpdateLineTarget();
            }
            if (Application.isPlaying)
                lineRenderer.enabled = false;
#endif
            //        // Option 1: Relation was Destroyed => Reconstructed and must attach themself to nodes again.
            //        // Option 2: Node was Destroyed => Relation was destroyed => Node reconstructed => relation reconstructe and must attach itself to the other node again.
            //        if (!_target?.Relations?.Contains(this) ?? false)
            //            _target.Relations.Add(this);
            //        if (!_source?.Relations?.Contains(this) ?? false)
            //            _source.Relations.Add(this);
            disableDisposables = new CompositeDisposable
            {
                _relationData.Subscribe(OnRelationDataChanged)
            };

            base.OnEnable();
            var hgraph = HGraph.Instance;
            disableDisposables.Add(hgraph.Relations.ObserveAdd().Where(kv => kv.Key == HGraphId.Value).Select(kv => kv.Value).Merge(
                     hgraph.Relations.ObserveRemove().Where(kv => kv.Key == HGraphId.Value).Select(_ => (HGraphRelation)null),
                     hgraph.Relations.ObserveReplace().Where(kv => kv.Key == HGraphId.Value).Select(_ => (HGraphRelation)null),
                     hgraph.Relations.ObserveReset().Select(_ => (HGraphRelation)null))
                 .Subscribe(SetRelationData));

        }
        protected override void OnHGraphIdChanged(string oldId, string newId)
        {
            gameObject.name = newId;
            var hGraph = HGraph.Instance;
            if (HGraphResources.IsHGraphIdValid(oldId))
                hGraph.SceneRelations.Remove(oldId);
            if (HGraphResources.IsHGraphIdValid(newId))
            {
                if (hGraph.SceneRelations.ContainsKey(newId))
                {
                    _isRegistered.Value = false;
                    _isDuplicate.Value = true;
                    SetRelationData(null);
                    return;
                }
                hGraph.SceneRelations.Add(newId, this);
                _isRegistered.Value = true;
                if (hGraph.Relations.TryGetValue(newId, out var relation))
                    SetRelationData(relation);
                else
                    SetRelationData(null);
            }
            else
            {
                _isRegistered.Value = false;
                SetRelationData(null);
            }
            _isDuplicate.Value = false;
        }
        CompositeDisposable disableDisposables;
        private void DisposeSubscribers()
        {
            disableDisposables?.Dispose();
        }
        private void SetRelationData(HGraphRelation data)
        {
            _relationData.Value?.Disconnect();
            _relationData.Value = data;
        }

        CompositeDisposable relationSourceTargetSubscriber;
        private void OnRelationDataChanged(HGraphRelation newData)
        {
            relationSourceTargetSubscriber?.Dispose();
            var hgraph = HGraph.Instance;
            if (newData == null)
            {
                _source.Value = null;
                _target.Value = null;
                return;
            }
            newData.Connect(this);
            relationSourceTargetSubscriber = new CompositeDisposable();
            if (HGraph.Instance.Nodes.TryGetValue(newData.Source.Value, out var source))
                relationSourceTargetSubscriber.Add(source.SceneNode.Subscribe(_source.AsObserver()));
            else
                _source.Value = null;
            if (HGraph.Instance.Nodes.TryGetValue(newData.Target.Value, out var target))
                relationSourceTargetSubscriber.Add(target.SceneNode.Subscribe(_target.AsObserver()));
            else
                _target.Value = null;
        }






        private void OnDisable()
        {
#if UNITY_EDITOR
            if (Application.isPlaying)
                return;
            // Enabling will disable LineRenderer
            lineRenderer.enabled = false;
#endif
            DisposeSubscribers();
        }
#if UNITY_EDITOR
        public void HideLine()
        {
            lineRenderer.enabled = false;
        }
        public void ShowLine()
        {
            UpdateLineSource();
            UpdateLineTarget();
            lineRenderer.enabled = true;
        }
        private void Update()
        {
            // Listen to transform changes and update line
            // Note: transform.hasChanged might be dangerous (its just a flag).
            if (_source.Value != null)
            {
                if (_source.Value.transform.hasChanged)
                {
                    UpdateLineSource();
                }
            }
            if (_target.Value != null)
            {
                if (_target.Value.transform.hasChanged)
                {
                    UpdateLineTarget();
                }
            }
        }
        private void LateUpdate()
        {
            // Reset hasChanged flag
            if (_source.Value != null)
                _source.Value.transform.hasChanged = false;
            if (_target.Value != null)
                _target.Value.transform.hasChanged = false;
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            // Update Line if properties changed
            if (lineRenderer != null)
            {
                UpdateLineSource();
                UpdateLineTarget();
            }
        }
        /// <summary>
        /// Update start position
        /// </summary>
        private void UpdateLineSource()
        {
            lineRenderer.SetPosition(0, _source.Value?.transform?.position ?? Vector3.zero);
        }
        /// <summary>
        /// Update end position
        /// </summary>
        private void UpdateLineTarget()
        {
            lineRenderer.SetPosition(1, _target.Value?.transform?.position ?? Vector3.zero);
        }

#endif

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // Remove from HGraph
            DisposeSubscribers();
            if (HGraphResources.IsHGraphIdValid(HGraphId.Value))
                HGraph.Instance.SceneRelations.Remove(HGraphId.Value);
            RelationData.Value?.Disconnect();
            // Only remove Relations from node if destroyed by user.
            // TODO: This does currently not work for immediate Destroy->Undo->Redo because isLoaded is false on Redo. Produces error and reference errors.
            if (gameObject.scene.isLoaded)
            {
                if (RelationData.Value != null)
                    HGraphController.DestroyRelation(RelationData?.Value);

            }
        }
    }
}