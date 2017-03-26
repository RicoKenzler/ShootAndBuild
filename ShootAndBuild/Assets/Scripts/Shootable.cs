using UnityEngine;

public class Shootable : MonoBehaviour
{
    public AudioClip[] shootSounds;
    public GameObject projectilePrefab;
    public float shootCooldown = 0.5f;

    public float flashDuration = 0.5f;
    public float flashMaxIntensity = 0.5f;

    private float currentCooldown = 0.0f;
    private float defaultLightIntensity = 0.0f;
    private float lastShootTime = -1000.0f;

    private Light playerLight;


    void Start()
    {
        playerLight = GetComponentInChildren<Light>();
        defaultLightIntensity = playerLight.intensity;
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

        playerLight.intensity = defaultLightIntensity + flashIntensity;
    }

    public void Shoot()
    {
        if (currentCooldown > 0.0f)
        {
            return;
        }

        GameObject projectileContainer = GameObject.Find("Projectiles");

        GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
        instance.transform.position = transform.position;

        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.direction = transform.forward;
        projectile.owner = this;

        currentCooldown = shootCooldown;
        lastShootTime = Time.time;

        if (shootSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, shootSounds.Length);
            AudioClip rndSound = shootSounds[rndSoundIndex];
			AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
        }
    }
}
