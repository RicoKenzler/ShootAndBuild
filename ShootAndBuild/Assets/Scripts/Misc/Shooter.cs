using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB
{

	public class Shooter : MonoBehaviour
    {
        private List<Weapon> m_WeaponsInInventory = new List<Weapon>();

        private int m_CurrentWeaponIndex = 0;
        
        ///////////////////////////////////////////////////////////////////////////

		public Weapon currentWeapon { get; private set; }
		
		public float cooldown
        {
            get
            {
                if (currentWeapon != null)
                {
                    return currentWeapon.cooldown;
                }

                return float.MaxValue;
            }
        }

		///////////////////////////////////////////////////////////////////////////

		public void ReceiveStartWeapons()
		{
			Debug.Assert(m_WeaponsInInventory.Count == 0);

			foreach (WeaponWithAmmo weaponwithAmmo in ItemManager.instance.startWeapons)
			{
				AddWeapon(weaponwithAmmo);
			}
		}

        ///////////////////////////////////////////////////////////////////////////
		
        void Update()
        {
            //TODO move to physics timestep to be more precise
			if (currentWeapon != null)
			{
				currentWeapon.OnUpdate();
			}
        }

        ///////////////////////////////////////////////////////////////////////////

        public void Shoot(Quaternion? projectileDirection = null)
        {
			if (currentWeapon != null)
			{
				currentWeapon.TryShoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.rotation);
			}
        }

        ///////////////////////////////////////////////////////////////////////////

        public void AddWeapon(WeaponWithAmmo weaponWithAmmo, bool setAsCurrent = false)
        {
			foreach (Weapon weapon in m_WeaponsInInventory)
			{
				if (weapon.weaponData == weaponWithAmmo.weaponData)
				{
					if (!weapon.weaponData.infiniteAmmo)
					{
						weapon.AddAmmo(weaponWithAmmo.ammoCount);
					}
					return;
				}
			}

            Weapon newWeapon = new Weapon();
            newWeapon.Init(this, weaponWithAmmo.weaponData, weaponWithAmmo.ammoCount);
            m_WeaponsInInventory.Add(newWeapon);

            if (setAsCurrent || (currentWeapon == null) || !(currentWeapon.HasEnoughAmmoToShoot()))
            {
                currentWeapon = m_WeaponsInInventory.Last();
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public bool CycleWeapons(bool positiveOrder)
        {
            if (m_WeaponsInInventory.Count > 1)
            {
                m_CurrentWeaponIndex += positiveOrder ? 1 : -1;

                if (m_CurrentWeaponIndex >= m_WeaponsInInventory.Count)
                {
                    m_CurrentWeaponIndex = 0;
                } else if (m_CurrentWeaponIndex < 0)
                {
                    m_CurrentWeaponIndex = m_WeaponsInInventory.Count - 1;
                }

                currentWeapon = m_WeaponsInInventory[m_CurrentWeaponIndex];

                return true;        
            }

            return false;
        }
    }
}