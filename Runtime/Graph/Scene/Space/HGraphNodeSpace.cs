using System.Linq;
using UnityEngine;
using UnityUtilities.Attributes;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene.Space
{
    [ExecuteAlways]
    [RequireComponent(typeof(HGraphSceneNode))]
    public class HGraphNodeSpace : Space
    {

        [SerializeField, Tooltip("This node does not have an explicit space but its space is instead the collection of subspaces")] bool _implicitSpace;
        [SerializeField, ShowIfBool(nameof(_implicitSpace), false, false)] Space _space;
        public Space Space => _space;
        public bool ImplicitSpace => _implicitSpace;
        /// <summary>
        /// Returns if a given point is inside a space but not inside a subspace!
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public override bool IsPointInsideSpace(Vector3 point)
        {
            var node = GetComponent<HGraphSceneNode>();
            if (_implicitSpace)
                return false;
            if (_space.IsPointInsideSpace(point))
            {
                if (!node.IsHGraphConnected)
                    return true;
                // check if in subspace instead
                foreach (var child in node.NodeData.Value.GetChildren().Select(c => c.SceneNode.Value))
                {
                    if (child == null)
                        continue;
                    var subSpace = child.GetComponent<HGraphNodeSpace>();
                    if (subSpace == null)
                        continue;
                    if (subSpace.IsPointInsideSpaceOrSubspace(point, out _))// Check Space directly otherwise complications with subspaces inside subspaces
                        return false;
                }
                return true;
            }
            return false;
        }
        public override Bounds ApproximateBounds => _space?.ApproximateBounds ?? default;

        public override float Distance(Vector3 point, out Vector3 closestPoint)
        {
            if (!ImplicitSpace)
                return _space.Distance(point, out closestPoint);

            float minDist = float.MaxValue;
            closestPoint = default;
            foreach (var child in SceneNode.NodeData.Value.GetChildren().Select(c => c.SceneNode.Value))
            {
                if (child == null)
                    continue;
                var subSpace = child.GetComponent<HGraphNodeSpace>();
                if (subSpace == null)
                    continue;
                var d = subSpace.Distance(point, out var cp);
                if (d < minDist)
                {
                    closestPoint = cp;
                    minDist = d;
                }
            }
            return minDist;
        }
        public HGraphSceneNode SceneNode => GetComponent<HGraphSceneNode>();
        public bool IsPointInsideSpaceOrSubspace(Vector3 point, out HGraphNodeSpace space)
        {
            var node = GetComponent<HGraphSceneNode>();
            space = null;

            if (_implicitSpace)
            {
                if (CheckChildrenForPoint(node, point, out space))
                    return true;
                return false;
            }
            if (_space.IsPointInsideSpace(point))
            {
                if (!node.IsHGraphConnected)
                    return true;
                // check if in subspace instead
                if (CheckChildrenForPoint(node, point, out space))
                    return true;
                else
                    space = this;
                return true;
            }
            return false;
        }
        private bool CheckChildrenForPoint(HGraphSceneNode node, Vector3 point, out HGraphNodeSpace space)
        {
            foreach (var child in node.NodeData.Value.GetChildren().Select(c => c.SceneNode.Value))
            {
                if (child == null)
                    continue;
                var subSpace = child.GetComponent<HGraphNodeSpace>();
                if (subSpace == null)
                    continue;
                if (subSpace._implicitSpace)
                {
                    if (subSpace.IsPointInsideSpaceOrSubspace(point, out space))
                        return true;
                }
                else if (subSpace.Space?.IsPointInsideSpace(point) ?? false)
                {
                    space = subSpace;
                    return true;
                }
            }
            space = default;
            return false;
        }

        private void OnEnable()
        {
            SceneNode.NodeSpace = this;
        }



    }
}