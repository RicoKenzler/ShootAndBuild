using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    public enum EnemyType
    {
        None = 0,
        Bat = 1,
        Rabbit = 2,
        Slime = 3,
        Orc = 4,
        Knight = 5,
    }

	///////////////////////////////////////////////////////////////////////////

    public enum TargetPreference
    {
        Human,
        Buildings,
        QuestBuilding,
        None,
    }

	///////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public struct RestingInfo
    {
        public float restDuration;
        public float restCooldown;

		///////////////////////////////////////////////////////////////////////////

        private bool    m_IsResting;
        private float   m_TimeUntilNextStatusChange;

		///////////////////////////////////////////////////////////////////////////

        public bool isResting { get { return m_IsResting; } }

		///////////////////////////////////////////////////////////////////////////

        public bool UsesResting()
        {
            return restDuration > 0.0f;
        }


        private void StartResting()
        {
            m_IsResting = true;
            m_TimeUntilNextStatusChange = restDuration * Random.Range(0.8f, 1.3f);
        }

        private void EndResting()
        {
            m_IsResting = false;
            m_TimeUntilNextStatusChange = restCooldown * Random.Range(0.8f, 1.3f);
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

            m_TimeUntilNextStatusChange -= Time.deltaTime;

            if (m_TimeUntilNextStatusChange <= 0.0f)
            {
                if (m_IsResting)
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

	///////////////////////////////////////////////////////////////////////////

    public abstract class EnemyBehaviourBase : MonoBehaviour
    {
		[FormerlySerializedAs("type")]
        [SerializeField] protected EnemyType	m_Type;

		[FormerlySerializedAs("speed")]
        [SerializeField] protected float		m_Speed			= 10;
		[FormerlySerializedAs("attackDistance")]
        [SerializeField] protected float		m_AttackDistance = 1;
		[FormerlySerializedAs("attackCooldown")]
        [SerializeField] protected float		m_AttackCooldown = 1;
		[FormerlySerializedAs("damage")]
        [SerializeField] protected int			m_Damage		= 1;
		[FormerlySerializedAs("hitSound")]
        [SerializeField] protected AudioData	m_HitSound;

		[FormerlySerializedAs("targetPreference")]
        [SerializeField] protected TargetPreference	m_TargetPreference = TargetPreference.None;

		[FormerlySerializedAs("restingInfo")]
        [SerializeField] protected RestingInfo		m_RestingInfo;

		[FormerlySerializedAs("buffOnAttack")]
		[SerializeField] protected BuffData			m_BuffOnAttack;
		[FormerlySerializedAs("buffOnAttackProbability")]
		[SerializeField] protected float			m_BuffOnAttackProbability = 0.2f;

		///////////////////////////////////////////////////////////////////////////

        protected float		m_CurrentAttackCooldown = 0;
        protected Animation m_AnimationController;
        protected Movable	m_Movable;

		///////////////////////////////////////////////////////////////////////////

		protected const string IDLE_ANIM_NAME	= "idle";
		protected const string WALK_ANIM_NAME	= "walk";
		protected const string ATTACK_ANIM_NAME	= "attack";

		///////////////////////////////////////////////////////////////////////////

		public EnemyType type { get { return m_Type; } }

		///////////////////////////////////////////////////////////////////////////

		protected void TryStartAnim(string animName, float speed = 1.0f, bool suppressedByAttack = true)
		{
            if (suppressedByAttack && m_AnimationController.IsPlaying(ATTACK_ANIM_NAME))
            {
				return;
			}

            AnimationState animationState = m_AnimationController[animName];
            if (animationState)
            {
                animationState.speed = speed;
			    m_AnimationController.Play(animName);
            }
            else
            {
                Debug.Log(gameObject + "does not have animation " + animName);
            }
		}

		///////////////////////////////////////////////////////////////////////////

		protected virtual void Awake()
		{
			m_AnimationController = GetComponentInChildren<Animation>();
			m_Movable = GetComponent<Movable>();
		}

		///////////////////////////////////////////////////////////////////////////

        protected virtual void Start()
        {
            m_RestingInfo.Init(false);

            TryStartAnim(IDLE_ANIM_NAME);
			
            EnemyManager.instance.RegisterEnemy(this, false);
            transform.SetParent(EnemyManager.instance.transform);
        }

		///////////////////////////////////////////////////////////////////////////

        void OnDisable()
        {
            EnemyManager.instance.RegisterEnemy(this, true);
        }

		///////////////////////////////////////////////////////////////////////////

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

		///////////////////////////////////////////////////////////////////////////

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

		///////////////////////////////////////////////////////////////////////////

		protected GameObject FindNextTarget()
		{
			float playerDistanceSq;
            float buildingDistanceSq;
            GameObject nearestPlayer = GetNearestPlayer(out playerDistanceSq);
            GameObject nearestBuilding = GetNearestBuilding(out buildingDistanceSq);
            GameObject nearestTarget = playerDistanceSq < buildingDistanceSq ? nearestPlayer : nearestBuilding;
            float nearestDistanceSq = Mathf.Min(playerDistanceSq, buildingDistanceSq);

            GameObject bestTarget = null;
            
            switch (m_TargetPreference)
            {
                case TargetPreference.Buildings:
                    bestTarget = nearestBuilding ? nearestBuilding : nearestPlayer;
                    break;

                case TargetPreference.Human:
                    bestTarget = nearestPlayer ? nearestPlayer : nearestBuilding;
                    break;

                case TargetPreference.QuestBuilding:
                    if (GameManager.instance.loseCondition == LoseCondition.DestroyObject)
                    {
                        bestTarget = GameManager.instance.loseConditionContextObject;
                    }
                    break;

                case TargetPreference.None:
                    break;
            }

            if (nearestTarget && nearestDistanceSq <= (m_AttackDistance * m_AttackDistance))
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

		///////////////////////////////////////////////////////////////////////////

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

        ///////////////////////////////////////////////////////////////////////////

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
   
		///////////////////////////////////////////////////////////////////////////

		protected void TryPerformInstantAttack(GameObject target)
		{
            if (m_CurrentAttackCooldown > 0)
            {
				return;
			}

            m_CurrentAttackCooldown = m_AttackCooldown;
            target.GetComponent<Attackable>().DealDamage(m_Damage, gameObject, gameObject);

			TryStartAnim(ATTACK_ANIM_NAME, 4.0f, false);
					
            AudioManager.instance.PlayAudio(m_HitSound, transform.position);

			if (m_BuffOnAttack && Random.Range(0.0001f, 1.0f) <= m_BuffOnAttackProbability)
			{
				Buffable targetBuffable = target.GetComponent<Buffable>();

				if (targetBuffable)
				{
					targetBuffable.AddBuff(m_BuffOnAttack);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

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
            if (m_CurrentAttackCooldown > 0)
            {
                m_CurrentAttackCooldown = Mathf.Max(m_CurrentAttackCooldown - Time.deltaTime, 0);
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
            m_RestingInfo.Tick();

			// 3) Main Update of Sub-Class
			OnUpdate();
            
			TryStartAnim(WALK_ANIM_NAME);
        }

		///////////////////////////////////////////////////////////////////////////

		protected abstract void OnUpdate();
    }

}