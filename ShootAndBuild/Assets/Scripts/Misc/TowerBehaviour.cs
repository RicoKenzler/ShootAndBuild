using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class TowerBehaviour : MonoBehaviour
    {
        [SerializeField] private bool m_TurnTowardsEnemy = false;
		[SerializeField] private WeaponData m_WeaponData;

		///////////////////////////////////////////////////////////////////////////

        private Shooter shooter;

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            shooter = GetComponent<Shooter>();
        }

		///////////////////////////////////////////////////////////////////////////

		void Start()
		{
			if (m_WeaponData)
			{
				shooter.AddWeapon(new WeaponWithAmmo(m_WeaponData, 1), true);
			}
		}

		///////////////////////////////////////////////////////////////////////////

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

            if (m_TurnTowardsEnemy)
            {
                transform.LookAt(nearestEnemy.transform);
            }

			TryShoot(nearestEnemy);
        }

		///////////////////////////////////////////////////////////////////////////

		void TryShoot(GameObject target)
		{
			if (shooter.currentWeapon == null)
			{
				return;
			}

			Vector3 lookatVector = target.transform.position - transform.position;
			lookatVector.y = 0.0f;

			float weaponRangeSquared = shooter.currentWeapon.weaponData.range;
			weaponRangeSquared *= weaponRangeSquared;

			if (weaponRangeSquared < lookatVector.sqrMagnitude)
			{
				return;
			}

			shooter.TryShoot(lookatVector);
		}

		///////////////////////////////////////////////////////////////////////////

        private GameObject GetNearestEnemy()
        {
			List<EnemyBehaviourBase> allEnemies = EnemyManager.instance.allEnemies;
            
            GameObject bestEnemy = null;
            float bestDistanceSq = float.MaxValue;

            foreach (EnemyBehaviourBase enemy in allEnemies)
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
}