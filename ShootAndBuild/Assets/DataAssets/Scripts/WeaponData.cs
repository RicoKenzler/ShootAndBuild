using UnityEngine;
using UnityEditor;
using System.Linq;

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

    //----------------------------------------------------------------------

    [CreateAssetMenu(menuName = "Custom/WeaponData", fileName = "WeaponData")]
    public class WeaponData : ScriptableObject
    {

        static int layerMask;

        public WeaponType type = WeaponType.Projectile;

        public DamageType damageType = DamageType.Direct;

        /// <summary>
        /// spread in degrees
        /// </summary>
        public float spread = 0;

        /// <summary>
        /// cooldown before next shot for pulsed weapons
        /// </summary>
        public float coolDownTime;

        public GameObject viewModel;

        public GameObject projectile = null;

        public GameObject muzzleFlashEffect;

        public AudioData shootSound;

        public int damage = 10;

        public float projectileSpeed;

        /// <summary>can be used to implement for example a shotgun</summary>
        public int projectilesPerShot = 1;

        [Tooltip("amount of default speed that will be added/subtracted from default speed" )]
        public float projectileRandomSpeed = 0;

        public float range = 50;

        public float areaDamage = 0;

        public float areaRadius = 0;

        public float recoilForce = 0;

        private float cooldown = 0f;

        private RaycastHit[] hits = new RaycastHit[20];

        private Shootable owner;

        //----------------------------------------------------------------------

        public float Cooldown
        {
            get
            {
                return cooldown;
            }
        }

        //----------------------------------------------------------------------

        // see if we need this
        public void Init(Shootable _owner)
        {
            this.cooldown = 0;
            this.owner = _owner;
            //TODO put important layers and combinations into some sort of static layermanager
            layerMask = (1 << 0) | (1 << 9);
        }

        //----------------------------------------------------------------------

        public void Shoot(Shootable _owner, Vector3 _origin, Quaternion _direction)
        {
            if (cooldown > 0.0f)
            {
                return;
            }

          

            Vector3 shootHeightOffset = new Vector3(0.0f, 0.5f, 0.0f);
            _origin += shootHeightOffset;


            if (type == WeaponType.Projectile)
            {
                for (int i = 0; i < projectilesPerShot; ++i)
                {
                    Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-spread * 0.5f, spread * 0.5f), Vector3.up);

                    GameObject projectileContainer = GameObject.Find("Projectiles");
                    GameObject projectileGo = Instantiate(projectile, projectileContainer.transform);
                    projectileGo.transform.position = _origin;
                    projectileGo.transform.rotation = dir;

                    Projectile proj = projectileGo.GetComponent<Projectile>();
                    proj.Direction = new Vector3(0.0f, 0.0f, 1.0f);
                    proj.Owner = _owner;
                    proj.Damage = damage;
                    proj.speed = projectileSpeed + Random.Range(-projectileSpeed * projectileRandomSpeed, projectileSpeed * projectileRandomSpeed);
                    //TODO range of projectile
                    //TODO damage type to projectile
                }
            }
            else if (type == WeaponType.Hitscan)
            {

                for (int i = 0; i < projectilesPerShot; ++i)
                {

                    Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-spread * 0.5f, spread * 0.5f), Vector3.up);

                    //raycast goes here
                    int hitCount = Physics.RaycastNonAlloc(_origin, dir * Vector3.forward, hits, range, layerMask);
                    //Debug.Log(hitCount);

                    int damageToDeal = damage;

                    Debug.DrawLine(_origin, _origin + dir * (Vector3.forward * range), Color.magenta, 1f);

                    hits = hits.OrderBy(h => h.distance).ToArray(); //seems like the most unefficient way to do it

                    if (damageType == DamageType.Direct)
                    {
                        Attackable attackable = null;
                        for (int h = 0; h < hitCount; ++h)
                        {
                            //if we found we hit something that blocks the shot
                            //break

                            if (hits[h].rigidbody != null)
                            {
                                //Debug.Log(hits[h].rigidbody.gameObject.name);
                                attackable = hits[h].rigidbody.GetComponent<Attackable>();
                                if (attackable != null)
                                {
                                    damageToDeal -= attackable.DealDamage(damageToDeal, owner.gameObject, owner.gameObject);
                                }

                                if (damageToDeal <= 0)
                                {
                                    break;
                                }
                            }

                        }
                    } else if (damageType == DamageType.Area)
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
                movable.impulseForce = _direction * (-Vector3.forward * recoilForce);
            }


            cooldown = coolDownTime;

            //sound
            AudioManager.instance.PlayAudio(shootSound, _origin);

            //muzzleflash
            if (muzzleFlashEffect != null)
            {
                ParticleManager.instance.SpawnParticle(muzzleFlashEffect, _owner.gameObject, _origin, _direction, false, 1.0f, true, true);
            }
        }

        //----------------------------------------------------------------------

        //TODO move to physics timestep
        public void OnUpdate()
        {
            if (cooldown > 0.0f)
            {
                cooldown = Mathf.Max(cooldown - Time.deltaTime, 0.0f);
            }
        }
    }

    //----------------------------------------------------------------------

    // TODO move to seperate script
    [CustomEditor(typeof(WeaponData))]
    public class WeaponDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            WeaponData editorTarget = (WeaponData)target;

            DrawDefaultInspector();
        }
    }
}