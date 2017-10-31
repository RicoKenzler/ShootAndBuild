using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
    public class GlobalPanel : MonoBehaviour
    {
        [SerializeField] private Text m_GoldAmountText;
        [SerializeField] private Text m_LifesAmountText;

		///////////////////////////////////////////////////////////////////////////

        private int m_LastGoldAmount = -1;
        private int m_LastLifesAmount = -1;
		
        private Animator m_GoldAmountAnimator;
        private Animator m_LifesAmountAnimator;

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }

		///////////////////////////////////////////////////////////////////////////
        
        void Start()
        {
            m_GoldAmountAnimator = m_GoldAmountText.GetComponent<Animator>();
            m_LifesAmountAnimator = m_LifesAmountText.GetComponent<Animator>();
        }

		///////////////////////////////////////////////////////////////////////////
       
        void Update()
        {
            Inventory sharedInventory = Inventory.sharedInventoryInstance;

            int newGoldAmount = sharedInventory.GetItemCount(ItemType.Gold);

            if (newGoldAmount != m_LastGoldAmount)
            {
                m_GoldAmountText.text = newGoldAmount.ToString();
                m_LastGoldAmount = newGoldAmount;

                HighlightMoney();
            }

            int newLivesAmount = sharedInventory.GetItemCount(ItemType.ExtraLifes);

            if (newLivesAmount != m_LastLifesAmount)
            {
                m_LifesAmountText.text = newLivesAmount.ToString() + "  Lifes";
                m_LastLifesAmount = newLivesAmount;

                HighlightLifes();
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightMoney()
        {
            m_GoldAmountAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public void HighlightLifes()
        {
            m_LifesAmountAnimator.SetTrigger("Grow");
        }

		///////////////////////////////////////////////////////////////////////////

        public static GlobalPanel instance
        {
            get; private set;
        }
    }
}