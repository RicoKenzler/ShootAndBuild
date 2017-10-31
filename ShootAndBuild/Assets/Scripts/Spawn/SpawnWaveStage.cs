using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnMob
	{
		public GameObject enemy;
		public int count;
	}

	///////////////////////////////////////////////////////////////////////////

	[Serializable]
	public class SpawnWaveStage
	{
		public int index = 0;
		public bool isStarted = false;

		[NonSerialized]
		public SpawnWave wave = null;

		///////////////////////////////////////////////////////////////////////////

		public virtual void Start()
		{
			isStarted = true;
		}

		///////////////////////////////////////////////////////////////////////////

		public virtual void Update()
		{
		}

		///////////////////////////////////////////////////////////////////////////

		public virtual bool IsCompleted
		{
			get
			{
				return true;
			}
		}
	}
}