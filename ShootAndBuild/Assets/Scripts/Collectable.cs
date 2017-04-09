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
		AudioManager.instance.PlayRandomOneShot(dropSounds, new OneShotParams(transform.position));
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
		AudioManager.instance.PlayRandomOneShot(collectSounds, new OneShotParams(transform.position));

		bool usesSharedInventory = ItemManager.instance.UsesSharedInventory(itemType);

		Inventory inventory = usesSharedInventory ? Inventory.sharedInventoryInstance : player.gameObject.GetComponent<Inventory>();
		inventory.AddItem(itemType, amount); 

        Destroy(gameObject);
    }
}
