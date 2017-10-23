using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SAB
{
    [CreateAssetMenu(menuName = "Custom/AmbientSoundData", fileName = "AmbientSoundData")]
    public class AmbientSoundData : ScriptableObject
    {
        public AudioClip audioClip;

        [Range(0.0f, 1.0f)]
        public float VolumeFactor = 1.0f;
    }

    [CustomEditor(typeof(AmbientSoundData))]
    public class AmbientSoundDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            AmbientSoundData audioData = (AmbientSoundData)target;

            List<string> hideFields = new List<string>();

            DrawPropertiesExcluding(serializedObject, hideFields.ToArray());
            serializedObject.ApplyModifiedProperties();
        }
    }
}