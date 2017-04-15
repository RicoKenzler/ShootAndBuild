﻿using System.Collections.Generic;
using UnityEngine;

public enum InventorySelectionCategory
{
	Item,
	Weapon,
	Building,

	Count,
}

public class Inventory : MonoBehaviour
{
	Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

	public int startGold		= 10;
	public int startLives		= 4;
	public int startGrenades	= 4;

	public AudioData notEnoughResourcesSound;
	public AudioData menuSelectionSound;

	private Throwable		throwable;
	private InputController inputController;
	private Attackable		attackable;

	[System.NonSerialized]
	public InventorySelectionCategory activeSelectionCategory = InventorySelectionCategory.Item;

	void Awake()
	{
		inputController = GetComponent<InputController>();
		attackable		= GetComponent<Attackable>();

		if (inputController)
		{
			// init start items
			Dictionary<ItemType, ItemData> itemInfos = ItemManager.instance.itemDataMap;

			activeItemType = ItemType.Granades;

			foreach (KeyValuePair<ItemType, ItemData> item in itemInfos)
			{
				if (item.Value.isShared || item.Value.initialCount <= 0)
				{
					continue;
				}

				AddItem(item.Value.itemType, item.Value.initialCount);
				activeItemType = item.Value.itemType;
			}

			throwable = GetComponent<Throwable>();
		}
		else
		{
			sharedInventoryInstance = this;
		}
	}

	// Use this for initialization
	void Start()
	{
	
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void TriggerNotEnoughItemsSound()
	{
		// Only applicable for shared inventory (so we have ONE place where to balance it)
		Debug.Assert(!inputController);

		AudioManager.instance.PlayAudio(notEnoughResourcesSound);
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

		UseItem(activeItemType);

		AddItem(activeItemType, -1);
	}

	void UseItem(ItemType itemType, int count = 1)
	{
		ItemData itemInfos = ItemManager.instance.GetItemInfos(itemType);

		switch (itemInfos.usageCategory)
		{
			case ItemUsageCategory.PassiveItem:
				Debug.Assert(false, "Passive Items are not usable");
				break;
			case ItemUsageCategory.Throwable:
				throwable.Throw();
				break;
			case ItemUsageCategory.Weapon:
				Debug.Assert(false, "Weapons are not usable");
				break;
			case ItemUsageCategory.StatChanger:
				switch (itemInfos.itemType)
				{
					case ItemType.FullHealth:
						attackable.Heal();
						break;

					default:
						Debug.LogWarning("Missing case statement for " + itemInfos.itemType);
						break;
				}
				break;

			default:
				Debug.LogWarning("Missing case statement for " + itemInfos.itemType);
				break;
		}
	}

	public void AddItem(ItemType itemType, int count)
	{
		ItemData itemInfos = ItemManager.instance.GetItemInfos(itemType);

		if (itemInfos.useOnCollect)
		{
			UseItem(itemType, count);
			return;
		}

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

	public void ChangeActiveItem(bool positiveOrder)
	{	
		
	}

	public void ChangeSelectionCategory(bool positiveOrder)
	{
		activeSelectionCategory += positiveOrder ? 1 : -1;

		if (activeSelectionCategory >= InventorySelectionCategory.Count)
		{
			activeSelectionCategory = 0;
		}
		else if (activeSelectionCategory < 0)
		{
			activeSelectionCategory = InventorySelectionCategory.Count - 1;
		}

		float halfTonesPositive = 24.0f;
		float halfTonesNegative = 21.0f;
		AudioManager.instance.PlayAudio(menuSelectionSound, null, positiveOrder ? halfTonesPositive : halfTonesNegative);
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
