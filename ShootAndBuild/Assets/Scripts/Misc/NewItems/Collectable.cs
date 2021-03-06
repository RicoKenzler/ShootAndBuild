﻿using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SAB
{
    public class Collectable : MonoBehaviour
    {
		[FormerlySerializedAs("collectRadius")]
        [SerializeField] private float m_CollectRadius = 1.1f;

		[FormerlySerializedAs("collectSound")]
        [SerializeField] private AudioData m_CollectSound;

		[FormerlySerializedAs("dropSound")]
        [SerializeField] private AudioData m_DropSound;

		[SerializeField] private GameObject collectableData;

		///////////////////////////////////////////////////////////////////////////

		private float		m_SpawnTime = 0.0f;
		private Material	m_Material;
		private Color		m_DefaultColor;
        private int			m_Count = 1;

		///////////////////////////////////////////////////////////////////////////

		public int amount			{ get { return m_Count; } set { m_Count = value; } }

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
			m_Material = GetComponentInChildren<Renderer>().material;
			m_DefaultColor = m_Material.color;

            AudioManager.instance.PlayAudio(m_DropSound, transform.position);
			m_SpawnTime = Time.time;
        }

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            List<InputController> allPlayers = PlayerManager.instance.allAlivePlayers;

            Vector3 selfPosition = transform.position;

            for (int i = 0; i < allPlayers.Count; ++i)
            {
                GameObject player = allPlayers[i].gameObject;

                Vector3 differenceVector = (player.transform.position - selfPosition);
                differenceVector.y = 0.0f;

                if (differenceVector.sqrMagnitude <= (m_CollectRadius * m_CollectRadius))
                {
                    OnCollect(player.GetComponent<InputController>());
                    return;
                }
            }

			float terrainHeight = TerrainManager.instance.GetInterpolatedHeight(selfPosition.x, selfPosition.z);

            if (selfPosition.y > terrainHeight)
            {
                selfPosition.y -= 0.1f;
                selfPosition.y = Mathf.Max(selfPosition.y, terrainHeight);
                transform.position = selfPosition;
            }

			float lifetimeLeft = (m_SpawnTime + ItemManager.instance.itemFadeOutTime) - Time.time;

			const float START_FADE_BEFORE_END = 7.0f;
			if (lifetimeLeft < START_FADE_BEFORE_END)
			{
				float fadeDuration = Mathf.Min(ItemManager.instance.itemFadeOutTime, START_FADE_BEFORE_END);
				float fadeoutAmount = 1.0f - (lifetimeLeft / fadeDuration);

				// fadeoutAmount = 0     -> t = 0
				// fadeoutAmount = 1     -> t = fadeDuration
				float blinkSpeed = (fadeoutAmount + 0.01f) * 3.0f * fadeDuration * Mathf.PI;

				float cosAlpha = Mathf.Abs(Mathf.Cos(fadeoutAmount * blinkSpeed));
				float alphaWithoutSin = (1.0f - fadeoutAmount);

				float alpha = Mathf.Lerp(alphaWithoutSin * 0.5f, alphaWithoutSin, cosAlpha);
				alpha = Mathf.Lerp(0, alphaWithoutSin, cosAlpha);

				Color newColor = m_DefaultColor;
				newColor.a = Mathf.Lerp(0.0f, m_DefaultColor.a, alpha);

				m_Material.color = newColor;
			}

			if (lifetimeLeft <= 0.0f)
			{
				Destroy(gameObject);
			}
        }

		///////////////////////////////////////////////////////////////////////////

        private void OnCollect(InputController player)
        {
			GameObject playerObject = player.gameObject;
			WeaponData weaponData	= collectableData.GetComponent<WeaponData>();

			int wasHandledCount			= 0;
			bool wasInstantleyConsumed	= false;

			// A) Weapon
			if (weaponData)
			{
				Shooter shooter = playerObject.GetComponent<Shooter>();
				shooter.AddWeapon(new WeaponWithAmmo(weaponData, m_Count));
				wasHandledCount++;
			}
            
			// B) Consume on collect
			ConsumableData consumableData = collectableData.GetComponent<ConsumableData>();

            if (consumableData && consumableData.useOnCollect)
            {
                consumableData.Consume(playerObject);
				wasHandledCount++;
				wasInstantleyConsumed = true;
            }

			// C) Storable Item
			StorableItemData storableData = collectableData.GetComponent<StorableItemData>();

			if (storableData)
			{
				Inventory inventory = storableData.isShared ? Inventory.sharedInventoryInstance : playerObject.GetComponent<Inventory>();
				
				inventory.ChangeItemCount(storableData, m_Count);
				wasHandledCount++;
			}

			Debug.Assert(wasHandledCount == 1);

			if (!wasInstantleyConsumed)
			{
				// Consume sound is more important than collect sound
				AudioManager.instance.PlayAudio(m_CollectSound, transform.position);
			}

            Destroy(gameObject);
        }
    }
}