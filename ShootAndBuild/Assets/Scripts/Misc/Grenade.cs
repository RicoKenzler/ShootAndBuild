using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SAB
{
    public class Grenade : MonoBehaviour
    {
		///////////////////////////////////////////////////////////////////////////

        [SerializeField] private float	m_TimeToExplode		= 0.8f;
        [SerializeField] private float	m_MaxDamage			= 50.0f;
        [SerializeField] private float	m_MaxForce			= 50.0f;
        [SerializeField] private float	m_Radius			= 5.0f;
		[SerializeField] private bool	m_DoFriendlyFire	= false;

		[SerializeField] private List<BuffData> m_Buffs;

		[FormerlySerializedAs("explosionParticles")]
        [SerializeField] private ParticleSystem m_ExplosionParticles;

		[FormerlySerializedAs("explosionSound")]
        [SerializeField] private AudioData m_ExplosionSound;

		[FormerlySerializedAs("explosionShake")]
		[SerializeField] private CameraShakeParams m_ExplosionShake = new CameraShakeParams();

		///////////////////////////////////////////////////////////////////////////

        private Throwable m_Owner;
		private Faction m_OwnerFaction;

        private float m_ExplodeTimer	= 0.0f;
        private float m_RadiusSquared	= 0.0f;

		///////////////////////////////////////////////////////////////////////////

		public Throwable owner			{ get { return m_Owner; }			set { m_Owner = value; } }
		public Faction   ownerFaction	{ get { return m_OwnerFaction; }	set { m_OwnerFaction = value; } }

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
            m_ExplodeTimer = m_TimeToExplode;
            m_RadiusSquared = m_Radius * m_Radius;
        }

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            m_ExplodeTimer -= Time.deltaTime;

            if (m_ExplodeTimer <= 0)
            {
                Explode();
            }
        }

		///////////////////////////////////////////////////////////////////////////

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

		///////////////////////////////////////////////////////////////////////////

        public void Explode()
        {
            m_ExplodeTimer = 0;

            Vector3 selfPos = transform.position;

            // Note: we'll defer the damage dealing, because damage dealing can cause OnDisable, which
            // changes the allAttackables list during enumeration
            List<AttackableAndDamage> allDamages = new List<AttackableAndDamage>();

            foreach (Attackable attackable in AttackableManager.instance.allAttackables)
            {
				if (!m_DoFriendlyFire)
				{
					if (attackable.faction == m_OwnerFaction)
					{
						continue;
					}
				}

                float distanceSq = (attackable.transform.position - selfPos).sqrMagnitude;

                if (distanceSq < m_RadiusSquared)
                {
                    float dist = Mathf.Sqrt(distanceSq);

                    float f = 1.0f - (dist / m_Radius);
                    f = Mathf.Sqrt(f);

                    float damage	= f * m_MaxDamage;
                    float force		= f * m_MaxForce;

                    Vector3 direction	= attackable.transform.position - selfPos;
					
                    Vector3 impulse		= direction.normalized * force;

					// Let them fly up into the air!
					impulse += Vector3.up * force * 0.5f;

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

                damage.attackable.DealDamage(damage.damage, gameObject, m_Owner.gameObject, m_Buffs);
            }

            ParticleManager.instance.SpawnParticle(m_ExplosionParticles.gameObject, gameObject, transform.position, null, false, 10.0f, false, false);
            AudioManager.instance.PlayAudio(m_ExplosionSound, selfPos);
            InputManager.instance.SetVibrationAll(0.1f, 0.1f, 0.5f);
			CameraController.instance.AddCameraShake(m_ExplosionShake);

            Destroy(gameObject);
        }
    }

}