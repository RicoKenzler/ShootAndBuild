using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB
{

    public class Shootable : MonoBehaviour
    {
        public WeaponData defaultWeapon;

        private List<WeaponData> arsenal = new List<WeaponData>();

        int currentWeaponIndex = 0;
        
        //----------------------------------------------------------------------

		public WeaponData CurrentWeapon
		{
			get; private set;
		}

        public float Cooldown
        {
            get
            {
                if (CurrentWeapon != null)
                {
                    return CurrentWeapon.Cooldown;
                }

                return float.MaxValue;
            }
        }

        //----------------------------------------------------------------------
        
        void Awake()
        {
            AddWeapon(defaultWeapon, true);
        }

        //----------------------------------------------------------------------

        void Start()
        {

        }

        //----------------------------------------------------------------------

        void Update()
        {
            //TODO move to physics timestep to be more precise
            CurrentWeapon.OnUpdate();
        }

        //----------------------------------------------------------------------

        public void Shoot(Quaternion? projectileDirection = null)
        {

            CurrentWeapon.Shoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.rotation);

        }

        //----------------------------------------------------------------------

        public void AddWeapon(WeaponData _newWeapon, bool setAsCurrent = false)
        {

            if ( !arsenal.Any(w => w.weaponID == _newWeapon.weaponID) ) {

                //Debug.Log("picked up " + _newWeapon.weaponID.ToString());
                //weapon is not contained -> add
                WeaponData weapon = UnityEngine.Object.Instantiate(_newWeapon);
                weapon.Init(this);
                arsenal.Add(weapon);

                if (setAsCurrent)
                {
                    this.CurrentWeapon = arsenal.Last();
                }

            } else
            {
                //weapon is contained
                //add ammo or something
            }

        }

        //----------------------------------------------------------------------

        public bool CycleWeapons(bool positiveOrder)
        {
            if (arsenal.Count > 1)
            {

                currentWeaponIndex += positiveOrder ? 1 : -1;

                if (currentWeaponIndex >= arsenal.Count)
                {
                    currentWeaponIndex = 0;
                } else if (currentWeaponIndex < 0)
                {
                    currentWeaponIndex = arsenal.Count - 1;
                }

                this.CurrentWeapon = arsenal[currentWeaponIndex];

                return true;        
            }

            return false;
        }
    }
}