using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class RewardStage : SpawnWaveStage
	{
		[SerializeField] private int m_Gold = 0;
		[SerializeField] private Collectable m_Reward = null;

		///////////////////////////////////////////////////////////////////////////

		public int gold				{ get { return m_Gold; }	set { m_Gold = value; } }
		public Collectable reward	{ get { return m_Reward; }	set { m_Reward = value; } }

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();
		}

		///////////////////////////////////////////////////////////////////////////

		public override bool IsCompleted
		{
			get
			{
				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////

	}
}