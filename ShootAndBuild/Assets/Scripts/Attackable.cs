using UnityEngine;

public class Attackable : MonoBehaviour
{
    public int maxHealth = 10;
 
    public ItemDrop[]  itemDrops;

    public AudioClip[] dieSounds;
	public AudioClip[] spawnSounds;

    public event PlayerHandler PlayerDies;

    private int currentHealth = 0;
	private InputController inputController;

    void Start()
    {
        currentHealth = maxHealth;

        RegisterHealthBar();

		PlaySpawnSound();

		inputController = GetComponent<InputController>();
    }

	public void RegisterHealthBar(bool unregister = false)
	{
		if (unregister)
		{
			HealthBarManager.instance.RemoveHealthBar(this);
		}
		else
		{
			HealthBarManager.instance.AddHealthBar(this);
		}
	}

    void OnDestroy()
    {
		RegisterHealthBar(true);
    }

	private void Die()
	{
		if (dieSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, dieSounds.Length);
            AudioClip rndSound = dieSounds[rndSoundIndex];
            AudioManager.instance.PlayOneShot(rndSound, transform.position);
        }

		DropItems();

        if (inputController)
        {
            PlayerDies(inputController.playerID);
			RegisterHealthBar(true);

			return;
        }
        else
        {
            Destroy(gameObject);
			return;
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

		Collectable newCollectable = itemInstance.GetComponent<Collectable>();

		newCollectable.targetHeight = transform.position.y;
		newCollectable.amount = itemAmount;
	}

    public void DealDamage(int damage, GameObject damageDealer)
    {
        currentHealth -= damage;
		currentHealth = Mathf.Max(currentHealth, 0);

		if (inputController)
		{
			// vibrate for taking damage
			float damagePercentage = (float) damage / (float) maxHealth;
			float vibrationAmount;

			if (currentHealth == 0)
			{
				vibrationAmount = 1.0f;
			}
			else
			{
				vibrationAmount = Mathf.Lerp(0.1f, 1.0f, damagePercentage);
			}

			Vector3 towardsDamageDealer3D = (damageDealer.transform.position - transform.position);
			Vector2 towardsDamageDealer2D = new Vector2(towardsDamageDealer3D.x, towardsDamageDealer3D.z);
			float leftAmount  = vibrationAmount;
			float rightAmount = vibrationAmount;

			float distToDamageDealer = towardsDamageDealer2D.magnitude;

			if (distToDamageDealer >= 0.01f)
			{
				towardsDamageDealer2D /= distToDamageDealer;

				// rightness in  left[0,1]right
				float directionalAmount = Mathf.Abs(towardsDamageDealer2D.x);

				float dampOppositeFactor = Mathf.Lerp(1.0f, 0.5f, directionalAmount);
				float boostFactor		 = Mathf.Lerp(1.0f, 2.0f, directionalAmount);

				leftAmount  *= (towardsDamageDealer2D.x < 0) ? boostFactor : dampOppositeFactor;
				rightAmount *= (towardsDamageDealer2D.x > 0) ? boostFactor : dampOppositeFactor;

				leftAmount  = Mathf.Clamp(leftAmount, 0.0f, 1.0f);
				rightAmount = Mathf.Clamp(rightAmount, 0.0f, 1.0f);
			}
	
			PlayerManager.instance.SetVibration(inputController.playerID, leftAmount, rightAmount, 0.3f);
		}

        if (currentHealth <= 0)
        {
           Die();
        }
    }

	public void OnRespawn()
	{
		// Prepare respawn
        currentHealth = maxHealth;

        float respawnRadius = 10.0f;
        transform.position = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) * respawnRadius;
        transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

		RegisterHealthBar();

		PlaySpawnSound();
	}

	private void PlaySpawnSound()
	{
		if (spawnSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, spawnSounds.Length);
            AudioClip rndSound = spawnSounds[rndSoundIndex];
            AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f, true, 0.5f);
        }
	}

    public int health
    {
        get { return currentHealth; }
    }
}
