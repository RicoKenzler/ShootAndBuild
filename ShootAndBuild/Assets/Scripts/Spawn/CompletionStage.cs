using System;

namespace SAB.Spawn
{
	[Serializable]
	public class CompletionStage : SpawnWaveStage
	{
		public int completion = 0;
		private MonsterSpawnStage linkedMonsterSpawn = null;

		//----------------------------------------------------------------------

		public override void Start()
		{
			base.Start();

			linkedMonsterSpawn = wave.GetFirstPreviousMonsterSpawn(index);
		}

		//----------------------------------------------------------------------

		public override bool IsCompleted
		{
			get
			{
				if (linkedMonsterSpawn == null)
				{
					return true;
				}

				float p = linkedMonsterSpawn.GetProgress();
				p *= 100.0f;

				return p >= completion;
			}
		}

		//----------------------------------------------------------------------

	}
}