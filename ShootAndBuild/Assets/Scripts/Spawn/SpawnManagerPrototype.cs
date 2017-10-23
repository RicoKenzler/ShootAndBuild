using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnManagerPrototype : MonoBehaviour
	{
		public List<SpawnWave> waves = new List<SpawnWave>();

		private int waveIndex = 0;

		//----------------------------------------------------------------------

		void Awake()
		{
			instance = this;
		}

		//----------------------------------------------------------------------

		void Update()
		{
			if (waveIndex >= waves.Count)
			{
				return;
			}

			SpawnWave wave = waves[waveIndex];

			if (!wave.IsCompleted)
			{
				wave.Update();
			}

			if (wave.IsCompleted)
			{
				waveIndex++;
			}
		}

		//----------------------------------------------------------------------

		public static SpawnManagerPrototype instance
		{
			get; private set;
		}
	}
}