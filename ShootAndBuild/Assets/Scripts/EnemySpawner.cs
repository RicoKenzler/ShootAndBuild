﻿using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 5.0f;
    public GameObject enemyPrefab;

    private float nextSpawnDuration = 0.0f;
	int totalSpawnCount = 0;

	void Start()
    {
        nextSpawnDuration = 2.0f;
	}
	
	void Update()
    {
        nextSpawnDuration -= Time.deltaTime;
        if (nextSpawnDuration <= 0)
        {
			totalSpawnCount++;
            nextSpawnDuration = spawnInterval;
            GameObject instance = Instantiate(enemyPrefab, gameObject.transform);
            instance.transform.position = transform.position;
			instance.name = enemyPrefab.name + " " + totalSpawnCount;
        }
	}
}
