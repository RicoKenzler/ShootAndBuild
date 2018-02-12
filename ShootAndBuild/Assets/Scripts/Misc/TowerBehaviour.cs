using UnityEngine;

namespace SAB
{
    public class TowerBehaviour : MonoBehaviour
    {
        [SerializeField] private bool m_TurnTowardsEnemy = false;

		///////////////////////////////////////////////////////////////////////////

        private Shooter shootable;

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            shootable = GetComponent<Shooter>();
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

            if (shootable.cooldown <= 0)
            {
				Vector3 lookatVector = nearestEnemy.transform.position - transform.position;

				// make 2D (?)
				lookatVector.y = 0.0f;

                Quaternion rotationToEnemy = Quaternion.LookRotation(lookatVector);

                shootable.Shoot(rotationToEnemy);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        private GameObject GetNearestEnemy()
        {
            EnemyBehaviourBase[] enemies = FindObjectsOfType<EnemyBehaviourBase>();
            GameObject bestEnemy = null;
            float bestDistanceSq = float.MaxValue;

            foreach (EnemyBehaviourBase enemy in enemies)
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