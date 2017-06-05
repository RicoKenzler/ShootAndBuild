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
                Vector3 desiredForce = directionTowardsTarget * speed;

                if (restingInfo.IsResting)
                {
                    TryStartAnim(idleAnimName);
                    movable.moveForce = movable.moveForce * 0.8f;
                }
                else
                {
                    movable.moveForce = desiredForce; 
                }
            }
            else
            {
				movable.moveForce = Vector2.zero;

                TryPerformInstantAttack(nearestTarget);
            }
        }
    }
}