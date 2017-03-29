using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
	private Shootable shootable;

	void Awake()
	{
		shootable = GetComponent<Shootable>();
	}

	void Update()
	{
		GameObject nearestEnemy = GetNearestEnemy();
		if (!nearestEnemy)
		{
			return;
		}

		transform.LookAt(nearestEnemy.transform);
		if (shootable.currentCooldown == 0)
		{
			shootable.Shoot();
		}
	}

	private GameObject GetNearestEnemy()
	{
		EnemyBehaviour[] enemies = FindObjectsOfType<EnemyBehaviour>();
		GameObject bestEnemy = null;
		float bestDistanceSq = float.MaxValue;

		foreach (EnemyBehaviour enemy in enemies)
		{
			float distanceSq = (enemy.transform.position - transform.position).sqrMagnitude;

			if (distanceSq < bestDistanceSq)
			{
				bestDistanceSq = distanceSq;
				bestEnemy = enemy.gameObject;
			}
		}

		return bestEnemy;
	}
}
