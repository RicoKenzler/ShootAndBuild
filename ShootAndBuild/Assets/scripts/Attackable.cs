using UnityEngine;

public class Attackable : MonoBehaviour
{
    public int MaxHealth = 10;
    public bool respawnAfterDeath = true;

	public GameObject itemDropPrefab;
	public float itemDropPercentage = 0.5f;

    private int m_CurrentHealth = 0;

	public AudioClip[] DieSounds;

	void Start()
    {
        m_CurrentHealth = MaxHealth;

        HealthBarManager.Instance.AddHealthBar(this);
	}

    void OnDestroy()
    {
        HealthBarManager.Instance.RemoveHealthBar(this);
    }
	
    public void DealDamage(int damage)
    {
        m_CurrentHealth -= damage;

        if (m_CurrentHealth <= 0)
        {
			if (DieSounds.Length > 0)
			{
				int rndSoundIndex = Random.Range(0, DieSounds.Length -1);
				AudioClip rndSound = DieSounds[rndSoundIndex];
				AudioSource.PlayClipAtPoint(rndSound, transform.position);
			}

            Debug.Log("Attackable died!");

			if (itemDropPrefab && (Random.Range(0.0f, 1.0f) <= itemDropPercentage))
			{
				GameObject itemInstance = Instantiate(itemDropPrefab);

				float dropHeight = 2.0f;

				itemInstance.transform.position = transform.position + new Vector3(0.0f, dropHeight, 0.0f);
				itemInstance.GetComponent<Collectable>().SetTargetHeight(transform.position.y);
			}

            if (respawnAfterDeath)
            {
                m_CurrentHealth = MaxHealth;

                float respawnRadius = 10.0f;
                transform.position = new Vector3(Random.Range(-1.0f, 1.0f), 0.0f, Random.Range(-1.0f, 1.0f)) * respawnRadius;
                transform.rotation = Quaternion.Euler(0.0f, Random.Range(0.0f, 360.0f), 0.0f);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    public int Health
    {
        get { return m_CurrentHealth; }
    }
}
