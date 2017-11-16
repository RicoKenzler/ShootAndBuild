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
		
		public override bool isCompleted
		{
			get
			{
				if (m_LinkedMonsterSpawn == null)
				{
					return true;
				}

				float p = m_LinkedMonsterSpawn.GetProgress();
				p *= 100.0f;

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

	}
}