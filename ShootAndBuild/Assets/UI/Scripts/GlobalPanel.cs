using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
    public class GlobalPanel : MonoBehaviour
    {
        int lastGoldAmount = -1;
        int lastLifesAmount = -1;

        public Text goldAmountText;
        public Text lifesAmountText;

        public Animator goldAmountAnimator;
        public Animator lifesAmountAnimator;

        void Awake()
        {
            instance = this;
        }

        // Use this for initialization
        void Start()
        {
            goldAmountAnimator = goldAmountText.GetComponent<Animator>();
            lifesAmountAnimator = lifesAmountText.GetComponent<Animator>();
        }

        // Update is called once per frame
        void Update()
        {
            Inventory sharedInventory = Inventory.sharedInventoryInstance;

            int newGoldAmount = sharedInventory.GetItemCount(ItemType.Gold);

            if (newGoldAmount != lastGoldAmount)
            {
                goldAmountText.text = newGoldAmount.ToString();
                lastGoldAmount = newGoldAmount;

                HighlightMoney();
            }

            int newLivesAmount = sharedInventory.GetItemCount(ItemType.ExtraLifes);

            if (newLivesAmount != lastLifesAmount)
            {
                lifesAmountText.text = newLivesAmount.ToString() + "  Lifes";
                lastLifesAmount = newLivesAmount;

                HighlightLifes();
            }
        }

        public void HighlightMoney()
        {
            goldAmountAnimator.SetTrigger("Grow");
        }

        public void HighlightLifes()
        {
            lifesAmountAnimator.SetTrigger("Grow");
        }

        public static GlobalPanel instance
        {
            get; private set;
        }
    }
}