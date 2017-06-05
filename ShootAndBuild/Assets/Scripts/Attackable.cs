using UnityEngine;

namespace SAB
{
	public enum Faction
	{
		Player,
		Enemy
	}

	public class Attackable : MonoBehaviour
	{
		public int maxHealth = 10;
		public bool showHealthBar = true;
		public Faction faction = Faction.Enemy;

		public ItemDrop[] itemDrops;

		public AudioData dieSound;
		public AudioData spawnSound;

		public ParticleSystem dieParticles;
		public ParticleSystem damageParticles;

		public BloodDecal bloodDecal;

		public delegate void PlayerDiesEvent(PlayerID id);
		public event PlayerDiesEvent PlayerDies;

		public delegate void ReceiveDmage();
		public event ReceiveDmage OnDamage;

		private int currentHealth = 0;
		private InputController inputController;

		void Start()
		{
			Heal();

			PlaySpawnSound();

			inputController = GetComponent<InputController>();
		}

		public void Heal(float relativeAmount = 1.0f)
		{
			currentHealth += (int)Mathf.Round(maxHealth * relativeAmount);
			currentHealth = Mathf.Min(currentHealth, maxHealth);
		}

		void OnEnable()
		{
			RegisterHealthBar();
			AttackableManager.instance.RegisterAttackable(this, false);
		}

		void OnDisable()
		{
			RegisterHealthBar(true);
			AttackableManager.instance.RegisterAttackable(this, true);
		}

		public void RegisterHealthBar(bool unregister = false)
		{
			if (!showHealthBar || GetComponent<Building>())
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

		private void Die(GameObject damageDealerMedium, GameObject damageDealerActor)
		{
			// Drops
			DropItems();

			// Audio
			AudioManager.instance.PlayAudio(dieSound, transform.position);

			// Particles
			if (dieParticles)
			{
				Vector3 towardsEnemy = (damageDealerMedium.transform.position - transform.position);

				Quaternion rotationAwayFromEnemy = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, 1.0f), -towardsEnemy);

				Vector3 posOffset = new Vector3(0.0f, 1.0f, 0.0f);
				ParticleManager.instance.SpawnParticle(dieParticles.gameObject, gameObject, transform.position + posOffset, rotationAwayFromEnemy, false, 10.0f, true, false);
			}

			// Player Counter
			EnemyBehaviourBase enemy = GetComponent<EnemyBehaviourBase>();

			if (enemy)
			{
				InputController playerWhoKilledMe = damageDealerActor ? damageDealerActor.GetComponent<InputController>() : null;

				PlayerID? playerIDWhoKilledMe = playerWhoKilledMe ? (PlayerID?)playerWhoKilledMe.playerID : null;

				CounterManager.instance.AddToCounters(playerIDWhoKilledMe, CounterType.KilledEnemies, 1, enemy.type.ToString() );
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

				if (bloodDecal)
				{
					ani.ShowBloodDecal(bloodDecal.gameObject);
				}
			}
		}

		private void DropItems()
		{
			int itemToDrop = -1;
			float lowestProbability = 1.0f;

			for (int i = 0; i < itemDrops.Length; ++i)
			{
				if (itemDrops[i].dropProbability > lowestProbability)
				{
					// give lower probabilities higher prio, such that shitty items do not suppres rare items
					continue;
				}

				if ((Random.Range(0.0f, 1.0f) <= itemDrops[i].dropProbability))
				{
					itemToDrop = i;
				}
			}

			if (itemToDrop == -1)
			{
				return;
			}

			ItemDrop bestItemDrop = itemDrops[itemToDrop];

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

			newCollectable.targetHeight = transform.position.y;
			newCollectable.amount = itemAmount;
		}

		public void DealLethalDamage(GameObject damageDealerMedium, GameObject damageDealerActor)
		{
			DealDamage(currentHealth, damageDealerMedium, damageDealerActor);
		}

        /// <summary>
        /// reduces health of attackable, triggers effects etc
        /// </summary>
        /// <param name="damage"></param>
        /// <param name="damageDealerMedium"></param>
        /// <param name="damageDealerActor"></param>
        /// <returns>the damage actually dealt</returns>
		public int DealDamage(int damage, GameObject damageDealerMedium, GameObject damageDealerActor)
		{
            int damageDealt = 0;

			MusicManager.instance.SignalIsInCombat();

			if (CheatManager.instance.ultraHighDamage)
			{
				damage = maxHealth + 100000;
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
				float damagePercentage = (float)damage / (float)maxHealth;
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

			if (damageParticles)
			{
				Vector3 towardsEnemy = (damageDealerMedium.transform.position - transform.position);

				Quaternion rotationTowardsEnemy = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, 1.0f), towardsEnemy);

				Vector3 posOffset = new Vector3(0.0f, 1.0f, 0.0f);

				ParticleManager.instance.SpawnParticle(damageParticles.gameObject, gameObject, transform.position + posOffset, rotationTowardsEnemy, false, 3.0f, true, true);
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

		public void OnRespawn()
		{
			// Prepare respawn
			Heal();

			float respawnRadius = 10.0f;
			transform.position = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) * respawnRadius;
			transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

			RegisterHealthBar();

			PlaySpawnSound();
		}

		private void PlaySpawnSound()
		{
			AudioManager.instance.PlayAudio(spawnSound, transform.position);
		}

		public int Health
		{
			get { return currentHealth; }
		}

		public float HealthNormalized
		{
			get { return (float)this.currentHealth / (float)this.maxHealth; }
		}
	}
}