using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{	
	///////////////////////////////////////////////////////////////////////////

	[System.Serializable]
	public struct WeaponWithAmmo
	{
		public WeaponData	weaponData;
		public int			ammoCount;

		public WeaponWithAmmo(WeaponData _weaponData, int _ammoCount)
		{
			weaponData	= _weaponData;
			ammoCount	= _ammoCount;
		}
	}

	public enum WeaponType
    {
        None		= 0,
        Projectile	= 1,
        Hitscan		= 2,
    }

    public enum DamageType
    {
        Direct	= 0,
        Area	= 1,
    }

    public enum ShootType
    {
        Burst		= 0,
        Constant	= 1,
    }

	///////////////////////////////////////////////////////////////////////////

	public class WeaponData : MonoBehaviour
	{
        [SerializeField] private WeaponType			m_Type					= WeaponType.Projectile;
        [SerializeField] private DamageType			m_DamageType			= DamageType.Direct;
        [Tooltip("In Degrees" )]
        [SerializeField] private float				m_Spread				= 5.0f;
        [SerializeField] private float				m_CoolDownTime			= 1.0f;
        [SerializeField] private GameObject			m_Projectile			= null;
        [SerializeField] private GameObject			m_MuzzleFlashEffect;
        [SerializeField] private GameObject			m_RicochetEffect;
        [SerializeField] private AudioData			m_ShootSound;
		[SerializeField] private CameraShakeParams	m_ShakeParams;
        [SerializeField] private int				m_Damage				= 10;
        [SerializeField] private float				m_ProjectileSpeed;
        [SerializeField] private int				m_ProjectilesPerShot	= 1;
        [Tooltip("amount of default speed that will be added/subtracted from default speed" )]
        [SerializeField] private float				m_ProjectileRandomSpeed	= 0;
		[SerializeField] private float				m_Range					= 50;
        [SerializeField] private float				m_AreaDamage			= 0;
        [SerializeField] private float				m_AreaRadius			= 0;
        [SerializeField] private float				m_RecoilForce			= 0;
		[SerializeField] private bool				m_InfiniteAmmo			= false;
		[SerializeField] private List<BuffData>		m_Buffs;

		public WeaponType			type					{ get { return m_Type;					}}
		public DamageType			damageType				{ get { return m_DamageType;			}}
        public float				spread					{ get { return m_Spread;				}}
        public float				coolDownTime			{ get { return m_CoolDownTime;			}}
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
		public bool					infiniteAmmo			{ get { return m_InfiniteAmmo;			}}
	}
}