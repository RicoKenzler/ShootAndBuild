﻿using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnManagerPrototype : MonoBehaviour
	{
		[FormerlySerializedAs("waves")]
		[SerializeField] private List<SpawnWave> m_Waves = new List<SpawnWave>();

        ///////////////////////////////////////////////////////////////////////////

		private int m_WaveIndex = 0;

		///////////////////////////////////////////////////////////////////////////

		public List<SpawnWave> waves { get { return m_Waves; } }
		
        ///////////////////////////////////////////////////////////////////////////


		void Awake()
		{
			instance = this;
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			if (m_WaveIndex >= m_Waves.Count)
			{
				return;
			}

			SpawnWave wave = m_Waves[m_WaveIndex];

			if (!wave.IsCompleted)
			{
				wave.Update();
			}

			if (wave.IsCompleted)
			{
				m_WaveIndex++;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public static SpawnManagerPrototype instance
		{
			get; private set;
		}
	}
}