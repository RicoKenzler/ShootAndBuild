using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

	public int startGold		= 10;
	public int startLives		= 4;
	public int startGrenades	= 4;

	public AudioClip[] noMoneySounds;

	private Throwable		throwable;
	private InputController inputController;

	// Use this for initialization
	void Start()
	{
		inputController = GetComponent<InputController>();

		if (inputController)
		{
			AddItem(ItemType.Granades,		startGrenades);

			throwable = GetComponent<Throwable>();
			activeItemType = ItemType.Granades;
		}
		else
		{
			sharedInventoryInstance = this;
			AddItem(ItemType.Gold,			startGold);
			AddItem(ItemType.ExtraLifes,	startLives);
		}

	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void TriggerNotEnoughItemsSound()
	{
		// Only applicable for shared inventory (so we have ONE place where to balance it)
		Debug.Assert(!inputController);

		float pitch = AudioManager.instance.GetRandomMusicalPitch();
		AudioManager.instance.PlayRandomOneShot(noMoneySounds, new OneShotParams(transform.position, 0.5f, false, 1.0f, pitch));
	}

	public void TryUseActiveItem()
	{
		if (!inputController)
		{
			// not aplicable for shared inventory
			Debug.Assert(false);
			return;
		}

		PlayerPanel playerPanel = PlayerPanelGroup.instance.GetPlayerPanel(inputController.playerID);

		if (GetItemCount(activeItemType) <= 0)
		{
			// Item not usable
			sharedInventoryInstance.TriggerNotEnoughItemsSound();
			
			playerPanel.HighlightActiveItemCount();

			return;
		}

		// Use Item
		playerPanel.HighlightActiveItem();

		if (activeItemType == ItemType.Granades)
		{
			AddItem(activeItemType, -1);

			if (throwable != null)
			{
				throwable.Throw();
			}
		}
	}

	public void AddItem(ItemType itemType, int count)
	{
		if (itemCounts.ContainsKey(itemType))
		{
			itemCounts[itemType] += count;
		}
		else
		{
			itemCounts.Add(itemType, count);
		}
	}

	public int GetItemCount(ItemType itemType)
	{
		int itemAmount = 0;

		if (!itemCounts.TryGetValue(itemType, out itemAmount))
		{
			return 0;
		}

		return itemAmount;
	}

	public ItemType activeItemType
	{
		get; private set;
	}

	public static Inventory sharedInventoryInstance
    {
        get; private set;
    }
}
