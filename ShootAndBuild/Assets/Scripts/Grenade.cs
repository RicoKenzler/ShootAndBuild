using UnityEngine;
using System.Collections.Generic;

public class Grenade : MonoBehaviour
{
	public float timeToExplode = 3.0f;
	public ParticleSystem explosionParticles;
	public float maxDamage = 10.0f;
	public float radius = 8.0f;
	public AudioData explosionSound;

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

	struct AttackableAndDamage
	{
		public Attackable	attackable;
		public int			damage;

		public AttackableAndDamage(Attackable _attackable, int _damage)
		{
			attackable = _attackable;
			damage = _damage;
		}
	};

	public void Explode()
	{
		explodeTimer = 0;

		Vector3 selfPos = transform.position;

		// Note: we'll defer the damage dealing, because damage dealing can cause OnDisable, which
		// changes the allAttackables list during enumeration
		List<AttackableAndDamage> allDamages = new List<AttackableAndDamage>();

		foreach (Attackable attackable in AttackableManager.instance.allAttackables)
		{
			float distanceSq = (attackable.transform.position - selfPos).sqrMagnitude;

			if (distanceSq < radiusSquared)
			{
				float dist = Mathf.Sqrt(distanceSq);
				float damage = dist / radius * maxDamage;
				allDamages.Add(new AttackableAndDamage(attackable, (int) damage));
			}
		}

		foreach (AttackableAndDamage damage in allDamages)
		{
			damage.attackable.DealDamage((int) damage.damage, gameObject);
		}

		ParticleManager.instance.SpawnParticle(explosionParticles, gameObject, transform.position, Quaternion.identity, false, 10.0f, false, false);
		AudioManager.instance.PlayAudio(explosionSound, selfPos);
		InputManager.instance.SetVibrationAll(0.1f, 0.1f, 0.5f);

		Destroy(gameObject);
	}
}
