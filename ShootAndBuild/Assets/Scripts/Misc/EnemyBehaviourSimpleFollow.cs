using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    public class EnemyBehaviourSimpleFollow : EnemyBehaviourBase
    {
		[FormerlySerializedAs("zigZagFrequency")]
        [SerializeField] private float m_ZigZagFrequency					= 0.0f;

		[FormerlySerializedAs("zigZagAngle")]
        [SerializeField] private float m_ZigZagAngle						= 90.0f;

		[FormerlySerializedAs("estimateFuturePosition")]
        [Range (0.0f, 1.0f)]
        [SerializeField] private float m_EstimateFuturePosition				= 0.0f;

		[FormerlySerializedAs("estimateFuturePositionSmoothness")]
        [Range (0.0f, 1.0f)]
        [SerializeField] private float m_EstimateFuturePositionSmoothness	= 0.9f;

		///////////////////////////////////////////////////////////////////////////

        private int			m_ZigZagID							= 0;
        private GameObject  m_LastTarget						= null;
        private Vector3     m_LastTargetToTargetFutureOffset	= Vector3.zero;

		///////////////////////////////////////////////////////////////////////////

        protected override void Start()
        {
			base.Start();

            // TODO: Use other ID system
            m_ZigZagID = UnityEngine.Random.Range(0, 10000);
        }

		///////////////////////////////////////////////////////////////////////////

        Vector3 ApplyZigZagAmount(Vector3 direction)
        {
            if (m_ZigZagFrequency == 0.0f || m_ZigZagAngle == 0.0f)
            {
                return direction;
            }

            float currentAngleNorm = Mathf.Sin(m_ZigZagFrequency * Time.time + m_ZigZagID);
            float currentAngle = m_ZigZagAngle * 0.5f * currentAngleNorm;

            Quaternion rotation = Quaternion.AngleAxis(currentAngle, Vector3.up);

            return rotation * direction;
        }

		///////////////////////////////////////////////////////////////////////////

		protected override void OnUpdate()
		{
            GameObject nearestTarget = FindNextTarget();

            if (!nearestTarget)
            {
                // No target: Idle around
                TryStartAnim(IDLE_ANIM_NAME);

                m_Movable.moveForce = Vector2.zero;
                return;
            }
			
            // 1) Directly towards target
            Vector3 exactTargetPos = nearestTarget.transform.position;

            Vector3 exactDirectionTowardsTarget;
			float distToTarget = GetDistanceAndDirectionTo(exactTargetPos, out exactDirectionTowardsTarget);

            // 2) Mix with future position
            float timeIntoFuture = Math.Min(distToTarget, 15.0f) * 0.2f * m_EstimateFuturePosition;
            Vector3 futureTargetPos = EstimateFuturePosition(nearestTarget, timeIntoFuture);

            if (m_LastTarget != nearestTarget)
            {
                m_LastTargetToTargetFutureOffset = Vector3.zero;
            }

            // Try not to run away from current target, just to reach future target
            Vector3 towardsFutureTargetPos = futureTargetPos - transform.position; //< caution: not normalized
            if (Vector3.Dot(towardsFutureTargetPos, exactDirectionTowardsTarget) <= 0.0f)
            {
                futureTargetPos = Vector3.Lerp(futureTargetPos, exactTargetPos, 0.5f);
            }

            Vector3 targetToTargetFuturePositionOffset = futureTargetPos - exactTargetPos;
            Vector3 smoothTargetToTargetFuturePositionOffset = Vector3.Lerp(targetToTargetFuturePositionOffset, m_LastTargetToTargetFutureOffset, m_EstimateFuturePositionSmoothness);
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

            if (distToTarget > m_AttackDistance)
            {
                // 5 a) Move
                Vector3 desiredForce = modifiedDirectionTowardsTarget * m_Speed;

                if (m_RestingInfo.IsResting)
                {
                    // 5 a1) Fade out moving to rest
                    TryStartAnim(IDLE_ANIM_NAME);
                    m_Movable.moveForce = m_Movable.moveForce * 0.8f;
                }
                else
                {
                    // 5 a2) Move
                    m_Movable.moveForce = desiredForce; 
                }
            }
            else
            {
                // 5 b) Attack
				m_Movable.moveForce = Vector2.zero;

                TryPerformInstantAttack(nearestTarget);
            }

            m_LastTarget                      = nearestTarget;
            m_LastTargetToTargetFutureOffset  = smoothTargetToTargetFuturePositionOffset;
        }
    }
}