using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnWave : ISerializationCallbackReceiver
	{
		public List<SpawnWaveStage> stages = new List<SpawnWaveStage>();

		[SerializeField]
		private List<MonsterSpawnStage> spawnStages = new List<MonsterSpawnStage>();
		[SerializeField]
		private List<PauseStage> pauseStages = new List<PauseStage>();
		[SerializeField]
		private List<CompletionStage> completionStages = new List<CompletionStage>();
		[SerializeField]
		private List<RewardStage> rewardStages = new List<RewardStage>();

		private int stageIndex = 0;

		//----------------------------------------------------------------------

		public void Update()
		{
			if (IsCompleted)
			{
				return;
			}

			SpawnWaveStage stage = stages[stageIndex];

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
				stageIndex++;
			}
		}

		//----------------------------------------------------------------------

		public bool IsCompleted
		{
			get { return stageIndex >= stages.Count; }
		}

		//----------------------------------------------------------------------

		public void OnAfterDeserialize()
		{
			Deserialize();
		}

		//----------------------------------------------------------------------

		public void OnBeforeSerialize()
		{
			Serialize();
		}

		//----------------------------------------------------------------------

		private void Serialize()
		{
			spawnStages.Clear();
			pauseStages.Clear();
			completionStages.Clear();
			rewardStages.Clear();

			for (int i = 0; i < stages.Count; ++i)
			{
				SpawnWaveStage stage = stages[i];
				stage.index = i;

				if (stage is MonsterSpawnStage)
				{
					spawnStages.Add(stage as MonsterSpawnStage);
				}
				else if (stage is PauseStage)
				{
					pauseStages.Add(stage as PauseStage);
				}
				else if (stage is CompletionStage)
				{
					completionStages.Add(stage as CompletionStage);
				}
				else if (stage is RewardStage)
				{
					rewardStages.Add(stage as RewardStage);
				}
			}
		}

		//----------------------------------------------------------------------

		private void Deserialize()
		{
			stages.Clear();

			stages.AddRange(spawnStages.ConvertAll(p => p as SpawnWaveStage));
			stages.AddRange(pauseStages.ConvertAll(p => p as SpawnWaveStage));
			stages.AddRange(completionStages.ConvertAll(p => p as SpawnWaveStage));
			stages.AddRange(rewardStages.ConvertAll(p => p as SpawnWaveStage));

			stages = stages.OrderBy(p => p.index).ToList();

			stages.ForEach(p => p.wave = this);
		}

		//----------------------------------------------------------------------

		public MonsterSpawnStage GetFirstPreviousMonsterSpawn(int index)
		{
			for (int i = index - 1; i <= index; --i)
			{
				if (stages[i] is MonsterSpawnStage)
				{
					return stages[i] as MonsterSpawnStage;
				}
			}

			return null;
		}

		//----------------------------------------------------------------------

	}
}