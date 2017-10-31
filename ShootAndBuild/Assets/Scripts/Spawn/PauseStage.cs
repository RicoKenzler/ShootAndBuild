using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class PauseStage : SpawnWaveStage
	{
		public float duration = 0;
		private float timeLeft = 0;

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			timeLeft = duration;
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Update()
		{
			base.Update();

			timeLeft -= Time.deltaTime;
			if (timeLeft < 0)
			{
				timeLeft = 0;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override bool IsCompleted
		{
			get
			{
				return timeLeft == 0;
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}

}