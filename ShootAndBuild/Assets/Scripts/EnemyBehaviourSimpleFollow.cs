using System;
using UnityEngine;

namespace SAB
{
    public class EnemyBehaviourSimpleFollow : EnemyBehaviourBase
    {
        public  float zigZagFrequency    = 0.0f;
        public  float zigZagAngle        = 90.0f;
        private int   zigZagID           = 0;

        protected override void Start()
        {
			base.Start();

            // TODO: Use other ID system
            zigZagID = UnityEngine.Random.Range(0, 10000);
        }

        Vector3 ApplyZigZagAmount(Vector3 direction)
        {
            if (zigZagFrequency == 0.0f || zigZagAngle == 0.0f)
            {
                return direction;
            }

            float currentAngleNorm = Mathf.Sin(zigZagFrequency * Time.time + zigZagID);
            float currentAngle = zigZagAngle * 0.5f * currentAngleNorm;

            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);

            return rotation * direction;
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

            directionTowardsTarget = ApplyZigZagAmount(directionTowardsTarget);

            transform.rotation = Quaternion.LookRotation(directionTowardsTarget);

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