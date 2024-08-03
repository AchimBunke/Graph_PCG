using Achioto.Gamespace_PCG.Runtime.Graph.Runtime;
using UnityEngine;

namespace Achioto.Gamespace_PCG.Runtime.Graph.Scene
{
    /// <summary>
    /// Functions as the parent for all HGraph objects that are not attached to actual assets.
    /// </summary>
    [ExecuteInEditMode]
    public class HGraphSceneRoot : MonoBehaviour
    {
        private void OnEnable()
        {
            var hgraph = HGraph.Instance;
        }
    }
}
