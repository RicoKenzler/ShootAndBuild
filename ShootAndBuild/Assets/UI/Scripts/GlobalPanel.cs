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

	public Animator goldAmountAnimator;

	// Use this for initialization
	void Start ()
	{
		goldAmountAnimator = goldAmountText.GetComponent<Animator>();
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

			goldAmountAnimator.ResetTrigger("Grow");
			goldAmountAnimator.SetTrigger("Grow");
		}

		int newLivesAmount = sharedInventory.GetItemCount(ItemType.ExtraLifes);

		if (newLivesAmount != lastLivesAmount)
		{
			lifesAmountText.text = newLivesAmount.ToString() + "  Lifes";
			lastLivesAmount = newLivesAmount;
		}
	}
}
