using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB
{

	public class Shooter : MonoBehaviour
    {
        private List<Weapon> m_WeaponsInInventory = new List<Weapon>();
        
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

        public void TryShoot(Vector3? projectileDirection = null)
        {
			if (currentWeapon == null)
			{
				return;
			}

			currentWeapon.TryShoot(this, transform.position, projectileDirection.HasValue ? projectileDirection.Value : this.transform.forward);

			if (!CanBeDisplayedAsActiveWeapon(currentWeapon))
			{
				CycleWeapons(false, true);
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
						weapon.ChangeAmmoCount(weaponWithAmmo.ammoCount);
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

		bool CanBeDisplayedAsActiveWeapon(Weapon weapon, bool onlyAcceptInfiniteAmmoGuns = false)
		{
			if (onlyAcceptInfiniteAmmoGuns)
			{
				return weapon.weaponData.infiniteAmmo;
			}
			else
			{
				return weapon.HasEnoughAmmoToShoot();
			}
		}

		///////////////////////////////////////////////////////////////////////////

        public bool CycleWeapons(bool positiveOrder, bool preferInfiniteAmmoWeapons)
        {
			// 1) Get Current Index
			int curIndex			= -1;
			int usableWeaponsCount	= 0;
			int tmpIndex			= -1;

			foreach (Weapon weapon in m_WeaponsInInventory)
			{
				if (!CanBeDisplayedAsActiveWeapon(weapon, preferInfiniteAmmoWeapons))
				{
					continue;
				}

				usableWeaponsCount++;
				tmpIndex++;
				
				if (weapon == currentWeapon)
				{
					curIndex = tmpIndex;
				}
			}

			if (usableWeaponsCount == 0)
			{
				if (preferInfiniteAmmoWeapons)
				{
					// perhaps we only did not succeed because our infinite-ammo-restriction?
					return CycleWeapons(positiveOrder, false);
				}

				currentWeapon = null;
				return false;
			}

			// 2) Chose next index
			int targetIndex;

			if (curIndex == -1)
			{
				targetIndex = positiveOrder ? 0 : usableWeaponsCount - 1;
			}
			else
			{
				targetIndex = curIndex + (positiveOrder ? 1 : -1);
				targetIndex = targetIndex % usableWeaponsCount;
			}

			if (targetIndex == curIndex)
			{
				return false;
			}

			// 3) Set next index
			tmpIndex = -1;
			foreach (Weapon weapon in m_WeaponsInInventory)
			{
				if (!CanBeDisplayedAsActiveWeapon(weapon, preferInfiniteAmmoWeapons))
				{
					continue;
				}

				tmpIndex++;
				
				if (tmpIndex == targetIndex)
				{
					currentWeapon = weapon;
					return true;
				}
			}

			Debug.Assert(false);
			return false;
        }
    }
}