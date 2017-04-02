using UnityEngine;
using System.Collections.Generic;

public class Collectable : MonoBehaviour
{
	public float collectRadius = 0.5f;
	public AudioClip[] collectSounds;
	public AudioClip[] dropSounds;

	public ItemType itemType	= ItemType.Gold;
	public int		amount		= 1;

	void Start()
	{
		if (dropSounds.Length > 0)
		{
			int rndSoundIndex = Random.Range(0, dropSounds.Length);
			AudioClip rndSound = dropSounds[rndSoundIndex];
			AudioManager.instance.PlayOneShot(rndSound, transform.position);
		}
	}

	void Update()
	{
		List<GameObject> allPlayers = PlayerManager.instance.allPlayers;

		Vector3 selfPosition = transform.position;

		for (int i = 0; i < allPlayers.Count; ++i)
		{
			GameObject player = allPlayers[i];

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
	}

    public float targetHeight
    {
        get; set;
    }

    private void OnCollect(InputController player)
    {
        if (collectSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, collectSounds.Length);
            AudioClip rndSound = collectSounds[rndSoundIndex];
            AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
        }

		PlayerManager.instance.SetVibration(player.playerID, 1.0f, 1.0f, 0.10f);

		bool usesSharedInventory = ItemManager.instance.UsesSharedInventory(itemType);

		Inventory inventory = usesSharedInventory ? Inventory.sharedInventoryInstance : player.gameObject.GetComponent<Inventory>();
		inventory.AddItem(itemType, amount); 

        Destroy(gameObject);
    }
}
