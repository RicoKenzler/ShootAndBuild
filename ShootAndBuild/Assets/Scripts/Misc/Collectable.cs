using UnityEngine;
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

		[FormerlySerializedAs("itemType")]
        [SerializeField] private ItemType m_ItemType = ItemType.Gold;

		///////////////////////////////////////////////////////////////////////////

		private float		m_SpawnTime = 0.0f;
		private Material	m_Material;
		private Color		m_DefaultColor;
        private int			m_Amount = 1;

		///////////////////////////////////////////////////////////////////////////

		public int amount { get { return m_Amount; } set { m_Amount = value; } }

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

            if (selfPosition.y > targetHeight)
            {
                selfPosition.y -= 0.1f;
                selfPosition.y = Mathf.Max(selfPosition.y, targetHeight);
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

        public float targetHeight
        {
            get; set;
        }

		///////////////////////////////////////////////////////////////////////////

        private void OnCollect(InputController player)
        {
            AudioManager.instance.PlayAudio(m_CollectSound, transform.position);

            ItemData itemData = ItemManager.instance.GetItemInfos(m_ItemType);

            Inventory inventory = itemData.isShared ? Inventory.sharedInventoryInstance : player.gameObject.GetComponent<Inventory>();
            inventory.AddItem(m_ItemType, m_Amount);

            Destroy(gameObject);
        }
    }
}