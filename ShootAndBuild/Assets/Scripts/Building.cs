using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	public int costs = 10;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void Pay()
	{
		Inventory.sharedInventoryInstance.AddItem(ItemType.Gold, -costs);
	}

	public bool IsPayable()
	{
		int goldAmount = Inventory.sharedInventoryInstance.GetItemAmount(ItemType.Gold);
		if (goldAmount >= costs)
		{
			return true;
		}

		return false;
	}
}
