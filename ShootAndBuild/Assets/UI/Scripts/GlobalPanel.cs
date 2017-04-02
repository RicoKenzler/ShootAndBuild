using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalPanel : MonoBehaviour
{
	int lastGoldAmount = -1;

	public Text goldAmountText;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		Inventory sharedInventory = Inventory.sharedInventoryInstance;

		int newAmount = sharedInventory.GetItemAmount(ItemType.Gold);

		if (newAmount != lastGoldAmount)
		{
			goldAmountText.text = newAmount.ToString();
			lastGoldAmount = newAmount;
		}
	}
}
