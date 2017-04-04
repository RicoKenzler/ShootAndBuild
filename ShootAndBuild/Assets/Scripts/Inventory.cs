using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour
{
	Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

	// Use this for initialization
	void Start ()
	{
		if (!GetComponent<InputController>())
		{
			sharedInventoryInstance = this;
			AddItem(ItemType.Gold, 10);
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

	public int GetItemAmount(ItemType itemType)
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
