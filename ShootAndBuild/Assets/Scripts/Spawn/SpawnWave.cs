using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnWave : ISerializationCallbackReceiver
	{
		[SerializeField] private List<SpawnWaveStage>		m_Stages			= new List<SpawnWaveStage>();
		[SerializeField] private List<MonsterSpawnStage>	m_SpawnStages		= new List<MonsterSpawnStage>();
		[SerializeField] private List<PauseStage>			m_PauseStages		= new List<PauseStage>();
		[SerializeField] private List<CompletionStage>		m_CompletionStages	= new List<CompletionStage>();
		[SerializeField] private List<RewardStage>			m_RewardStages		= new List<RewardStage>();

		///////////////////////////////////////////////////////////////////////////

		private int m_StageIndex = 0;

		///////////////////////////////////////////////////////////////////////////

		public List<SpawnWaveStage>		stages				{ get { return m_Stages;			 }}
		public List<MonsterSpawnStage>	spawnStages			{ get { return m_SpawnStages;		 }}
		public List<PauseStage>			pauseStages			{ get { return m_PauseStages;		 }}
		public List<CompletionStage>	completionStages	{ get { return m_CompletionStages;	 }}
		public List<RewardStage>		rewardStages		{ get { return m_RewardStages;		 }}
												
		
        ///////////////////////////////////////////////////////////////////////////


		public void Update()
		{
			if (IsCompleted)
			{
				return;
			}

			SpawnWaveStage stage = m_Stages[m_StageIndex];

			if (!stage.isStarted)
			{
				stage.Start();
			}

			if (!stage.IsCompleted)
			{
				stage.Update();
			}

			if (stage.IsCompleted)
			{
				m_StageIndex++;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public bool IsCompleted
		{
			get { return m_StageIndex >= m_Stages.Count; }
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnAfterDeserialize()
		{
			Deserialize();
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnBeforeSerialize()
		{
			Serialize();
		}

		///////////////////////////////////////////////////////////////////////////

		private void Serialize()
		{
			m_SpawnStages.Clear();
			m_PauseStages.Clear();
			m_CompletionStages.Clear();
			m_RewardStages.Clear();

			for (int i = 0; i < m_Stages.Count; ++i)
			{
				SpawnWaveStage stage = m_Stages[i];
				stage.index = i;

				if (stage is MonsterSpawnStage)
				{
					m_SpawnStages.Add(stage as MonsterSpawnStage);
				}
				else if (stage is PauseStage)
				{
					m_PauseStages.Add(stage as PauseStage);
				}
				else if (stage is CompletionStage)
				{
					m_CompletionStages.Add(stage as CompletionStage);
				}
				else if (stage is RewardStage)
				{
					m_RewardStages.Add(stage as RewardStage);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private void Deserialize()
		{
			m_Stages.Clear();

			m_Stages.AddRange(m_SpawnStages.ConvertAll(p => p as SpawnWaveStage));
			m_Stages.AddRange(m_PauseStages.ConvertAll(p => p as SpawnWaveStage));
			m_Stages.AddRange(m_CompletionStages.ConvertAll(p => p as SpawnWaveStage));
			m_Stages.AddRange(m_RewardStages.ConvertAll(p => p as SpawnWaveStage));

			m_Stages = m_Stages.OrderBy(p => p.index).ToList();

			m_Stages.ForEach(p => p.wave = this);
		}

		///////////////////////////////////////////////////////////////////////////

		public MonsterSpawnStage GetFirstPreviousMonsterSpawn(int index)
		{
			for (int i = index - 1; i <= index; --i)
			{
				if (m_Stages[i] is MonsterSpawnStage)
				{
					return m_Stages[i] as MonsterSpawnStage;
				}
			}

			return null;
		}

		///////////////////////////////////////////////////////////////////////////

	}
}