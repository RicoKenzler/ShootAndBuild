using UnityEngine;
using System.Collections.Generic;

namespace SAB
{

    public class Collectable : MonoBehaviour
    {
        public float collectRadius = 1.1f;
        public AudioData collectSound;
        public AudioData dropSound;

        public ItemType itemType = ItemType.Gold;

        [System.NonSerialized]
        public int amount = 1;

		public  bool vanishesAfterTimeout = true;
		private float spawnTime = 0.0f;
		private Material material;
		private Color    defaultColor;

        void Start()
        {
			material = GetComponentInChildren<Renderer>().material;
			defaultColor = material.color;

            AudioManager.instance.PlayAudio(dropSound, transform.position);
			spawnTime = Time.time;
        }

        void Update()
        {
            List<InputController> allPlayers = PlayerManager.instance.allAlivePlayers;

            Vector3 selfPosition = transform.position;

            for (int i = 0; i < allPlayers.Count; ++i)
            {
                GameObject player = allPlayers[i].gameObject;

                Vector3 differenceVector = (player.transform.position - selfPosition);
                differenceVector.y = 0.0f;

                if (differenceVector.sqrMagnitude <= (collectRadius * collectRadius))
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

			float lifetimeLeft = (spawnTime + ItemManager.instance.itemFadeOutTime) - Time.time;

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

				Color newColor = defaultColor;
				newColor.a = Mathf.Lerp(0.0f, defaultColor.a, alpha);

				material.color = newColor;
			}

			if (lifetimeLeft <= 0.0f)
			{
				Destroy(gameObject);
			}
        }

        public float targetHeight
        {
            get; set;
        }

        private void OnCollect(InputController player)
        {
            AudioManager.instance.PlayAudio(collectSound, transform.position);

            ItemData itemData = ItemManager.instance.GetItemInfos(itemType);

            Inventory inventory = itemData.isShared ? Inventory.sharedInventoryInstance : player.gameObject.GetComponent<Inventory>();
            inventory.AddItem(itemType, amount);

            Destroy(gameObject);
        }
    }
}