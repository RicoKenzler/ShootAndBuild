using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    public float spawnInterval = 5.0f;
    public GameObject enemyPrefab;

    private float nextSpawnDuration = 0.0f;

	void Start()
    {
        nextSpawnDuration = 2.0f;
	}
	
	void Update()
    {
        nextSpawnDuration -= Time.deltaTime;
        if (nextSpawnDuration <= 0)
        {
            nextSpawnDuration = spawnInterval;
            GameObject instance = Instantiate(enemyPrefab);
            instance.transform.position = transform.position;
        }
	}
}
