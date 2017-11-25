using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class SpawnMob
	{
		public GameObject	enemy;
		public int			count;
	}

	///////////////////////////////////////////////////////////////////////////

	[Serializable]
	public class SpawnWaveStage
	{
		[SerializeField] private int	m_Index = 0;
		[SerializeField] private bool	m_IsStarted = false;

		///////////////////////////////////////////////////////////////////////////


		[NonSerialized] private SpawnWave m_Wave = null;

		///////////////////////////////////////////////////////////////////////////

		public int index		{ get { return m_Index; } set { m_Index = value; } }
		public bool isStarted	{ get { return m_IsStarted; } }
		public SpawnWave wave	{ get { return m_Wave; } set { m_Wave = value; } }

		///////////////////////////////////////////////////////////////////////////

		public virtual void Start()
		{
			Debug.Log("Starting Stage " + SpawnManagerPrototype.instance.waveIndexHumanReadable + "." + index + " (" + ToString() + ")" );
			m_IsStarted = true;
		}

		///////////////////////////////////////////////////////////////////////////

		public virtual void Update()
		{
		}

		///////////////////////////////////////////////////////////////////////////

		public virtual bool isCompleted
		{
			get
			{
				return true;
			}
		}
	}
}