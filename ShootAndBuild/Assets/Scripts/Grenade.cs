using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float timeToExplode = 3.0f;
	public ParticleSystem explosionParticles;
	public float maxDamage = 10.0f;
	public float radius = 8.0f;
	public AudioClip[] explosionSounds;

	private float explodeTimer = 0.0f;
	private float radiusSquared = 0.0f;


	void Start()
	{
		explodeTimer = timeToExplode;
		radiusSquared = radius * radius;
	}

	void Update()
	{
		explodeTimer -= Time.deltaTime;

		if (explodeTimer <= 0)
		{
			Explode();
		}
	}

	public void Explode()
	{
		explodeTimer = 0;

		Vector3 selfPos = transform.position;

		foreach (Attackable attackable in AttackableManager.instance.allAttackables)
		{
			float distanceSq = (attackable.transform.position - selfPos).sqrMagnitude;

			if (distanceSq < radiusSquared)
			{
				float dist = Mathf.Sqrt(distanceSq);
				float damage = dist / radius * maxDamage;
				attackable.DealDamage((int)damage, gameObject);
			}
		}

		ParticleManager.instance.SpawnParticle(explosionParticles, gameObject, transform.position, Quaternion.identity, false, 10.0f, false, false);
		AudioManager.instance.PlayRandomOneShot(explosionSounds, new OneShotParams(selfPos, 1.0f, true, 0.5f));

		Destroy(gameObject);
	}
}
