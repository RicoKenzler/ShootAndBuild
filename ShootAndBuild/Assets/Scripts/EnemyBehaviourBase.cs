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

    public abstract class EnemyBehaviourBase : MonoBehaviour
    {
        public float speed			= 10;
        public float attackDistance = 1;
        public float attackCooldown = 1;
        public int damage			= 1;
        public AudioData hitSound;

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
			
			// 3) Main Update of Sub-Class
			OnUpdate();
            
			TryStartAnim(walkAnimName);
        }

		// ------------------------------------------------

		protected abstract void OnUpdate();
    }

}