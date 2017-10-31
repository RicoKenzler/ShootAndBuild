using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SAB
{
	public class AudioManager : MonoBehaviour
	{
		[SerializeField] private GameObject			m_OneShotPrefab;

		[SerializeField] private AudioMixer			m_AudioMixer;
						 private AudioMixerGroup	m_SoundGroup;

		///////////////////////////////////////////////////////////////////////////

		public static AudioManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
			m_SoundGroup = m_AudioMixer.FindMatchingGroups("Sound")[0];
		}

		///////////////////////////////////////////////////////////////////////////
		
		public float SemitoneToPitch(float semitone)
		{
			float pitch = Mathf.Pow(2.0f, (semitone / (float) 12.0f));
			return pitch;
		}

		///////////////////////////////////////////////////////////////////////////

		public float RandomPitchFromSemitones(int[] semitones)
		{
			if (semitones.Length == 0)
			{
				return 1.0f;
			}

			int rndSoundIndex = Random.Range(0, semitones.Length);
			int halftone = semitones[rndSoundIndex];

			return SemitoneToPitch(halftone);
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetRandomMusicalPitch()
		{
			int[] niceSemitones = { 0, 2, 4, 5, 7, 9, 11, 12,  0, 4, 7, 12, 0, 7,   5};
			return RandomPitchFromSemitones(niceSemitones);
		}

		///////////////////////////////////////////////////////////////////////////

		public AudioSource PlayAudio(AudioData audioData, Vector3? position3D = null, float? overridePitch = null)
		{
			if (CheatManager.instance.disableAudio)
			{
				return null;
			}

			if (!audioData || audioData.audioClips.Length <= 0)
			{
				return null;
			}

			int rndClipIndex = Random.Range(0, audioData.audioClips.Length);
			AudioClip rndClip = audioData.audioClips[rndClipIndex];

			if (rndClip == null)
			{
				return null;
			}

			bool playUISound = audioData.isUISound;

			GameObject audioObject = Instantiate(m_OneShotPrefab, gameObject.transform);
			audioObject.name = "OneShot " + rndClip.name;

			if (position3D.HasValue)
			{
				audioObject.transform.position = position3D.Value;
			}
			else
			{
				gameObject.transform.position = Vector3.zero;
				playUISound = true;
			}

			AudioSource audioSource				= audioObject.GetComponent<AudioSource>();
			audioSource.clip					= rndClip;
			audioSource.volume					= audioData.volume;
			audioSource.dopplerLevel			= (playUISound || audioData.suppressDoppler) ? 0.0f : 1.0f;
			audioSource.spatialBlend			= playUISound ? 0.0f : audioData.amount3D;
			audioSource.outputAudioMixerGroup	= m_SoundGroup;

			if (overridePitch.HasValue)
			{
				audioSource.pitch = overridePitch.Value;
			}
			else
			{
				if (audioData.rndMusicalPitch)
				{
					audioSource.pitch = GetRandomMusicalPitch();
				}
				else
				{
					float semitoneOffsets = audioData.pitchOffsetSemitones;
					float additionalOffset = Random.Range(-audioData.pitchRangeSemitones, audioData.pitchRangeSemitones);

					audioSource.pitch = SemitoneToPitch(semitoneOffsets + additionalOffset);
				}
			}
		
			audioSource.Play();

			Destroy(audioObject, rndClip.length);

			return audioSource;
		}
	}
}