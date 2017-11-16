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

		public List<SpawnMob>	monsters { get { return m_Monsters; } set { m_Monsters = value; } }
		public float			duration { get { return m_Duration; } set { m_Duration = value; } }

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
			Vector3 pos = GetRandomSpawnPosition(monster);

			GameObject enemyInstance = GameObject.Instantiate(monster, pos, Quaternion.identity);
			enemyInstance.name = monster.name + " - " + m_NumOfSpawnedMonsters;

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

		private Vector3 GetRandomSpawnPosition(GameObject monster)
		{
			int run = 0;
			while (++run < 50)
			{
				float x = UnityEngine.Random.Range(0f, Grid.instance.size);
				float z = UnityEngine.Random.Range(0f, Grid.instance.size);

				Vector3 pos = new Vector3(x, 0, z);
				bool free = Grid.instance.IsFree(monster, pos);
				if (!free)
				{
					continue;
				}

				List<InputController> players = PlayerManager.instance.allAlivePlayers;
				foreach (InputController player in players)
				{
					float distance = Vector3.Distance(player.transform.position, pos);
					if (distance < 10.0f)
					{
						continue;
					}
				}

				pos.y = TerrainManager.instance.GetInterpolatedHeight(pos.x, pos.z);
				return pos;
			}

			Debug.LogError("Found no suitable position for an enemy!");

			float middle = Grid.instance.size / 2.0f;
			return new Vector3(middle, middle);
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetProgress()
		{
			if (m_NumOfDeadMonsters <= 0)
			{
				return 1.0f;
			}

			return Mathf.Clamp01(m_NumOfDeadMonsters / m_NumOfSpawnedMonsters);
		}

		///////////////////////////////////////////////////////////////////////////
	}
}