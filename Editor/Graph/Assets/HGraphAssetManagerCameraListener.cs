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
