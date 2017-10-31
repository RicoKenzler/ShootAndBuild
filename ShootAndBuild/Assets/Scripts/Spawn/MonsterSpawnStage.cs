using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class MonsterSpawnStage : SpawnWaveStage
	{
		[SerializeField] private List<SpawnMob> m_Monsters = new List<SpawnMob>();
		[SerializeField] private float			m_Duration = 0;

		///////////////////////////////////////////////////////////////////////////

		private int m_NumOfSpawnedMonsters = 0;
		private int m_NumOfMonstersToSpawn = 0;
		private int m_NumOfDeadMonsters = 0;
		private float m_LifeTime = 0;
		private List<GameObject> m_MonstersToSpawn = new List<GameObject>();

		///////////////////////////////////////////////////////////////////////////

		public List<SpawnMob>	monsters { get { return m_Monsters; } set { m_Monsters = monsters; } }
		public float			duration { get { return m_Duration; } set { m_Duration = duration; } }

		public override bool isCompleted
		{
			get
			{
				return m_NumOfMonstersToSpawn == m_NumOfSpawnedMonsters;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();

			if (m_Duration == 0)
			{
				foreach (SpawnMob mob in m_Monsters)
				{
					for (int i = 0; i < mob.count; ++i)
					{
						SpawnMonster(mob.enemy);
					}

					m_NumOfMonstersToSpawn += mob.count;
				}
            }
			else
			{
				foreach (SpawnMob mob in m_Monsters)
				{
					for (int i = 0; i < mob.count; ++i)
					{
						m_MonstersToSpawn.Add(mob.enemy);
					}
				}

				m_NumOfMonstersToSpawn = m_MonstersToSpawn.Count;

				m_MonstersToSpawn = m_MonstersToSpawn.OrderBy(a => Guid.NewGuid()).ToList();
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Update()
		{
			base.Update();

			float p = m_LifeTime / m_Duration;
			p = Mathf.Clamp01(p);

			int targetIndex = Mathf.FloorToInt(p * m_NumOfMonstersToSpawn);
			int curIndex = m_NumOfMonstersToSpawn - m_MonstersToSpawn.Count;

			for (int i = 0; i < targetIndex - curIndex; ++i)
			{
				SpawnMonster(m_MonstersToSpawn[0]);
				m_MonstersToSpawn.RemoveAt(0);
			}

			m_LifeTime += Time.deltaTime;
		}

		///////////////////////////////////////////////////////////////////////////

		private void SpawnMonster(GameObject monster)
		{
			// this is the quick and dirty solution
			// TODO implement proper spawning

			EnemySpawner[] spawners = UnityEngine.Object.FindObjectsOfType<EnemySpawner>();
			if (spawners.Length == 0)
			{
				return;
			}

			int index = UnityEngine.Random.Range(0, spawners.Length);
			EnemySpawner spawner = spawners[index];

			Vector3 spawnPosition = spawner.transform.position + UnityEngine.Random.insideUnitSphere * spawner.spawnRadius;
			spawnPosition.y = spawner.transform.position.y;

			GameObject enemyInstance = GameObject.Instantiate(monster, spawnPosition, Quaternion.identity);
			enemyInstance.name = spawner.name + " " + m_NumOfSpawnedMonsters;

			Attackable attackable = enemyInstance.GetComponent<Attackable>();
			if (attackable)
			{
				attackable.OnAttackableDies += OnAttackableDies;
			}

			m_NumOfSpawnedMonsters++;
		}

		///////////////////////////////////////////////////////////////////////////

		private void OnAttackableDies(Attackable attackable)
		{
			attackable.OnAttackableDies -= OnAttackableDies;

			m_NumOfDeadMonsters++;
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetProgress()
		{
			return Mathf.Clamp01(m_NumOfDeadMonsters / m_NumOfSpawnedMonsters);
		}

		///////////////////////////////////////////////////////////////////////////
	}
}