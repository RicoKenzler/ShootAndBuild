using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

	public int startGold	= 10;
	public int startLives	= 4;

	// Use this for initialization
	void Start ()
	{
		if (!GetComponent<InputController>())
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

	public static Inventory sharedInventoryInstance
    {
        get; private set;
    }
}
