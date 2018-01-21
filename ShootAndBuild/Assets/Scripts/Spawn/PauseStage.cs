using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class PauseStage : SpawnWaveStage
	{
		[SerializeField] private float m_Duration = 0;

		///////////////////////////////////////////////////////////////////////////

		private float m_TimeLeft = 0;

		///////////////////////////////////////////////////////////////////////////

		public float duration { get { return m_Duration; } set { m_Duration = value; } }

		///////////////////////////////////////////////////////////////////////////

		public override bool isCompleted
		{
			get
			{
				return m_TimeLeft == 0;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override string GetDebugInfo() 
		{
			string stageName = "Pause";

			string progressionString = "(" + (int) m_TimeLeft + "/" + (int) m_Duration + ")";

			return stageName + " " + progressionString;
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();

			m_TimeLeft = m_Duration;
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Update()
		{
			base.Update();

			m_TimeLeft -= Time.deltaTime;
			if (m_TimeLeft < 0)
			{
				m_TimeLeft = 0;
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}

}