using UnityEngine;

namespace SAB
{

    public enum EnemyType
    {
        None = 0,
        Bat = 1,
        Rabbit = 2,
        Slime = 3,
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
        public float speed			= 10;
        public float attackDistance = 1;
        public float attackCooldown = 1;
        public int damage			= 1;
        public AudioData hitSound;

        public RestingInfo restingInfo;

        protected float currentAttackCooldown = 0;

        protected Animation animationController;
        protected Movable movable;

        public EnemyType type;

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

			animationController[animName].speed = speed;
			animationController.Play(animName);
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

		protected GameObject FindNearestTarget()
		{
			float playerDistanceSq;
            float buildingDistanceSq;
            GameObject nearestPlayer = GetNearestPlayer(out playerDistanceSq);
            GameObject nearestBuilding = GetNearestBuilding(out buildingDistanceSq);
			
            GameObject nearestTarget = playerDistanceSq < buildingDistanceSq ? nearestPlayer : nearestBuilding;

			return nearestTarget;
		}

		// ------------------------------------------------

		protected float GetDistanceTo(GameObject target, out Vector3 direction)
		{
			direction = (target.transform.position - transform.position);
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
			// 0) Is Forzen...?
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