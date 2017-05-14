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
            currentWeapon.OnUpdate();
        }

        //----------------------------------------------------------------------

        public void Shoot(Quaternion? projectileDirection = null)
        {

            currentWeapon.Shoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.rotation);

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
                    this.currentWeapon = arsenal.Last();
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

                this.currentWeapon = arsenal[currentWeaponIndex];

                return true;        
            }

            return false;
        }
    }
}