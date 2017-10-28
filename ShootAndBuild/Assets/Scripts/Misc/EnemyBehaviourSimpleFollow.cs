using System;
using UnityEngine;

namespace SAB
{
    public class EnemyBehaviourSimpleFollow : EnemyBehaviourBase
    {
        public  float zigZagFrequency    = 0.0f;
        public  float zigZagAngle        = 90.0f;
        private int   zigZagID           = 0;

        [Range (0.0f, 1.0f)]
        public float estimateFuturePosition     = 0.0f;

        [Range (0.0f, 1.0f)]
        public float estimateFuturePositionSmoothness = 0.9f;

        private GameObject  lastTarget = null;
        private Vector3     lastTargetToTargetFutureOffset = Vector3.zero;

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
            GameObject nearestTarget = FindNextTarget();

            if (!nearestTarget)
            {
                // No target: Idle around
                TryStartAnim(idleAnimName);

                movable.moveForce = Vector2.zero;
                return;
            }
			
            // 1) Directly towards target
            Vector3 exactTargetPos = nearestTarget.transform.position;

            Vector3 exactDirectionTowardsTarget;
			float distToTarget = GetDistanceAndDirectionTo(exactTargetPos, out exactDirectionTowardsTarget);

            // 2) Mix with future position
            float timeIntoFuture = Math.Min(distToTarget, 15.0f) * 0.2f * estimateFuturePosition;
            Vector3 futureTargetPos = EstimateFuturePosition(nearestTarget, timeIntoFuture);

            if (lastTarget != nearestTarget)
            {
                lastTargetToTargetFutureOffset = Vector3.zero;
            }

            // Try not to run away from current target, just to reach future target
            Vector3 towardsFutureTargetPos = futureTargetPos - transform.position; //< caution: not normalized
            if (Vector3.Dot(towardsFutureTargetPos, exactDirectionTowardsTarget) <= 0.0f)
            {
                futureTargetPos = Vector3.Lerp(futureTargetPos, exactTargetPos, 0.5f);
            }

            Vector3 targetToTargetFuturePositionOffset = futureTargetPos - exactTargetPos;
            Vector3 smoothTargetToTargetFuturePositionOffset = Vector3.Lerp(targetToTargetFuturePositionOffset, lastTargetToTargetFutureOffset, estimateFuturePositionSmoothness);
            Vector3 smoothFuturePosition = exactTargetPos + smoothTargetToTargetFuturePositionOffset;

            // Debug future position:
            //Debug.DrawLine(transform.position, exactTargetPos,          Color.green,    0.0166f);
            //Debug.DrawLine(transform.position, futureTargetPos,         Color.red,      0.0166f);
            //Debug.DrawLine(transform.position, smoothFuturePosition,    Color.magenta,  0.0166f);

            Vector3 modifiedDirectionTowardsTarget;
            GetDistanceAndDirectionTo(smoothFuturePosition, out modifiedDirectionTowardsTarget);

            // 3) Add ZigZag
            modifiedDirectionTowardsTarget = ApplyZigZagAmount(modifiedDirectionTowardsTarget);

            // 4) apply rotation
            transform.rotation = Quaternion.LookRotation(modifiedDirectionTowardsTarget);

            if (distToTarget > attackDistance)
            {
                // 5 a) Move
                Vector3 desiredForce = modifiedDirectionTowardsTarget * speed;

                if (restingInfo.IsResting)
                {
                    // 5 a1) Fade out moving to rest
                    TryStartAnim(idleAnimName);
                    movable.moveForce = movable.moveForce * 0.8f;
                }
                else
                {
                    // 5 a2) Move
                    movable.moveForce = desiredForce; 
                }
            }
            else
            {
                // 5 b) Attack
				movable.moveForce = Vector2.zero;

                TryPerformInstantAttack(nearestTarget);
            }

            lastTarget                      = nearestTarget;
            lastTargetToTargetFutureOffset  = smoothTargetToTargetFuturePositionOffset;
        }

     
    }
}