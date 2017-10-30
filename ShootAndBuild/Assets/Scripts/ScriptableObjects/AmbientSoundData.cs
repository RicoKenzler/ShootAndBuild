using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    [CreateAssetMenu(menuName = "Custom/AmbientSoundData", fileName = "AmbientSoundData")]
    public class AmbientSoundData : ScriptableObject
    {
        public AudioClip audioClip;

        [Range(0.0f, 1.0f)]
        public float VolumeFactor = 1.0f;
    }
}