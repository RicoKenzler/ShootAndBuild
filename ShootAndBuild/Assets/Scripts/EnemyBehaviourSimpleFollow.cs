using System;
using UnityEngine;

namespace SAB
{
    public class EnemyBehaviourSimpleFollow : EnemyBehaviourBase
    {
        protected override void Start()
        {
			base.Start();
        }

		protected override void OnUpdate()
		{
            GameObject nearestTarget = FindNearestTarget();

            if (!nearestTarget)
            {
                TryStartAnim(idleAnimName);

                movable.moveForce = Vector2.zero;
                return;
            }
			
            Vector3 directionTowardsTarget;
			float distToTarget = GetDistanceTo(nearestTarget, out directionTowardsTarget);

            transform.LookAt(nearestTarget.transform);

            if (distToTarget > attackDistance)
            {
                movable.moveForce = directionTowardsTarget * speed;
            }
            else
            {
				movable.moveForce = Vector2.zero;

                TryAttack(nearestTarget);
            }
        }
    }
}