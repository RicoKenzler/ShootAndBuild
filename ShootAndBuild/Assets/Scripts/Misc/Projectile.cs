using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	public class Projectile : MonoBehaviour
	{
		[Tooltip("Speed in units per second")]
		[FormerlySerializedAs("speed")]
		[SerializeField] private float m_Speed = 5;

		[FormerlySerializedAs("range")]
		[SerializeField] private float m_Range;

		[FormerlySerializedAs("buffs")]
		[SerializeField] private List<BuffData> m_Buffs;

		[FormerlySerializedAs("ricochetEffect")]
		[SerializeField] private GameObject m_RicochetEffect;

		///////////////////////////////////////////////////////////////////////////

		private Vector3		m_StartPos;
		private Shooter	m_Owner;
		private Faction		m_OwnerFaction;   //< remember faction separately as Owner could have died when we need the info

		///////////////////////////////////////////////////////////////////////////

		public Vector3	direction	{ get; set; }
		public int		damage		{ get; set; }

		public Shooter owner {

			get { return m_Owner; } 

			set
			{
				m_Owner = value;
				m_OwnerFaction = m_Owner.GetComponent<Attackable>().faction;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		static GameObject s_ProjectileContainer_IfAny = null;
		const string PROJECTILES_CONTAINER_NAME = "Projectiles";

		///////////////////////////////////////////////////////////////////////////

		public static GameObject GetOrCreateProjectilesContainer()
		{
			if (!s_ProjectileContainer_IfAny)
			{
				s_ProjectileContainer_IfAny = new GameObject();
				s_ProjectileContainer_IfAny.name = PROJECTILES_CONTAINER_NAME;
			}

			return s_ProjectileContainer_IfAny;
		}

		///////////////////////////////////////////////////////////////////////////

		public void Init(float speed, float range, List<BuffData> buffs, GameObject riccochetEffect)
		{
			m_Speed				= speed;
			m_Range				= range;
			m_Buffs				= buffs;
			m_RicochetEffect	= riccochetEffect;
		}

		///////////////////////////////////////////////////////////////////////////

		void Start()
		{
			m_StartPos = transform.position;
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			float delta = Time.deltaTime * m_Speed;
			transform.Translate(delta * direction);

			if ((m_StartPos - transform.position).sqrMagnitude > m_Range * m_Range)
			{
				Destroy(gameObject);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void OnTriggerEnter(Collider other)
		{
			// we are immune to our own projectiles
			if (m_Owner && (m_Owner.gameObject == other.gameObject))
			{
				return;
			}

			Attackable targetAttackable = other.GetComponent<Attackable>();

			if (targetAttackable && (targetAttackable.faction == m_OwnerFaction))
			{
				// no friendly fire
				return;
			}

			if (other.gameObject.layer == 0)
			{
				//might work better in oncollision enter with collision normal
				ParticleManager.instance.SpawnParticle(m_RicochetEffect, ParticleManager.instance.gameObject, this.transform.position,
											Quaternion.LookRotation(this.transform.forward * -1f, Vector3.up), false, 2.0f, false, false);
			}
			
			if (targetAttackable != null)
			{
				targetAttackable.DealDamage(damage, gameObject, m_Owner ? m_Owner.gameObject : null, m_Buffs);
			}

			Destroy(gameObject);
		}
	}
}