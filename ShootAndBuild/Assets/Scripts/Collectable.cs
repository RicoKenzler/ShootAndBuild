using UnityEngine;
using System.Collections.Generic;

public class Collectable : MonoBehaviour
{
	public float collectRadius = 0.5f;
	public AudioData collectSound;
	public AudioData dropSound;

	public ItemType itemType	= ItemType.Gold;
	public int		amount		= 1;

	void Start()
	{
		AudioManager.instance.PlayAudio(dropSound, transform.position);
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
