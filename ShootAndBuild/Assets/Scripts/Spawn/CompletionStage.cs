using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class CompletionStage : SpawnWaveStage
	{
		[SerializeField] private int m_Completion = 100;
		private MonsterSpawnStage m_LinkedMonsterSpawn = null;

		///////////////////////////////////////////////////////////////////////////

		public int completion { get { return m_Completion; } set { m_Completion = value; } }
		
		float GetCompletionPercentage()
		{
			if (m_LinkedMonsterSpawn == null)
			{
				return 1.0f;
			}

			float p = m_LinkedMonsterSpawn.GetProgress();

			return p;
		}

		///////////////////////////////////////////////////////////////////////////

		public override bool isCompleted
		{
			get
			{
				float p = GetCompletionPercentage() * 100.0f;

				return p >= m_Completion;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();

			m_LinkedMonsterSpawn = wave.GetFirstPreviousMonsterSpawn(index);
		}

		///////////////////////////////////////////////////////////////////////////

		public override string GetDebugInfo() 
		{
			string stageName = "Completion";

			string progressionString = "(" + (int) (GetCompletionPercentage() * 100.0f) + " / " + m_Completion + ")";

			return stageName + " " + progressionString;
		}

		///////////////////////////////////////////////////////////////////////////
	}
}