using UnityEngine;

public class Shootable : MonoBehaviour
{
    public AudioClip[] shootSounds;
	public ParticleSystem shootEffect;

    public GameObject projectilePrefab;
    public float shootCooldown = 0.5f;

    public float flashDuration = 0.5f;
    public float flashMaxIntensity = 0.5f;

	public int damage = 1;
	
    void Start()
    {
		currentCooldown = 0.0f;
    }

    void Update()
    {
        if (currentCooldown > 0.0f)
        {
            currentCooldown = Mathf.Max(currentCooldown - Time.deltaTime, 0.0f);
        }
    }

    public void Shoot(Quaternion? projectileDirection = null)
    {
        if (currentCooldown > 0.0f)
        {
            return;
        }

        GameObject projectileContainer = GameObject.Find("Projectiles");

		Vector3 shootHeightOffset = new Vector3(0.0f, 0.5f, 0.0f);

        GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
        instance.transform.position = transform.position + shootHeightOffset;
		instance.transform.rotation = (projectileDirection.HasValue) ? projectileDirection.Value : transform.rotation;

        Projectile projectile = instance.GetComponent<Projectile>();
        projectile.direction = new Vector3(0.0f, 0.0f, 1.0f);
        projectile.owner = this;
		projectile.damage = damage;

        currentCooldown = shootCooldown;

        AudioManager.instance.PlayRandomOneShot(shootSounds, new OneShotParams(transform.position, 0.5f));

		if (shootEffect)
		{
			Vector3 towardsEnemy = instance.transform.forward;

			Quaternion rotationTowardsEnemy = Quaternion.FromToRotation(new Vector3(0.0f, 0.0f, 1.0f), towardsEnemy);

					

			ParticleManager.instance.SpawnParticle(shootEffect, gameObject, transform.position + shootHeightOffset, rotationTowardsEnemy, true, 4.0f, true, true);
		}
    }

	public float currentCooldown
	{
		get; private set;
	}
}
