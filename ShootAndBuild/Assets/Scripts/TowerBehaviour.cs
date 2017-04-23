using UnityEngine;

public class TowerBehaviour : MonoBehaviour
{
	private Shootable shootable;
	public bool turnTowardsEnemy = false;

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

		if (CheatManager.instance.freezeTowers)
		{
			return;
		}

		if (turnTowardsEnemy)
		{
			transform.LookAt(nearestEnemy.transform);
		}
		
		if (shootable.currentCooldown == 0)
		{
			Quaternion rotationToEnemy = Quaternion.LookRotation(nearestEnemy.transform.position - transform.position);

			shootable.Shoot(rotationToEnemy);
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
