using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SAB
{
	public class Weapon
	{
		private int				m_LayerMask;
		private float			m_Cooldown	= 0f;
		private RaycastHit[]	m_Hits		= new RaycastHit[20];

		private Shooter			m_Owner;
		private WeaponData		m_WeaponData;

		///////////////////////////////////////////////////////////////////////////

		public float			cooldown				{ get { return m_Cooldown; }}
		public WeaponData		weaponData				{ get { return m_WeaponData; }}
		public int				ammoCount				{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		// see if we need this
		public void Init(Shooter _owner, WeaponData weaponData, int _ammoCount)
		{
			m_Cooldown		= 0;
			m_Owner			= _owner;
			m_WeaponData	= weaponData;
			ammoCount		= _ammoCount;

			// TODO put important layers and combinations into some sort of static layermanager
			m_LayerMask = (1 << 0) | (1 << 9);
		}

		///////////////////////////////////////////////////////////////////////////

		public bool	HasEnoughAmmoToShoot()
		{
			if (weaponData.infiniteAmmo)
			{
				return true;
			}

			if (CheatManager.instance.noResourceCosts)
			{
				return true;
			}

			return (ammoCount > 0); 
		}


		///////////////////////////////////////////////////////////////////////////

		public void TryShoot(Shooter _owner, Vector3 _origin, Quaternion _direction)
		{
			if (m_Cooldown > 0.0f)
			{
				return;
			}

			if (!HasEnoughAmmoToShoot())
			{
				return;
			}
			
			Vector3 shootHeightOffset = new Vector3(0.0f, 0.5f, 0.0f);
			_origin += shootHeightOffset;

			if (m_WeaponData.type == WeaponType.Projectile)
			{
				for (int i = 0; i < m_WeaponData.projectilesPerShot; ++i)
				{
					Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-m_WeaponData.spread * 0.5f, m_WeaponData.spread * 0.5f), Vector3.up);

					GameObject projectileContainer = Projectile.GetOrCreateProjectilesContainer();
					GameObject projectileGo = GameObject.Instantiate(m_WeaponData.projectile, projectileContainer.transform);
					projectileGo.transform.position = _origin;
					projectileGo.transform.rotation = dir;

					float speed = m_WeaponData.projectileSpeed + Random.Range(-m_WeaponData.projectileSpeed * m_WeaponData.projectileRandomSpeed, m_WeaponData.projectileSpeed * m_WeaponData.projectileRandomSpeed);

					Projectile proj = projectileGo.GetComponent<Projectile>();
					proj.direction = new Vector3(0.0f, 0.0f, 1.0f);
					proj.owner = _owner;
					proj.damage = m_WeaponData.damage;
					proj.Init(speed, m_WeaponData.range, new List<BuffData>(m_WeaponData.buffs), m_WeaponData.ricochetEffect);

					//TODO range of projectile
					//TODO damage type to projectile
				}
			}
			else if (m_WeaponData.type == WeaponType.Hitscan)
			{
				for (int i = 0; i < m_WeaponData.projectilesPerShot; ++i)
				{
					Quaternion dir = _direction * Quaternion.AngleAxis(Random.Range(-m_WeaponData.spread * 0.5f, m_WeaponData.spread * 0.5f), Vector3.up);

					//this is shit!
					for(int r = 0; r < m_Hits.Length; ++r)
					{
						m_Hits[r].distance = float.MaxValue;
					}

					//raycast goes here
					Vector3 rayDirection = dir * Vector3.forward;
					int hitCount = Physics.RaycastNonAlloc(_origin, rayDirection, m_Hits, m_WeaponData.range, m_LayerMask, QueryTriggerInteraction.Ignore);

					bool debugRay = false;

					if (debugRay)
					{
						Debug.DrawLine(_origin, _origin + rayDirection * m_WeaponData.range);
					}

					int damageToDeal = m_WeaponData.damage;

					Debug.DrawLine(_origin, _origin + dir * (Vector3.forward * m_WeaponData.range), Color.magenta, 1f);

					m_Hits = m_Hits.OrderBy(h => h.distance).ToArray(); //seems like the most unefficient way to do it

					if (m_WeaponData.damageType == DamageType.Direct)
					{
						Attackable attackable = null;
						for (int h = 0; h < hitCount; ++h)
						{
							if (m_Hits[h].transform != null && m_Hits[h].transform.gameObject.layer == 0)
							{
								//display ricochet effect
								if (m_WeaponData.ricochetEffect != null)
								{
									Debug.DrawLine(m_Hits[h].point, m_Hits[h].point + m_Hits[h].normal * 2f, Color.green, 5f);
									ParticleManager.instance.SpawnParticle(m_WeaponData.ricochetEffect, ParticleManager.instance.gameObject, m_Hits[h].point, 
																				Quaternion.LookRotation(m_Hits[h].normal, Vector3.up), false, 2.0f, false, false);

								}
								break;
							}

							if (m_Hits[h].rigidbody != null)
							{
								//Debug.Log(hits[h].rigidbody.gameObject.name);
								attackable = m_Hits[h].rigidbody.GetComponent<Attackable>();
								if (attackable != null)
								{
									damageToDeal -= attackable.DealDamage(damageToDeal, m_Owner.gameObject, m_Owner.gameObject, m_WeaponData.buffs);

									// je: I find it more appropriate, when damage is not consumed
									damageToDeal = m_WeaponData.damage;
								}

								if (damageToDeal <= 0)
								{
									break;
								}
							}

						}
					} else if (m_WeaponData.damageType == DamageType.Area)
					{
						if (hitCount > 0 && m_Hits[0].rigidbody != null)
						{
								// do area damage at psotion of first hit
						}

					}
				}
			}

			//recoil
			Mover movable = m_Owner.gameObject.GetComponent<Mover>();
			if (movable != null)
			{
				movable.impulseForce = _direction * (-Vector3.forward * m_WeaponData.recoilForce);
			}


			m_Cooldown = m_WeaponData.coolDownTime;

			//sound
			AudioManager.instance.PlayAudio(m_WeaponData.shootSound, _origin);
			CameraController.instance.AddCameraShake(m_WeaponData.shakeParams);

			//muzzleflash
			if (m_WeaponData.muzzleFlashEffect != null)
			{
				ParticleManager.instance.SpawnParticle(m_WeaponData.muzzleFlashEffect, _owner.gameObject, _origin, _direction, false, 1.0f, false, false);
			}

			// remove ammo
			if (!weaponData.infiniteAmmo)
			{
				AddAmmo(-1);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		//TODO move to physics timestep
		public void OnUpdate()
		{
			if (m_Cooldown > 0.0f)
			{
				m_Cooldown = Mathf.Max(m_Cooldown - Time.deltaTime, 0.0f);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void AddAmmo(int count)
		{
			Debug.Assert(!weaponData.infiniteAmmo);
			ammoCount += count;

			if (ammoCount < 0)
			{
				Debug.Assert(CheatManager.instance.noResourceCosts);

				ammoCount = 0;
			}
		}
	}
}

