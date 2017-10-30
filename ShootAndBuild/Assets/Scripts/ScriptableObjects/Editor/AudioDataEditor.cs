using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
	[CustomEditor(typeof(AudioData))]
    public class AudioDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AudioData audioData = (AudioData)target;

            List<string> hideFields = new List<string>();

            if (audioData.isUISound)
            {
                hideFields.Add("suppressDoppler");
                hideFields.Add("amount3D");
            }

            if (audioData.rndMusicalPitch)
            {
                hideFields.Add("pitchRangeSemitones");
                hideFields.Add("pitchOffsetSemitones");
            }

            DrawPropertiesExcluding(serializedObject, hideFields.ToArray());
            serializedObject.ApplyModifiedProperties();

            // Audio Manager only exist (and works) during play
            GUI.enabled = Application.isPlaying;

            if (GUILayout.Button("Play near LookAt"))
            {
				Vector3 pos = CameraController.Instance.GetListenerPosition();
				pos.y = TerrainManager.Instance.GetInterpolatedHeight(pos.x, pos.z);

                pos += new Vector3(2.0f, 0.0f, 0.0f);
                AudioManager.instance.PlayAudio(audioData, pos);
            }

            GUI.enabled = true;
        }
    }
}
