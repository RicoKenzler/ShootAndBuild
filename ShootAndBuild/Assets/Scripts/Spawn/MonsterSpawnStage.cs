using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class MonsterSpawnStage : SpawnWaveStage
	{
		public List<SpawnMob> monsters = new List<SpawnMob>();
		public float duration = 0;

		private int numOfSpawnedMonsters = 0;
		private int numOfMonstersToSpawn = 0;
		private int numOfDeadMonsters = 0;
		private float lifeTime = 0;
		private List<GameObject> monstersToSpawn = new List<GameObject>();

		//----------------------------------------------------------------------

		public override void Start()
		{
			base.Start();

			if (duration == 0)
			{
				foreach (SpawnMob mob in monsters)
				{
					for (int i = 0; i < mob.count; ++i)
					{
						SpawnMonster(mob.enemy);
					}

					numOfMonstersToSpawn += mob.count;
				}
            }
			else
			{
				foreach (SpawnMob mob in monsters)
				{
					for (int i = 0; i < mob.count; ++i)
					{
						monstersToSpawn.Add(mob.enemy);
					}
				}

				numOfMonstersToSpawn = monstersToSpawn.Count;

				monstersToSpawn = monstersToSpawn.OrderBy(a => Guid.NewGuid()).ToList();
			}
		}

		//----------------------------------------------------------------------

		public override void Update()
		{
			base.Update();

			float p = lifeTime / duration;
			p = Mathf.Clamp01(p);

			int targetIndex = Mathf.FloorToInt(p * numOfMonstersToSpawn);
			int curIndex = numOfMonstersToSpawn - monstersToSpawn.Count;

			for (int i = 0; i < targetIndex - curIndex; ++i)
			{
				SpawnMonster(monstersToSpawn[0]);
				monstersToSpawn.RemoveAt(0);
			}

			lifeTime += Time.deltaTime;
		}

		//----------------------------------------------------------------------

		public override bool IsCompleted
		{
			get
			{
				return numOfMonstersToSpawn == numOfSpawnedMonsters;
			}
		}

		//----------------------------------------------------------------------

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
			enemyInstance.name = spawner.name + " " + numOfSpawnedMonsters;

			Attackable attackable = enemyInstance.GetComponent<Attackable>();
			if (attackable)
			{
				attackable.OnAttackableDies += OnAttackableDies;
			}

			numOfSpawnedMonsters++;
		}

		//----------------------------------------------------------------------

		private void OnAttackableDies(Attackable attackable)
		{
			attackable.OnAttackableDies -= OnAttackableDies;

			numOfDeadMonsters++;
		}

		//----------------------------------------------------------------------

		public float GetProgress()
		{
			return Mathf.Clamp01(numOfDeadMonsters / numOfSpawnedMonsters);
		}

		//----------------------------------------------------------------------
	}
}