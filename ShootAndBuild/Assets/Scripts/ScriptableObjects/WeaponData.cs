using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    public enum WeaponType
    {
        None = 0,
        Melee = 1,
        Projectile = 2,
        Hitscan = 3,
        Trowable = 4, // grenades??
    }

    public enum DamageType
    {
        Direct = 0,
        Area = 1,
    }

    public enum ShootType
    {
        Burst = 0,
        Constant = 1,
    }

    ///////////////////////////////////////////////////////////////////////////

    [CreateAssetMenu(menuName = "Custom/WeaponData", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {
		[FormerlySerializedAs("weaponID")]
        [SerializeField] private ItemType m_WeaponID;

		[FormerlySerializedAs("type")]
        [SerializeField] private WeaponType m_Type = WeaponType.Projectile;

		[FormerlySerializedAs("damageType")]
        [SerializeField] private DamageType m_DamageType = DamageType.Direct;

        /// spread in degrees
		[FormerlySerializedAs("spread")]
        [SerializeField] private float m_Spread = 0;

        [FormerlySerializedAs("coolDownTime")]
        [SerializeField] private float m_CoolDownTime;

		[FormerlySerializedAs("viewModel")]
        [SerializeField] private GameObject m_ViewModel;

		[FormerlySerializedAs("projectile")]
        [SerializeField] private GameObject m_Projectile = null;

		[FormerlySerializedAs("muzzleFlashEffect")]
        [SerializeField] private GameObject m_MuzzleFlashEffect;

		[FormerlySerializedAs("ricochetEffect")]
        [SerializeField] private GameObject m_RicochetEffect;

		[FormerlySerializedAs("shootSound")]
        [SerializeField] private AudioData m_ShootSound;

		[FormerlySerializedAs("shakeParams")]
		[SerializeField] private CameraShakeParams m_ShakeParams;

		[FormerlySerializedAs("damage")]
        [SerializeField] private int m_Damage = 10;

		[FormerlySerializedAs("projectileSpeed")]
        [SerializeField] private float m_ProjectileSpeed;

        /// <summary>can be used to implement for example a shotgun</summary>
		[FormerlySerializedAs("projectilesPerShot")]
        [SerializeField] private int m_ProjectilesPerShot = 1;

        [Tooltip("amount of default speed that will be added/subtracted from default speed" )]
		[FormerlySerializedAs("projectileRandomSpeed")]
        [SerializeField] private float m_ProjectileRandomSpeed = 0;

		[FormerlySerializedAs("range")]
        [SerializeField] private float m_Range = 50;

		[FormerlySerializedAs("areaDamage")]
        [SerializeField] private float m_AreaDamage = 0;

		[FormerlySerializedAs("areaRadius")]
        [SerializeField] private float m_AreaRadius = 0;

		[FormerlySerializedAs("recoilForce")]
        [SerializeField] private float m_RecoilForce = 0;

		[FormerlySerializedAs("buffs")]
		[SerializeField] private List<BuffData> m_Buffs;

		///////////////////////////////////////////////////////////////////////////
		
		public ItemType				weaponID				{ get { return m_WeaponID;				}}
		public WeaponType			type					{ get { return m_Type;					}}
		public DamageType			damageType				{ get { return m_DamageType;			}}
        public float				spread					{ get { return m_Spread;				}}
        public float				coolDownTime			{ get { return m_CoolDownTime;			}}
		public GameObject			viewModel				{ get { return m_ViewModel;				}}
		public GameObject			projectile				{ get { return m_Projectile;			}}
		public GameObject			muzzleFlashEffect		{ get { return m_MuzzleFlashEffect;		}}
		public GameObject			ricochetEffect			{ get { return m_RicochetEffect;		}}
		public AudioData			shootSound				{ get { return m_ShootSound;			}}
		public CameraShakeParams	shakeParams				{ get { return m_ShakeParams;			}}
		public int					damage					{ get { return m_Damage;				}}
		public float				projectileSpeed			{ get { return m_ProjectileSpeed;		}}
        public int					projectilesPerShot		{ get { return m_ProjectilesPerShot;	}}
        public float				projectileRandomSpeed	{ get { return m_ProjectileRandomSpeed;	}}
		public float				range					{ get { return m_Range;					}}
		public float				areaDamage				{ get { return m_AreaDamage;			}}
		public float				areaRadius				{ get { return m_AreaRadius;			}}
		public float				recoilForce				{ get { return m_RecoilForce;			}}
		public List<BuffData>		buffs					{ get { return m_Buffs;					}}

		///////////////////////////////////////////////////////////////////////////

        private int layerMask;
        private float cooldown = 0f;

        private RaycastHit[] hits = new RaycastHit[20];

        private Shootable owner;

        ///////////////////////////////////////////////////////////////////////////

        public float Cooldown
        {
            get
            {
                return cooldown;
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        // see if we need this
        public void Init(Shootable _owner)
        {
            this.cooldown = 0;
            this.owner = _owner;
            //TODO put important layers and combinations into some sort of static layermanager
            layerMask = (1 << 0) | (1 << 9);
        }

        ///////////////////////////////////////////////////////////////////////////

        public void TryShoot(Shootable _owner, Vector3 _origin, Quaternion _direction)
        {
            if (cooldown > 0.0f)
            {
                return;
            }
			
            Vector3 shootHeightOffset = new Vector3(0.0f, 0.5f, 0.0f);
            _origin += shootHeightOffset;

            if (m_Type == WeaponType.Projectile)
            {
                for (int i = 0; i < m_ProjectilesPerShot; ++i)
                {
                    Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-m_Spread * 0.5f, m_Spread * 0.5f), Vector3.up);

                    GameObject projectileContainer = GameObject.Find("Projectiles");
                    GameObject projectileGo = Instantiate(m_Projectile, projectileContainer.transform);
                    projectileGo.transform.position = _origin;
                    projectileGo.transform.rotation = dir;

					float speed = m_ProjectileSpeed + Random.Range(-m_ProjectileSpeed * m_ProjectileRandomSpeed, m_ProjectileSpeed * m_ProjectileRandomSpeed);

                    Projectile proj = projectileGo.GetComponent<Projectile>();
                    proj.Direction = new Vector3(0.0f, 0.0f, 1.0f);
                    proj.Owner = _owner;
                    proj.Damage = m_Damage;
					proj.Init(speed, m_Range, new List<BuffData>(m_Buffs), m_RicochetEffect);

                    //TODO range of projectile
                    //TODO damage type to projectile
                }
            }
            else if (m_Type == WeaponType.Hitscan)
            {

                for (int i = 0; i < m_ProjectilesPerShot; ++i)
                {

                    Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-m_Spread * 0.5f, m_Spread * 0.5f), Vector3.up);

                    //this is shit!
                    for(int r = 0; r < hits.Length; ++r)
                    {
                        hits[r].distance = float.MaxValue;
                    }

                    //raycast goes here
                    int hitCount = Physics.RaycastNonAlloc(_origin, dir * Vector3.forward, hits, m_Range, layerMask, QueryTriggerInteraction.Ignore);

                    int damageToDeal = m_Damage;

                    Debug.DrawLine(_origin, _origin + dir * (Vector3.forward * m_Range), Color.magenta, 1f);

                    hits = hits.OrderBy(h => h.distance).ToArray(); //seems like the most unefficient way to do it

                    if (m_DamageType == DamageType.Direct)
                    {
                        Attackable attackable = null;
                        for (int h = 0; h < hitCount; ++h)
                        {
                            if (hits[h].transform != null && hits[h].transform.gameObject.layer == 0)
                            {
                                //display ricochet effect
                                if (m_RicochetEffect != null)
                                {
                                    Debug.DrawLine(hits[h].point, hits[h].point + hits[h].normal * 2f, Color.green, 5f);
                                    ParticleManager.instance.SpawnParticle(m_RicochetEffect, ParticleManager.instance.gameObject, hits[h].point, 
                                                                                Quaternion.LookRotation(hits[h].normal, Vector3.up), false, 2.0f, false, false);

                                }
                                break;
                            }

                            if (hits[h].rigidbody != null)
                            {
                                //Debug.Log(hits[h].rigidbody.gameObject.name);
                                attackable = hits[h].rigidbody.GetComponent<Attackable>();
                                if (attackable != null)
                                {
                                    damageToDeal -= attackable.DealDamage(damageToDeal, owner.gameObject, owner.gameObject, m_Buffs);

									// je: I find it more appropriate, when damage is not consumed
									damageToDeal = m_Damage;
                                }

                                if (damageToDeal <= 0)
                                {
                                    break;
                                }
                            }

                        }
                    } else if (m_DamageType == DamageType.Area)
                    {
                        if (hitCount > 0 && hits[0].rigidbody != null)
                        {
                             // do area damage at psotion of first hit
                        }

                    }
                }
            }

            //recoil
            Movable movable = owner.gameObject.GetComponent<Movable>();
            if (movable != null)
            {
                movable.impulseForce = _direction * (-Vector3.forward * m_RecoilForce);
            }


            cooldown = m_CoolDownTime;

            //sound
            AudioManager.instance.PlayAudio(m_ShootSound, _origin);
			CameraController.Instance.AddCameraShake(m_ShakeParams);

            //muzzleflash
            if (m_MuzzleFlashEffect != null)
            {
                ParticleManager.instance.SpawnParticle(m_MuzzleFlashEffect, _owner.gameObject, _origin, _direction, false, 1.0f, false, false);
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        //TODO move to physics timestep
        public void OnUpdate()
        {
            if (cooldown > 0.0f)
            {
                cooldown = Mathf.Max(cooldown - Time.deltaTime, 0.0f);
            }
        }
    }

    ///////////////////////////////////////////////////////////////////////////
}