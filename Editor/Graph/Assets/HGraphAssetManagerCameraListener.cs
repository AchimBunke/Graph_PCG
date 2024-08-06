﻿/*
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

namespace Achioto.Gamespace_PCG.Editor.Graph.Assets
{
    [InitializeOnLoad]
    public class HGraphAssetManagerCameraListener
    {
        public static Vector3 LastCameraPosition;

        public static Quaternion LastCameraRotation;
        static HGraphAssetManagerCameraListener()
        {
            SceneView.duringSceneGui += SceneView_duringSceneGui;
            LastCameraPosition = GetCurrentPosition();
            LastCameraRotation = GetCurrentRotation();
        }
        public HGraphAssetManagerCameraListener()
        {

        }

        private static Vector3 GetCurrentPosition()
        {
            return SceneView.lastActiveSceneView?.camera?.transform?.position ?? Vector3.zero;
        }
        private static Quaternion GetCurrentRotation()
        {
            return SceneView.lastActiveSceneView?.camera?.transform?.rotation ?? Quaternion.identity;
        }
        private static void SceneView_duringSceneGui(SceneView obj)
        {
            Vector3 currentCameraPosition = GetCurrentPosition();
            Quaternion currentCameraRotation = GetCurrentRotation();

            if (currentCameraPosition != LastCameraPosition)
            {
                // Your custom reaction to the camera transform changing
                //Debug.Log("Camera Position Changed");

                // Update the last camera position and rotation
                LastCameraPosition = currentCameraPosition;


            }
            if(currentCameraRotation != LastCameraRotation)
            {
               // Debug.Log("Camera Rotation Changed");
                LastCameraRotation = currentCameraRotation;
            }
        }
    }
}
