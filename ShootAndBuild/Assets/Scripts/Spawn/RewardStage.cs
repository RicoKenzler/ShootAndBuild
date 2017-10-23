using System;

namespace SAB.Spawn
{
	[Serializable]
	public class RewardStage : SpawnWaveStage
	{
		public int gold = 0;
		public Collectable reward = null;


		//----------------------------------------------------------------------

		public override void Start()
		{
			base.Start();
		}

		//----------------------------------------------------------------------

		public override bool IsCompleted
		{
			get
			{
				return true;
			}
		}

		//----------------------------------------------------------------------

	}
}