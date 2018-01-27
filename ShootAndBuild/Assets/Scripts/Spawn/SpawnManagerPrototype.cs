using System;
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
		public int waveIndexHumanReadable				{ get { return m_WaveIndex + 1; }}
		public int waveCount							{ get { return waves.Count; } }

        ///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
		}

		///////////////////////////////////////////////////////////////////////////

		SpawnWave GetCurrentWave()
		{
			if (m_WaveIndex >= m_Waves.Count)
			{
				return null;
			}

			return m_Waves[m_WaveIndex];
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			SpawnWave curWave = GetCurrentWave();

			if (curWave == null)
			{
				return;
			}

			if (CheatManager.instance.pauseWaves)
			{
				return;
			}

			if (!PlayerManager.instance.HasPlayerJoined(PlayerID.Player1))
			{
				// No Waves until player is ready
				return;
			}

			if (!curWave.isCompleted)
			{
				curWave.Update();
			}

			if (curWave.isCompleted)
			{
				m_WaveIndex++;
			}
		}
		
		///////////////////////////////////////////////////////////////////////////

		public string GetDebugInfo()
		{
			SpawnWave curWave = GetCurrentWave();

			if (curWave == null)
			{
				return "No Wave Active...";
			}

			string waveString	= "[" + waveIndexHumanReadable		+ " / " + m_Waves.Count			+ "] Wave";
			string stageString	= "[" + (curWave.stageIndex + 1)	+ " / " + curWave.stageCount	+ "] Stage";

			SpawnWaveStage curStage = curWave.curStage;

			if (curStage != null)
			{
				stageString += "\n>> " + curStage.GetDebugInfo();
			}

			string outString = waveString + "\n" + stageString;

			return outString;
		}
	}
}