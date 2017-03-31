using UnityEngine;

public class Shootable : MonoBehaviour
{
    public AudioClip[] shootSounds;
    public GameObject projectilePrefab;
    public float shootCooldown = 0.5f;

    public float flashDuration = 0.5f;
    public float flashMaxIntensity = 0.5f;

	public int damage = 1;

    private float defaultLightIntensity = 0.0f;
    private float lastShootTime = -1000.0f;

    private Light playerLight;


    void Start()
    {
		currentCooldown = 0.0f;
        playerLight = GetComponentInChildren<Light>();
		if (playerLight)
		{
			defaultLightIntensity = playerLight.intensity;
		}
    }

    void Update()
    {
        if (currentCooldown > 0.0f)
        {
            currentCooldown = Mathf.Max(currentCooldown - Time.deltaTime, 0.0f);
        }

        float timeSinceLastShot = Time.time - lastShootTime;

        float flashIntensity = 1.0f - (timeSinceLastShot / flashDuration);
        flashIntensity *= flashMaxIntensity;

        flashIntensity = Mathf.Max(flashIntensity, 0.0f);

        flashIntensity = flashIntensity * flashIntensity;

		if (playerLight)
		{
			playerLight.intensity = defaultLightIntensity + flashIntensity;
		}
    }

    public void Shoot(Quaternion? projectileDirection = null)
    {
        if (currentCooldown > 0.0f)
        {
            return;
        }

        GameObject projectileContainer = GameObject.Find("Projectiles");

        GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
        instance.transform.position = transform.position + new Vector3(0.0f, 0.5f, 0.0f);
		instance.transform.rotation = (projectileDirection.HasValue) ? projectileDirection.Value : transform.rotation;

        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.direction = new Vector3(0.0f, 0.0f, 1.0f);
        projectile.owner = this;
		projectile.damage = damage;

        currentCooldown = shootCooldown;
        lastShootTime = Time.time;

        if (shootSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, shootSounds.Length);
            AudioClip rndSound = shootSounds[rndSoundIndex];
			AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
        }
    }

	public float currentCooldown
	{
		get; private set;
	}
}
