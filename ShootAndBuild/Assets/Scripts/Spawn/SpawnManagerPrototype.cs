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

		[SerializeField] private AudioData m_NewWaveSound;
		[SerializeField] private AudioData m_FinishedWaveSound;

        ///////////////////////////////////////////////////////////////////////////

		private int m_WaveIndex		= 0;

		///////////////////////////////////////////////////////////////////////////

		public AudioData newWaveSound					{ get { return m_NewWaveSound; } }
		public AudioData finishedWaveSound				{ get { return m_FinishedWaveSound; } }
		public List<SpawnWave> waves					{ get { return m_Waves; } }
		public static SpawnManagerPrototype instance	{ get; private set; }
		public int waveIndex							{ get { return m_WaveIndex; } }
		public int waveCount							{ get { return waves.Count; } }

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

			if (!PlayerManager.instance.HasPlayerJoined(PlayerID.Player1))
			{
				// No Waves until player is ready
				return;
			}

			SpawnWave wave = m_Waves[m_WaveIndex];

			if (!wave.isCompleted)
			{
				wave.Update();
			}

			if (wave.isCompleted)
			{
				m_WaveIndex++;
			}
		}
	}
}