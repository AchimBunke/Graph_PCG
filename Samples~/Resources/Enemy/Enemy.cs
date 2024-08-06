/*
 * MIT License
 *
 * Copyright (c) 2024 Achim Bunke
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using UnityEditor;
using UnityEngine;

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
