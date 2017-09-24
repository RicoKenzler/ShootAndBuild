using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	enum AmbientSoundTypes
	{
		Grass,
		Water,
	}

	public class AmbientSoundManager : MonoBehaviour 
	{
		private AudioSource AudioSourceOld;
		private AudioSource AudioSourceNew;

		void Start () 
		{
			const int sourceCount = 2;
			AudioSource[] audioSources = new AudioSource[sourceCount];

			Debug.Assert(gameObject.GetComponent<AudioSource>() == null);

			for (int i = 0; i < sourceCount; ++i)
			{
				audioSources[i] = gameObject.AddComponent<AudioSource>();
			}

			AudioSourceOld = audioSources[0];
			AudioSourceNew = audioSources[1];
		}
		
		void Update () 
		{
			
		}
	}
}