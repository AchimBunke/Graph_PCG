using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Achioto.Gamespace_PCG.Samples.Resources
{
    [ExecuteAlways]
    public class Enemy : MonoBehaviour
    {
        [SerializeField]
        float _health = -1;
        public float Health
        {
            get => _health;
            set
            {
                _health = value;
                if (textmesh != null)
                    textmesh.text = Mathf.RoundToInt(_health).ToString() + " HP";
            }
        }
        [SerializeField] TextMesh textmesh;

        public float DifficultyLevel { get; set; }

#if UNITY_EDITOR
        private void Update()
        {
            var cam = SceneView.lastActiveSceneView.camera;
            textmesh.transform.LookAt(cam.transform.position);
            textmesh.transform.Rotate(0, 180, 0);
        }
#endif
    }
}
