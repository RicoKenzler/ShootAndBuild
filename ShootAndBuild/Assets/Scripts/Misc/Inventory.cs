using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SAB
{
    public enum InventorySelectionCategory
    {
        Item,
        Weapon,
        Building,

        Count,
    }

	///////////////////////////////////////////////////////////////////////////

    public class Inventory : MonoBehaviour
    {
        [SerializeField] private AudioData m_NotEnoughResourcesSound;

		///////////////////////////////////////////////////////////////////////////

        private Dictionary<ItemType, int> m_ItemCounts = new Dictionary<ItemType, int>();

        private Throwable		m_Throwable;
        private InputController m_InputController;
        private Attackable		m_Attackable;
        private PlayerMenu		m_PlayerMenu;
		private Buffable		m_Buffable;

		///////////////////////////////////////////////////////////////////////////

		public static Inventory sharedInventoryInstance { get; private set; }

		///////////////////////////////////////////////////////////////////////////

        // ReadOnly Dictionaries are not supported before .Net 4.5
        public Dictionary<ItemType, int> GetItemsReadOnly()
        {
            return m_ItemCounts;
        }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            m_InputController = GetComponent<InputController>();
            m_Attackable = GetComponent<Attackable>();
            m_PlayerMenu = GetComponent<PlayerMenu>();
            m_Throwable = GetComponent<Throwable>();
			m_Buffable = GetComponent<Buffable>();

            if (m_InputController)
            {
                // init start items
                Dictionary<ItemType, ItemData> itemInfos = ItemManager.instance.itemDataMap;

                foreach (KeyValuePair<ItemType, ItemData> item in itemInfos)
                {
                    if (item.Value.isShared || item.Value.initialCount <= 0)
                    {
                        continue;
                    }

                    AddItem(item.Value.itemType, item.Value.initialCount);
                }
            }
            else
            {
                sharedInventoryInstance = this;
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public void TriggerNotEnoughItemsSound()
        {
            // Only applicable for shared inventory (so we have ONE place where to balance it)
            Debug.Assert(!m_InputController);

            AudioManager.instance.PlayAudio(m_NotEnoughResourcesSound);
        }

		///////////////////////////////////////////////////////////////////////////

        public void TryUseActiveItem()
        {
            if (!m_InputController)
            {
                // not aplicable for shared inventory
                Debug.Assert(false);
                return;
            }

            PlayerPanel playerPanel = PlayerPanelGroup.instance.GetPlayerPanel(m_InputController.playerID);

            if ((GetItemCount(m_PlayerMenu.activeItemType) <= 0) && !CheatManager.instance.noResourceCosts)
            {
                // Item not usable
                sharedInventoryInstance.TriggerNotEnoughItemsSound();

                playerPanel.HighlightActiveItemCount();

                return;
            }

            // Use Item
            playerPanel.HighlightActiveItem();

            UseItem(m_PlayerMenu.activeItemType);

            if (!CheatManager.instance.noResourceCosts)
            {
                AddItem(m_PlayerMenu.activeItemType, -1);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        void UseItem(ItemType itemType, int count = 1)
        {
            ItemData itemInfos = ItemManager.instance.GetItemInfos(itemType);

            switch (itemInfos.usageCategory)
            {
                case ItemUsageCategory.PassiveItem:
                    Debug.Assert(false, "Passive Items are not usable");
                    break;
                case ItemUsageCategory.UsableItem:
                    m_Throwable.Throw();
                    break;
                case ItemUsageCategory.Weapon:
                    Debug.Assert(false, "Weapons are not usable");
                    break;
                case ItemUsageCategory.StatChanger:
                    switch (itemInfos.itemType)
                    {
                        case ItemType.CheeseHeal:
                            m_Attackable.Heal(1.0f);
                            break;

                        case ItemType.AppleHeal:
                            m_Attackable.Heal(0.25f);
                            break;

                        default:
                            Debug.LogWarning("Missing case statement for " + itemInfos.itemType);
                            break;
                    }
                    break;

                default:
                    Debug.LogWarning("Missing case statement for " + itemInfos.itemType);
                    break;
            }

			m_Buffable.AddBuffs(itemInfos.buffs);

            // Player Counter
            PlayerID? user = m_InputController ? (PlayerID?)m_InputController.playerID : null;
            CounterManager.instance.AddToCounters(user, CounterType.ItemsUsed, count, itemInfos.itemType.ToString());
        }

		///////////////////////////////////////////////////////////////////////////

        public void AddItem(ItemType itemType, int count)
        {
            ItemData itemInfos = ItemManager.instance.GetItemInfos(itemType);

            if (itemInfos.usageCategory == ItemUsageCategory.Weapon && count > 0) {

                Shootable shoot = this.GetComponent<Shootable>();
                if (shoot != null && itemInfos.weaponData != null)
                {
                    shoot.AddWeapon(itemInfos.weaponData);
                }
                return;
            }


            if (itemInfos.useOnCollect)
            {
                UseItem(itemType, count);
                return;
            }

            if (m_ItemCounts.ContainsKey(itemType))
            {
                m_ItemCounts[itemType] += count;
            }
            else
            {
                m_ItemCounts.Add(itemType, count);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public int GetItemCount(ItemType itemType)
        {
            int itemAmount = 0;

            if (!m_ItemCounts.TryGetValue(itemType, out itemAmount))
            {
                return 0;
            }

            return itemAmount;
        }
    }
}