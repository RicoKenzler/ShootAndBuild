﻿using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 5.0f;
	public float firstSpawn    = 2.0f;
    public GameObject enemyPrefab;

    private float nextSpawnDuration = 0.0f;
	int totalSpawnCount = 0;

	void Start()
    {
        nextSpawnDuration = firstSpawn;
	}
	
	void Update()
    {
        nextSpawnDuration -= Time.deltaTime;

        if (nextSpawnDuration <= 0)
        {
			Spawn();
        }
	}

	public void ForceImmediateSpawn()
	{
		Spawn();
	}

	void Spawn()
	{
		if (CheatManager.instance.stopEnemySpawns)
		{
			return;
		}

		totalSpawnCount++;
        nextSpawnDuration = spawnInterval;
        GameObject instance = Instantiate(enemyPrefab);
        instance.transform.position = transform.position;
		instance.name = enemyPrefab.name + " " + totalSpawnCount;
	}
}
