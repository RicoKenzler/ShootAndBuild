using UnityEngine;

public class Grenade : MonoBehaviour
{
	public float timeToExplode = 3.0f;
	public ParticleSystem explosionParticles;
	public float maxDamage = 10.0f;
	public float radius = 8.0f;

	private float explodeTimer = 0.0f;


	void Start()
	{
		explodeTimer = timeToExplode;
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

		ParticleManager.instance.SpawnParticle(explosionParticles, gameObject, transform.position, Quaternion.identity, false, 10.0f, false, false);



		Destroy(gameObject);
	}
}
