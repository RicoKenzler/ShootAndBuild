using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    [CreateAssetMenu(menuName = "Custom/AudioData", fileName = "AudioData")]
    public class AudioData : ScriptableObject
    {
		[FormerlySerializedAs("audioClips")]
        [SerializeField] private AudioClip[] m_AudioClips;

		[FormerlySerializedAs("volume")]
        [Range(0.0f, 10.0f)]
        [SerializeField] private float m_Volume = 1.0f;

        public enum PitchMode
        {
            Default,
            MusicalRandom,
            Mario,
            Mozart
        }

		[FormerlySerializedAs("rndMusicalPitch")]
        [SerializeField] private bool m_RndMusicalPitch = false;

		[FormerlySerializedAs("pitchOffsetSemitones")]
        [Range(-24.0f, 24.0f)]
        [SerializeField] private float m_PitchOffsetSemitones = 0.0f;

		[FormerlySerializedAs("pitchRangeSemitones")]
        [Range(0.0f, 24.0f)]
        [SerializeField] private float m_PitchRangeSemitones = 2.0f;

		[FormerlySerializedAs("isUISound")]
        [SerializeField] private bool m_IsUISound = false;

		[FormerlySerializedAs("suppressDoppler")]
        [SerializeField] private bool m_SuppressDoppler = false;

		[FormerlySerializedAs("amount3D")]
        [Range(0.0f, 1.0f)]
        [SerializeField] private float m_Amount3D = 1.0f;

		public AudioClip[]	audioClips				{ get { return m_AudioClips; } }
		public float		volume					{ get { return m_Volume; } }
		public bool			rndMusicalPitch			{ get { return m_RndMusicalPitch; } }
		public float		pitchOffsetSemitones	{ get { return m_PitchOffsetSemitones; } }
		public float		pitchRangeSemitones		{ get { return m_PitchRangeSemitones; } }
		public bool			isUISound				{ get { return m_IsUISound; } }
		public bool			suppressDoppler			{ get { return m_SuppressDoppler; } }
		public float		amount3D				{ get { return m_Amount3D; } }
	};
		
}