using UnityEngine;

namespace SAB
{

    public class Projectile : MonoBehaviour
    {
        [Tooltip("Speed in units per second")]
        public float speed = 5;

        public float range;

        private Vector3 startPos;


        void Start()
        {
            startPos = transform.position;
        }

        void Update()
        {
            float delta = Time.deltaTime * speed;
            transform.Translate(delta * Direction);

            if ((startPos - transform.position).sqrMagnitude > range* range)
            {
                Destroy(gameObject);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            // we are immune to our own projectiles
            if (owner && (owner.gameObject == other.gameObject))
            {
                return;
            }

            Attackable targetAttackable = other.GetComponent<Attackable>();

            if (targetAttackable && (targetAttackable.faction == ownerFaction))
            {
                // no friendly fire
                return;
            }

			if (other.gameObject.layer == 0)
            {
                //might work better in oncollision enter with collision normal
                ParticleManager.instance.SpawnParticle(ricochetEffect, ParticleManager.instance.gameObject, this.transform.position,
                                            Quaternion.LookRotation(this.transform.forward * -1f, Vector3.up), false, 2.0f, false, false);
            }

            Destroy(gameObject);

            if (targetAttackable != null)
            {
                targetAttackable.DealDamage(Damage, gameObject, owner ? owner.gameObject : null);
            }

        }

        public Vector3 Direction
        {
            get; set;
        }

        public int Damage
        {
            get; set;
        }

        private Shootable owner;
        private Faction ownerFaction;   //< remember faction separately as Owner could have died when we need the info
        public GameObject ricochetEffect;

        public Shootable Owner
        {
            get
            {
                return owner;
            }

            set
            {
                owner = value;
                ownerFaction = owner.GetComponent<Attackable>().faction;
            }
        }
    }
}