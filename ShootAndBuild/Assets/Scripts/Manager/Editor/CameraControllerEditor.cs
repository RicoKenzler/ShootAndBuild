using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
	[CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            CameraController cameraController = (CameraController)target;

            List<string> hideFields = new List<string>();
            DrawPropertiesExcluding(serializedObject, hideFields.ToArray());
            serializedObject.ApplyModifiedProperties();

			GUILayout.Space(20);

            // Some things Manager only exist (and works) during play
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("AddShake"))
            {
                cameraController.AddCameraShake(cameraController.testShakeParams);
            }
		
			GUILayout.Label("ShakeOffset: "		+ cameraController.GetCameraShakeOffset());
			GUILayout.Label("ShakeStrength: "	+ Vector3.Magnitude(cameraController.GetCameraShakeOffset()));

			// repaint every frame
			EditorUtility.SetDirty(target);

            GUI.enabled = true;
        }
    }
}
