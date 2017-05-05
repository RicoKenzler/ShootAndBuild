using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace SAB
{
	[System.Serializable]
	public struct ModularTrack
	{
		public AudioClip BaseTrack;
		public AudioClip CalmTrack;
		public AudioClip CombatTrack;
	}

	//-------------------------------------------------

	public class MusicManager : MonoBehaviour 
	{
		private const float MusicGroupVolumeMuted   = -80.0f;
		public  float		MusicGroupVolumeDefault = 0.0f;

		private const float TrackVolumeMuted		= 0.0f;
		private const float TrackVolumeDefault		= 1.0f;

		public AudioMixer	audioMixer;
		public ModularTrack musicTrack;

		public float keepCombatStateDuration = 1.0f;

		private AudioSource	baseTrackSource;
		private AudioSource	calmSource;
		private AudioSource	combatSource;

		private float	lastCombatTime	= 0.0f;
		private bool	isInCombat		= false;


		private AudioMixerGroup soundGroup;

		//-------------------------------------------------

		void Awake()
		{
			instance = this; 
		}

		//-------------------------------------------------

		void Start() 
		{
			AudioSource[] audioSources = GetComponents<AudioSource>();

			soundGroup = audioMixer.FindMatchingGroups("Music")[0];

			if (audioSources.Length != 3)
			{
				Debug.LogWarning("Not enough audioSources");
				return;
			}
			
			baseTrackSource = audioSources[0];
			calmSource		= audioSources[1];
			combatSource	= audioSources[2];
			
			baseTrackSource.clip = musicTrack.BaseTrack;
			calmSource.clip		 = musicTrack.CalmTrack;
			combatSource.clip	 = musicTrack.CombatTrack;

			baseTrackSource.loop	= true;
			calmSource.loop			= true;
			combatSource.loop		= true;

			baseTrackSource.Play();
			calmSource.Play();
			combatSource.Play();
		}
	
		//-------------------------------------------------

		public void SignalIsInCombat()
		{
			lastCombatTime = Time.time;
			isInCombat = true;
		}

		//-------------------------------------------------

		void Update() 
		{
			if (CheatManager.instance.disableMusic)
			{
				soundGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeMuted);
			}
			else
			{
				soundGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeDefault);
			}

			TickCombatState();
			TickMoodFades();
		}

		//-------------------------------------------------

		private void TickMoodFades()
		{
			if (isInCombat)
			{
				calmSource.volume	= TrackVolumeMuted;
				combatSource.volume = TrackVolumeDefault;
			}
			else
			{
				combatSource.volume	= TrackVolumeMuted;
				calmSource.volume	= TrackVolumeDefault;
			}
		}

		//-------------------------------------------------

		void TickCombatState()
		{
			if (!isInCombat)
			{
				return;
			}

			if (Time.time > lastCombatTime + keepCombatStateDuration)
			{
				isInCombat = false;
			}
		}

		//-------------------------------------------------

		public static MusicManager instance
		{
			get; private set;
		}
	}

}