using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB
{

	public class Shootable : MonoBehaviour
    {
        private List<WeaponData> m_Arsenal = new List<WeaponData>();

        private int m_CurrentWeaponIndex = 0;
        
        ///////////////////////////////////////////////////////////////////////////

		public WeaponData CurrentWeapon
		{
			get; private set;
		}

		///////////////////////////////////////////////////////////////////////////

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

        ///////////////////////////////////////////////////////////////////////////
        
        void Awake()
        {

        }

        ///////////////////////////////////////////////////////////////////////////
		
        void Update()
        {
            //TODO move to physics timestep to be more precise
			if (CurrentWeapon)
			{
				CurrentWeapon.OnUpdate();
			}
        }

        ///////////////////////////////////////////////////////////////////////////

        public void Shoot(Quaternion? projectileDirection = null)
        {
			if (CurrentWeapon)
			{
				CurrentWeapon.TryShoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.rotation);
			}
        }

        ///////////////////////////////////////////////////////////////////////////

        public void AddWeapon(WeaponData _newWeapon, bool setAsCurrent = false)
        {
            if ( !m_Arsenal.Any(w => w.weaponID == _newWeapon.weaponID) )
			{
                //Debug.Log("picked up " + _newWeapon.weaponID.ToString());
                //weapon is not contained -> add
                WeaponData weapon = Instantiate(_newWeapon);
                weapon.Init(this);
                m_Arsenal.Add(weapon);

                if (setAsCurrent || !this.CurrentWeapon)
                {
                    this.CurrentWeapon = m_Arsenal.Last();
                }

            } else
            {
                //weapon is contained
                //add ammo or something
            }

        }

        ///////////////////////////////////////////////////////////////////////////

        public bool CycleWeapons(bool positiveOrder)
        {
            if (m_Arsenal.Count > 1)
            {
                m_CurrentWeaponIndex += positiveOrder ? 1 : -1;

                if (m_CurrentWeaponIndex >= m_Arsenal.Count)
                {
                    m_CurrentWeaponIndex = 0;
                } else if (m_CurrentWeaponIndex < 0)
                {
                    m_CurrentWeaponIndex = m_Arsenal.Count - 1;
                }

                CurrentWeapon = m_Arsenal[m_CurrentWeaponIndex];

                return true;        
            }

            return false;
        }
    }
}