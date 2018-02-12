using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class RewardStage : SpawnWaveStage
	{
		[SerializeField] private ItemAndCount[] m_Rewards;

		///////////////////////////////////////////////////////////////////////////

		public ItemAndCount[] rewards	{ get { return m_Rewards; }	set { m_Rewards = value; } }

		///////////////////////////////////////////////////////////////////////////

		public override bool isCompleted
		{
			get
			{
				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();

			Inventory.ChangeItemCount_AutoSelectInventories(rewards, false);
		}

		///////////////////////////////////////////////////////////////////////////

	}
}