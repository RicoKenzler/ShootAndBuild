using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	public enum Faction
	{
		Player,
		Enemy
	}

	public class Attackable : MonoBehaviour
	{
		[FormerlySerializedAs("maxHealth")]
		[SerializeField] private int		m_MaxHealth		= 10;

		[FormerlySerializedAs("showHealthBar")]
		[SerializeField] private bool		m_ShowHealthBar	= true;

		[FormerlySerializedAs("faction")]
		[SerializeField] private Faction	m_Faction		= Faction.Enemy;

		[FormerlySerializedAs("itemDrops")]
		[SerializeField] private ItemDrop[] m_ItemDrops;

		[FormerlySerializedAs("dieSound")]
		[SerializeField] private AudioData	m_DieSound;

		[FormerlySerializedAs("spawnSound")]
		[SerializeField] private AudioData	m_SpawnSound;

		[FormerlySerializedAs("dieParticles")]
		[SerializeField] private ParticleSystem m_DieParticles;

		[FormerlySerializedAs("damageParticles")]
		[SerializeField] private ParticleSystem m_DamageParticles;

		[FormerlySerializedAs("bloodDecal")]
		[SerializeField] private BloodDecal m_BloodDecal;

		///////////////////////////////////////////////////////////////////////////

		public Faction	faction		{ get { return m_Faction;		} set { m_Faction = value; } }
		public int		maxHealth	{ get { return m_MaxHealth;		} }

		public int		health				{ get { return currentHealth; } }
		public float	healthNormalized	{ get { return (float)this.currentHealth / (float)this.m_MaxHealth; } }

		///////////////////////////////////////////////////////////////////////////

		public delegate void PlayerDiesEvent(PlayerID id);
		public event PlayerDiesEvent PlayerDies;

		public delegate void ReceiveDmage();
		public event ReceiveDmage OnDamage;

		public delegate void AttackableDiesEvent(Attackable attackable);
		public event AttackableDiesEvent OnAttackableDies;

		private int currentHealth = 0;
		private InputController inputController;

		private Buffable buffable;

		///////////////////////////////////////////////////////////////////////////

        void Start()
		{
			Heal();

			PlaySpawnSound();

			buffable = GetComponent<Buffable>();
			inputController = GetComponent<InputController>();
		}

		///////////////////////////////////////////////////////////////////////////

		public void Heal(float relativeAmount = 1.0f)
		{
			int hitpointDelta = (int)Mathf.Round(m_MaxHealth * relativeAmount);

			Heal(hitpointDelta);
		}

		///////////////////////////////////////////////////////////////////////////

		public void Heal(int hitpoints)
		{
			currentHealth = Mathf.Min(currentHealth + hitpoints, m_MaxHealth);
		}

		///////////////////////////////////////////////////////////////////////////

        void OnEnable()
		{
			RegisterHealthBar();
			AttackableManager.instance.RegisterAttackable(this, false);
		}

		///////////////////////////////////////////////////////////////////////////

		void OnDisable()
		{
			RegisterHealthBar(true);
			AttackableManager.instance.RegisterAttackable(this, true);
		}

		///////////////////////////////////////////////////////////////////////////

		public void RegisterHealthBar(bool unregister = false)
		{
			if (!m_ShowHealthBar)
			{
				return;
			}

			if (unregister)
			{
				HealthBarManager.instance.RemoveHealthBar(this);
			}
			else
			{
				HealthBarManager.instance.AddHealthBar(this);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private void Die(GameObject damageDealerMedium, GameObject damageDealerActor)
		{
			// Drops
			DropItems();

			// Audio
			AudioManager.instance.PlayAudio(m_DieSound, transform.position);

			// Particles
			if (m_DieParticles)
			{
				Vector3 towardsEnemy = (damageDealerMedium.transform.position - transform.position);

				Quaternion rotationAwayFromEnemy = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, 1.0f), -towardsEnemy);

				Vector3 posOffset = new Vector3(0.0f, 1.0f, 0.0f);
				ParticleManager.instance.SpawnParticle(m_DieParticles.gameObject, gameObject, transform.position + posOffset, rotationAwayFromEnemy, false, 10.0f, true, false);
			}

			// Player Counter
			EnemyBehaviourBase enemy = GetComponent<EnemyBehaviourBase>();

			if (enemy)
			{
				InputController playerWhoKilledMe = damageDealerActor ? damageDealerActor.GetComponent<InputController>() : null;

				PlayerID? playerIDWhoKilledMe = playerWhoKilledMe ? (PlayerID?)playerWhoKilledMe.playerID : null;

				CounterManager.instance.AddToCounters(playerIDWhoKilledMe, CounterType.KilledEnemies, 1, enemy.type.ToString() );
			}

			if (OnAttackableDies != null)
			{
				OnAttackableDies(this);
			}

			// Execute Die
			if (inputController)
			{
				PlayerDies(inputController.playerID);
			}
			else if (GetComponent<Building>())
			{
				Destroy(gameObject);
			}
			else
			{
				DieAnimation ani = gameObject.AddComponent<DieAnimation>();

				if (m_BloodDecal)
				{
					ani.ShowBloodDecal(m_BloodDecal.gameObject);
				}

				ani.Init();
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private void DropItems()
		{
			int itemToDrop = -1;
			float lowestProbability = 1.0f;

			for (int i = 0; i < m_ItemDrops.Length; ++i)
			{
				if (m_ItemDrops[i].dropProbability > lowestProbability)
				{
					// give lower probabilities higher prio, such that shitty items do not suppres rare items
					continue;
				}

				if ((Random.Range(0.0f, 1.0f) <= m_ItemDrops[i].dropProbability))
				{
					itemToDrop = i;
				}
			}

			if (itemToDrop == -1)
			{
				return;
			}

			ItemDrop bestItemDrop = m_ItemDrops[itemToDrop];

			int itemAmount = Random.Range(bestItemDrop.minDropAmount, bestItemDrop.maxDropAmount);

			if (itemAmount <= 0)
			{
				Debug.LogWarning("Unit drops 0 items");
				return;
			}

			GameObject itemInstance = Instantiate(bestItemDrop.itemPrefab, ItemManager.instance.transform);
			itemInstance.name = "Dropped " + bestItemDrop.itemPrefab.name;

			float dropHeight = 2.0f;

			itemInstance.transform.position = transform.position + new Vector3(0.0f, dropHeight, 0.0f);
			itemInstance.transform.rotation = Quaternion.AngleAxis(Random.Range(0.0f, 360.0f), new Vector3(0.0f, 1.0f, 0.0f));

			Collectable newCollectable = itemInstance.GetComponent<Collectable>();

			newCollectable.amount = itemAmount;
		}

		///////////////////////////////////////////////////////////////////////////

		public void DealLethalDamage(GameObject damageDealerMedium, GameObject damageDealerActor)
		{
			DealDamage(currentHealth, damageDealerMedium, damageDealerActor);
		}

		///////////////////////////////////////////////////////////////////////////

        /// reduces health of attackable, triggers effects etc
        /// <returns>the damage actually dealt</returns>
		public int DealDamage(int damage, GameObject damageDealerMedium, GameObject damageDealerActor, List<BuffData> buffs = null)
		{
            int damageDealt = 0;

			MusicManager.instance.SignalIsInCombat();

			if (CheatManager.instance.ultraHighDamage)
			{
				damage = m_MaxHealth + 100000;
			}

			Buffable buffableActor = damageDealerActor ? damageDealerActor.GetComponent<Buffable>() : null;
			if (buffableActor)
			{
				damage = (int)(damage * buffableActor.GetDamageMultiplier());
            }

			if (buffs != null && buffable != null)
			{
				buffable.AddBuffs(buffs);
			}

			if (GameManager.instance.loseCondition == LoseCondition.DestroyObject && GameManager.instance.loseConditionContextObject == gameObject)
			{
				MusicManager.instance.SignalIsDanger();
			}
			
            int lastHealth = currentHealth;

            currentHealth -= damage;
            currentHealth = Mathf.Max(currentHealth, 0);

            damageDealt = lastHealth - currentHealth;

			if (OnDamage != null)
			{
				OnDamage();
			}

			if (inputController)
			{
				// vibrate for taking damage
				float damagePercentage = (float)damage / (float)m_MaxHealth;
				float vibrationAmount;

				if (currentHealth == 0)
				{
					vibrationAmount = 1.0f;
				}
				else
				{
					vibrationAmount = Mathf.Lerp(0.1f, 1.0f, damagePercentage);
				}

				Vector3 towardsDamageDealer3D = (damageDealerMedium.transform.position - transform.position);
				Vector2 towardsDamageDealer2D = new Vector2(towardsDamageDealer3D.x, towardsDamageDealer3D.z);
				float leftAmount = vibrationAmount;
				float rightAmount = vibrationAmount;

				float distToDamageDealer = towardsDamageDealer2D.magnitude;

				if (distToDamageDealer >= 0.01f)
				{
					towardsDamageDealer2D /= distToDamageDealer;

					// rightness in  left[0,1]right
					float directionalAmount = Mathf.Abs(towardsDamageDealer2D.x);

					float dampOppositeFactor = Mathf.Lerp(1.0f, 0.5f, directionalAmount);
					float boostFactor = Mathf.Lerp(1.0f, 2.0f, directionalAmount);

					leftAmount *= (towardsDamageDealer2D.x < 0) ? boostFactor : dampOppositeFactor;
					rightAmount *= (towardsDamageDealer2D.x > 0) ? boostFactor : dampOppositeFactor;

					leftAmount = Mathf.Clamp(leftAmount, 0.0f, 1.0f);
					rightAmount = Mathf.Clamp(rightAmount, 0.0f, 1.0f);
				}

				InputManager.instance.SetVibration(inputController.playerID, leftAmount, rightAmount, 0.3f);
			}

			if (m_DamageParticles)
			{
				Vector3 towardsEnemy = (damageDealerMedium.transform.position - transform.position);

				Quaternion rotationTowardsEnemy = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, 1.0f), towardsEnemy);

				Vector3 posOffset = new Vector3(0.0f, 1.0f, 0.0f);

				ParticleManager.instance.SpawnParticle(m_DamageParticles.gameObject, gameObject, transform.position + posOffset, rotationTowardsEnemy, false, 3.0f, true, true);
			}

			if (currentHealth <= 0)
			{
				if ((CheatManager.instance.invinciblePlayers && inputController)
				|| (CheatManager.instance.invincibleEnemies && GetComponent<EnemyBehaviourBase>())
				|| (CheatManager.instance.invincibleBuildings && GetComponent<Building>()))
				{
					currentHealth = 1;
				}
				else
				{
					Die(damageDealerMedium, damageDealerActor);
				}
			}

            return damageDealt;
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnRespawn(Vector3 position)
		{
			// Prepare respawn
			Heal();

			transform.position = position;
			transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

			RegisterHealthBar();

			PlaySpawnSound();
		}

		///////////////////////////////////////////////////////////////////////////

		private void PlaySpawnSound()
		{
			AudioManager.instance.PlayAudio(m_SpawnSound, transform.position);
		}
	}
}