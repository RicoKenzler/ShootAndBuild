using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlobalPanel : MonoBehaviour
{
	int lastGoldAmount	= -1;
	int lastLivesAmount = -1;

	public Text goldAmountText;
	public Text lifesAmountText;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		Inventory sharedInventory = Inventory.sharedInventoryInstance;

		int newGoldAmount = sharedInventory.GetItemCount(ItemType.Gold);

		if (newGoldAmount != lastGoldAmount)
		{
			goldAmountText.text = newGoldAmount.ToString();
			lastGoldAmount = newGoldAmount;
		}

		int newLivesAmount = sharedInventory.GetItemCount(ItemType.ExtraLifes);

		if (newLivesAmount != lastLivesAmount)
		{
			lifesAmountText.text = newLivesAmount.ToString();
			lastLivesAmount = newLivesAmount;
		}
	}
}
