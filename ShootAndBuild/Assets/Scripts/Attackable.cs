using UnityEngine;

public class Attackable : MonoBehaviour
{
    public int maxHealth = 10;
 
    public GameObject itemDropPrefab;
    public float itemDropPercentage = 0.5f;

    public AudioClip[] dieSounds;

    private int currentHealth = 0;


    void Start()
    {
        currentHealth = maxHealth;

        HealthBarManager.instance.AddHealthBar(this);
    }

    void OnDestroy()
    {
        HealthBarManager.instance.RemoveHealthBar(this);
    }

    public void DealDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            if (dieSounds.Length > 0)
            {
                int rndSoundIndex = Random.Range(0, dieSounds.Length - 1);
                AudioClip rndSound = dieSounds[rndSoundIndex];
                AudioSource.PlayClipAtPoint(rndSound, transform.position);
            }

            if (itemDropPrefab && (Random.Range(0.0f, 1.0f) <= itemDropPercentage))
            {
                GameObject itemInstance = Instantiate(itemDropPrefab);

                float dropHeight = 2.0f;

                itemInstance.transform.position = transform.position + new Vector3(0.0f, dropHeight, 0.0f);
                itemInstance.GetComponent<Collectable>().targetHeight = transform.position.y;
            }

			InputController inputController = GetComponent<InputController>();

            if (inputController)
            {
				// Prepare respawn
                currentHealth = maxHealth;

                float respawnRadius = 10.0f;
                transform.position = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) * respawnRadius;
                transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);

				PlayerManager.instance.OnPlayerDies(inputController.playerID);
				return;
            }
            else
            {
                Destroy(gameObject);
				return;
            }
        }
    }

    public int health
    {
        get { return currentHealth; }
    }
}
