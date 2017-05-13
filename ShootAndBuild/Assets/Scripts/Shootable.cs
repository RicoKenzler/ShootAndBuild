using UnityEngine;

namespace SAB
{

    public class Shootable : MonoBehaviour
    {
        public WeaponData defaultWeapon;
        
        private WeaponData currentWeapon = null;

        //----------------------------------------------------------------------

        public float Cooldown
        {
            get
            {
                if (currentWeapon != null)
                {
                    return currentWeapon.Cooldown;
                }

                return float.MaxValue;
            }
        }

        //----------------------------------------------------------------------
        
        void Awake()
        {
            SetWeapon(defaultWeapon);
        }

        //----------------------------------------------------------------------

        void Start()
        {

        }

        //----------------------------------------------------------------------

        void Update()
        {
            //TODO move to physics timestep to be more precise
            currentWeapon.OnUpdate();
        }

        //----------------------------------------------------------------------

        public void Shoot(Quaternion? projectileDirection = null)
        {

            currentWeapon.Shoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.rotation);

        }

        //----------------------------------------------------------------------

        public void SetWeapon(WeaponData _newWeapon)
        {

            //maybe do this differenly  - store weapns in list so instances of available weapons will be kept
            currentWeapon = Object.Instantiate(_newWeapon);
            currentWeapon.Init(this);
        }


    }
}