using UnityEngine;
using System.Collections.Generic;

namespace SAB
{

    public class Grenade : MonoBehaviour
    {
        public float timeToExplode = 2.0f;
        public ParticleSystem explosionParticles;
        public float maxDamage = 10.0f;
        public float maxForce = 10.0f;
        public float radius = 8.0f;
        public AudioData explosionSound;
        public Throwable owner;

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
            public Attackable attackable;
            public int damage;
            public Vector3 impulse;

            public AttackableAndDamage(Attackable _attackable, int _damage, Vector3 _impulse)
            {
                attackable = _attackable;
                damage = _damage;
                impulse = _impulse;
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

                    float f = 1.0f - (dist / radius);
                    f = Mathf.Sqrt(f);

                    float damage = f * maxDamage;
                    float force = f * maxForce;

                    Vector3 direction = attackable.transform.position - selfPos;
                    Vector3 impulse = direction.normalized * force;

                    allDamages.Add(new AttackableAndDamage(attackable, (int)damage, impulse));
                }
            }

            foreach (AttackableAndDamage damage in allDamages)
            {
                Movable movable = damage.attackable.gameObject.GetComponent<Movable>();
                if (movable != null)
                {
                    movable.impulseForce = damage.impulse;
                }

                damage.attackable.DealDamage(damage.damage, gameObject, owner.gameObject);
            }

            ParticleManager.instance.SpawnParticle(explosionParticles.gameObject, gameObject, transform.position, null, false, 10.0f, false, false);
            AudioManager.instance.PlayAudio(explosionSound, selfPos);
            InputManager.instance.SetVibrationAll(0.1f, 0.1f, 0.5f);

            Destroy(gameObject);
        }
    }

}