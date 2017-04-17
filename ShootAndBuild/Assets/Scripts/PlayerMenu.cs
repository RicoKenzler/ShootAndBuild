using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMenu : MonoBehaviour 
{
	public AudioData menuSelectionRightSound;
	public AudioData menuSelectionLeftSound;
	public AudioData menuSelectionUpSound;
	public AudioData menuSelectionDownSound;
	public AudioData menuSelectionFailedSound;

	[System.NonSerialized]
	public InventorySelectionCategory activeSelectionCategory = InventorySelectionCategory.Item;

	private Inventory inventory;

	public float lastMenuInteractionTime = 0.0f;

	void Awake()
	{
		activeItemType = ItemType.None;
	}

	void InitActiveItemType()
	{
		Dictionary<ItemType, int> allItems = inventory.GetItemsReadOnly();

		activeItemType = ItemType.Granades;

		foreach (KeyValuePair<ItemType, int> item in allItems)
		{
			if (item.Value <= 0)
			{
				continue;
			}

			ItemData itemData = ItemManager.instance.GetItemInfos(item.Key);

			if (itemData.usageCategory == ItemUsageCategory.UsableItem)
			{
				activeItemType = item.Key;
				break;
			}
		}
	}

	void Start() 
	{
		inventory = GetComponent<Inventory>();

		InitActiveItemType();
	}
	
	void Update() 
	{
		
	}

	public void ChangeActiveWithinCategory(bool positiveOrder)
	{	
		bool success = false;

		switch (activeSelectionCategory)
		{
			case InventorySelectionCategory.Item:
				break;
			case InventorySelectionCategory.Weapon:
				break;
			case InventorySelectionCategory.Building:
				success = true;
				break;
		}

		if (success)
		{
			AudioManager.instance.PlayAudio(positiveOrder ? menuSelectionUpSound : menuSelectionDownSound);
		}
		else
		{
			AudioManager.instance.PlayAudio(menuSelectionFailedSound);
		}

		lastMenuInteractionTime = Time.time;
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

		AudioManager.instance.PlayAudio(positiveOrder ? menuSelectionRightSound : menuSelectionLeftSound);

		lastMenuInteractionTime = Time.time;
	}

	public ItemType activeItemType
	{
		get; private set;
	}
}
