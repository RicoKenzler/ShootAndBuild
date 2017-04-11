using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	public int costs = 10;

	// Use this for initialization
	void Start ()
	{
		BuildingManager.instance.RegisterBuilding(this, false);
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void OnDestroy()
	{
		BuildingManager.instance.RegisterBuilding(this, true);
	}

	public void Pay()
	{
		Inventory.sharedInventoryInstance.AddItem(ItemType.Gold, -costs);
	}

	public bool IsPayable()
	{
		int goldAmount = Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold);
		if (goldAmount >= costs)
		{
			return true;
		}

		return false;
	}
}
