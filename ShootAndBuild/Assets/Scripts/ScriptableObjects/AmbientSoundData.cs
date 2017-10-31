using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    [CreateAssetMenu(menuName = "Custom/AmbientSoundData", fileName = "AmbientSoundData")]
    public class AmbientSoundData : ScriptableObject
    {
		[FormerlySerializedAs("audioClip")]
        [SerializeField] private AudioClip m_AudioClip;

		[FormerlySerializedAs("VolumeFactor")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_VolumeFactor = 1.0f;

		public AudioClip audioClip	{ get { return m_AudioClip; } }
		public float volumeFactor	{ get { return m_VolumeFactor; } }
	}
}