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
		
		Debug.Log("Item " + itemType + ": " + itemCounts[itemType]);
	}

	public static Inventory sharedInventoryInstance
    {
        get; private set;
    }
}
