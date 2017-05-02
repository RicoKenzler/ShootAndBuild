using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace SAB
{
	[System.Serializable]
	public struct ModularTrack
	{
		public AudioClip BaseTrack;
		public AudioClip CalmTrack;
		public AudioClip CombatTrack;

		public float loopStart;
		public float loopEnd;
	}

	//-------------------------------------------------

	public class MusicManager : MonoBehaviour 
	{
		public ModularTrack musicTrack;
		public AudioSource	audioSource;
		//-------------------------------------------------

		void Awake()
		{
			instance = this;
		}

		//-------------------------------------------------

		void Start() 
		{
			audioSource = GetComponent<AudioSource>();

			if (!CheatManager.instance.disableMusic)
			{
				audioSource.clip = musicTrack.BaseTrack;
				audioSource.Play();
			}
		}
	
		//-------------------------------------------------

		void Update() 
		{
			
		}

		//-------------------------------------------------

		public static MusicManager instance
		{
			get; private set;
		}
	}

}