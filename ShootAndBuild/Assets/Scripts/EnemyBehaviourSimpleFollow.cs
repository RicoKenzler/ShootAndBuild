using System;
using UnityEngine;

namespace SAB
{
    public class EnemyBehaviourSimpleFollow : EnemyBehaviourBase
    {
        void Start()
        {
			base.Start();
        }

		protected override void OnUpdate()
		{
            float playerDistanceSq;
            float buildingDistanceSq;
            GameObject nearestPlayer = GetNearestPlayer(out playerDistanceSq);
            GameObject nearestBuilding = GetNearestBuilding(out buildingDistanceSq);
			
            GameObject nearestTarget = playerDistanceSq < buildingDistanceSq ? nearestPlayer : nearestBuilding;

            if (!nearestTarget)
            {
                if (!animationController.IsPlaying("attack"))
                {
                    animationController.Play("idle");
                }

                movable.moveForce = Vector2.zero;
                return;
            }

            Vector3 direction = (nearestTarget.transform.position - transform.position);
            float distToPlayer = direction.magnitude;

            if (distToPlayer == 0.0f)
            {
                direction = new Vector3(1.0f, 0.0f, 0.0f);
                distToPlayer = 1.0f;
            }
            else
            {
                direction /= distToPlayer;
            }

            transform.LookAt(nearestTarget.transform);

            if (distToPlayer > attackDistance)
            {
                movable.moveForce = direction * speed;
            }
            else
            {
                movable.moveForce = Vector2.zero;

                if (currentAttackCooldown == 0)
                {
                    currentAttackCooldown = attackCooldown;
                    nearestTarget.GetComponent<Attackable>().DealDamage(damage, gameObject, gameObject);

                    if (animationController)
                    {
                        animationController["attack"].speed = 4.0f;
                        animationController.Play("attack");
                    }

                    AudioManager.instance.PlayAudio(hitSound, transform.position);
                }
            }
        }
    }
}