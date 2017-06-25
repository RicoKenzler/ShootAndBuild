using UnityEngine;

namespace SAB
{

    public enum EnemyType
    {
        None = 0,
        Bat = 1,
        Rabbit = 2,
        Slime = 3,
        Orc = 4,
    }

    public enum TargetPreference
    {
        Human,
        Buildings,
        QuestBuilding,
        None,
    }

    [System.Serializable]
    public struct RestingInfo
    {
        public float restDuration;
        public float restCooldown;

        private bool    isResting;
        private float   timeUntilNextStatusChange;

        public bool UsesResting()
        {
            return restDuration > 0.0f;
        }

        public bool IsResting
        {
            get
            {
                return isResting;
            }
        }

        private void StartResting()
        {
            isResting = true;
            timeUntilNextStatusChange = restDuration * Random.Range(0.8f, 1.3f);
        }

        private void EndResting()
        {
            isResting = false;
            timeUntilNextStatusChange = restCooldown * Random.Range(0.8f, 1.3f);
        }

        public void Init(bool startResting)
        {
            if (startResting)
            {
                StartResting();
            }
            else
            {
                EndResting();
            }
        }

        public void Tick()
        {
            if (!UsesResting())
            {
                return;
            }

            timeUntilNextStatusChange -= Time.deltaTime;

            if (timeUntilNextStatusChange <= 0.0f)
            {
                if (isResting)
                {
                    EndResting();
                }
                else
                {
                    StartResting();
                }
            }
        }
    }

    public abstract class EnemyBehaviourBase : MonoBehaviour
    {
        public EnemyType type;

        public float speed			= 10;
        public float attackDistance = 1;
        public float attackCooldown = 1;
        public int damage			= 1;
        public AudioData hitSound;

        public TargetPreference targetPreference = TargetPreference.None;

        public RestingInfo restingInfo;

        protected float currentAttackCooldown = 0;

        protected Animation animationController;
        protected Movable movable;


		protected string idleAnimName		= "idle";
		protected string walkAnimName		= "walk";
		protected string attackAnimName		= "attack";

		// ------------------------------------------------

		protected void TryStartAnim(string animName, float speed = 1.0f, bool suppressedByAttack = true)
		{
            if (suppressedByAttack && animationController.IsPlaying(attackAnimName))
            {
				return;
			}

            AnimationState animationState = animationController[animName];
            if (animationState)
            {
                animationState.speed = speed;
			    animationController.Play(animName);
            }
            else
            {
                Debug.Log(gameObject + "does not have animation " + animName);
            }
		}

		// ------------------------------------------------

		protected virtual void Awake()
		{
			animationController = GetComponentInChildren<Animation>();
			movable = GetComponent<Movable>();
		}

		// ------------------------------------------------

        protected virtual void Start()
        {
            restingInfo.Init(false);

            TryStartAnim(idleAnimName);
			
            EnemyManager.instance.RegisterEnemy(this, false);
            transform.SetParent(EnemyManager.instance.transform);
        }

		// ------------------------------------------------

        void OnDisable()
        {
            EnemyManager.instance.RegisterEnemy(this, true);
        }

		// ------------------------------------------------

        protected GameObject GetNearestPlayer(out float distSq)
        {
            Vector3 selfPos = transform.position;

            GameObject bestPlayer = null;
            float bestDistanceSq = float.MaxValue;

            foreach (InputController player in PlayerManager.instance.allAlivePlayers)
            {
                float distanceSq = (player.transform.position - selfPos).sqrMagnitude;

                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestPlayer = player.gameObject;
                }
            }

            distSq = bestDistanceSq;
            return bestPlayer;
        }

		// ------------------------------------------------

        protected GameObject GetNearestBuilding(out float distSq)
        {
            Vector3 selfPos = transform.position;

            GameObject bestBuilding = null;
            float bestDistanceSq = float.MaxValue;

            foreach (Building building in BuildingManager.instance.allBuildings)
            {
                float distanceSq = (building.gameObject.transform.position - selfPos).sqrMagnitude;

                if (distanceSq < bestDistanceSq)
                {
                    bestDistanceSq = distanceSq;
                    bestBuilding = building.gameObject;
                }
            }

            distSq = bestDistanceSq;
            return bestBuilding;
        }

		// ------------------------------------------------

		protected GameObject FindNextTarget()
		{
			float playerDistanceSq;
            float buildingDistanceSq;
            GameObject nearestPlayer = GetNearestPlayer(out playerDistanceSq);
            GameObject nearestBuilding = GetNearestBuilding(out buildingDistanceSq);
            GameObject nearestTarget = playerDistanceSq < buildingDistanceSq ? nearestPlayer : nearestBuilding;
            float nearestDistanceSq = Mathf.Min(playerDistanceSq, buildingDistanceSq);

            GameObject bestTarget = null;
            
            switch (targetPreference)
            {
                case TargetPreference.Buildings:
                    bestTarget = nearestBuilding ? nearestBuilding : nearestPlayer;
                    break;

                case TargetPreference.Human:
                    bestTarget = nearestPlayer ? nearestPlayer : nearestBuilding;
                    break;

                case TargetPreference.QuestBuilding:
                    if (GameManager.Instance.loseCondition == LoseCondition.DestroyObject)
                    {
                        bestTarget = GameManager.Instance.loseConditionContextObject;
                    }
                    break;

                case TargetPreference.None:
                    break;
            }

            if (nearestTarget && nearestDistanceSq <= (attackDistance * attackDistance))
            {
                // even if we have a preference, we should attack proximite targets
                bestTarget = nearestTarget;
            }

            if (!bestTarget)
            {
                // nearest target as fallback
                bestTarget = nearestTarget;
            }

			return bestTarget;
		}

		// ------------------------------------------------

		protected float GetDistanceAndDirectionTo(Vector3 target, out Vector3 direction)
		{
			direction = (target - transform.position);
            float distance = direction.magnitude;

            if (distance == 0.0f)
            {
                direction = new Vector3(1.0f, 0.0f, 0.0f);
                distance  = 1.0f;
            }
            else
            {
                direction /= distance;
            }

			return distance;
		}

        // ------------------------------------------------

        protected Vector3 EstimateFuturePosition(GameObject target, float timeIntoFuture)
        {
            Rigidbody targetRigidbody = target.GetComponent<Rigidbody>();

            if (!targetRigidbody)
            {
                return target.transform.position;
            }
           
            Vector3 estimatedOffset = targetRigidbody.velocity * timeIntoFuture;
            Vector3 estimatedPosition = target.transform.position + estimatedOffset;

            return estimatedPosition;
        }
   
		// ------------------------------------------------

		protected void TryPerformInstantAttack(GameObject target)
		{
            if (currentAttackCooldown > 0)
            {
				return;
			}

            currentAttackCooldown = attackCooldown;
            target.GetComponent<Attackable>().DealDamage(damage, gameObject, gameObject);

			TryStartAnim(attackAnimName, 4.0f, false);
					
            AudioManager.instance.PlayAudio(hitSound, transform.position);
		}

		// ------------------------------------------------

        void Update()
        {
			// 0) Is Frozen...?
            if (CheatManager.instance.freezeEnemies)
            {
				// ... stop update
                GetComponent<Movable>().moveForce = new Vector3(0.0f, 0.0f, 0.0f);
                return;
            }

			// 1) Attack Cooldown
            if (currentAttackCooldown > 0)
            {
                currentAttackCooldown = Mathf.Max(currentAttackCooldown - Time.deltaTime, 0);
            }

			// 2) Trigger Combat music
			if (MusicManager.instance.LastCombatTime != Time.time)
			{
				float playerDistanceSq;
				GetNearestPlayer(out playerDistanceSq);

				const float PLAYER_DISTANCE_COMBAT_STATE = 15.0f;
				if (playerDistanceSq < (PLAYER_DISTANCE_COMBAT_STATE * PLAYER_DISTANCE_COMBAT_STATE))
				{
					MusicManager.instance.SignalIsInCombat();
				}
			}
			
            // 3) Tick Resting
            restingInfo.Tick();

			// 3) Main Update of Sub-Class
			OnUpdate();
            
			TryStartAnim(walkAnimName);
        }

		// ------------------------------------------------

		protected abstract void OnUpdate();
    }

}