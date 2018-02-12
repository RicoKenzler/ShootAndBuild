using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace SAB
{
	[System.Serializable]
	public struct ItemAndCount
	{
		public StorableItemData		itemData;
		public int					count;

		public ItemAndCount(StorableItemData _itemData, int _count)
		{
			itemData	= _itemData;
			count		= _count;
		}
	}

	///////////////////////////////////////////////////////////////////////////

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

        private Dictionary<StorableItemData, int> m_ItemCounts = new Dictionary<StorableItemData, int>();

        private Thrower			m_Throwable;
        private InputController m_InputController;
        private PlayerMenu		m_PlayerMenu;

		///////////////////////////////////////////////////////////////////////////

		public static Inventory sharedInventoryInstance { get; private set; }

		///////////////////////////////////////////////////////////////////////////

        // ReadOnly Dictionaries are not supported before .Net 4.5
        public Dictionary<StorableItemData, int> GetItemsReadOnly()
        {
            return m_ItemCounts;
        }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            m_InputController = GetComponent<InputController>();
            m_PlayerMenu = GetComponent<PlayerMenu>();
            m_Throwable = GetComponent<Thrower>();

            if (m_InputController)
            {
                // init start items
               StartItem[] startItems = ItemManager.instance.startItem;

                foreach (StartItem startItem in startItems)
                {
                    if (startItem.item.isShared || startItem.count <= 0)
                    {
                        continue;
                    }

                    ChangeItemCount(startItem.item, startItem.count);
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

            if ((GetItemCount(m_PlayerMenu.activeItemData) <= 0) && !CheatManager.instance.noResourceCosts)
            {
                // Item not usable
                sharedInventoryInstance.TriggerNotEnoughItemsSound();

                playerPanel.HighlightActiveItemCount();

                return;
            }

            // Use Item
            playerPanel.HighlightActiveItem();

            UseItem(m_PlayerMenu.activeItemData);

            ChangeItemCount(m_PlayerMenu.activeItemData, -1);
        }

		///////////////////////////////////////////////////////////////////////////

		private void UseItem(StorableItemData itemData)
		{
			bool itemWasUsed = false;

			ThrowableData throwable		= itemData.GetComponent<ThrowableData>();
			ConsumableData consumable	= itemData.GetComponent<ConsumableData>();

			if (throwable)
			{
				m_Throwable.Throw(throwable);
				itemWasUsed = true;
			}

			if (consumable)
			{
				consumable.Consume(gameObject);
				itemWasUsed = true;
			}

			Debug.Assert(itemWasUsed);
		}

		///////////////////////////////////////////////////////////////////////////

        public void ChangeItemCount(StorableItemData itemData, int count)
        {
			if (m_ItemCounts.ContainsKey(itemData))
			{
				m_ItemCounts[itemData] += count;
			}
			else
			{
				m_ItemCounts.Add(itemData, count);
			}

			if (m_ItemCounts[itemData] < 0)
			{
				Debug.Assert(CheatManager.instance.noResourceCosts);

				m_ItemCounts[itemData] = 0;
			}
        }

		///////////////////////////////////////////////////////////////////////////

        public int GetItemCount(StorableItemData itemType)
        {
			if (itemType == null)
			{
				Debug.Assert(false, "You did not specify an item");
				return 0;
			}

            int itemAmount = 0;

            if (!m_ItemCounts.TryGetValue(itemType, out itemAmount))
            {
                return 0;
            }

            return itemAmount;
        }

		///////////////////////////////////////////////////////////////////////////

		// null = all players pay
		public static bool CanBePaid(ItemAndCount cost, GameObject singlePayingPlayer = null)
		{
			// Cheat
			if (CheatManager.instance.noResourceCosts)
			{
				return true;
			}

			// Check
			if (cost.itemData.isShared)
			{
				// Can Global inventory afford?
				Inventory sharedInventory = Inventory.sharedInventoryInstance;
				return (sharedInventory.GetItemCount(cost.itemData) >= cost.count);
			}
			else
			{
				if (singlePayingPlayer)
				{
					// Can single player afford?
					Inventory playerInventory = singlePayingPlayer.GetComponent<Inventory>();
					return (playerInventory.GetItemCount(cost.itemData) >= cost.count);
				}
				else
				{
					foreach (InputController player in PlayerManager.instance.allDeadOrAlivePlayers)
					{
						Inventory playerInventory = player.GetComponent<Inventory>();
						if (playerInventory.GetItemCount(cost.itemData) >= cost.count)
						{
							continue;
						}

						// One player cannot afford this
						return false;
					}

					// all player can afford this
					return true;
				}
			}		
		}

		///////////////////////////////////////////////////////////////////////////

		// null = all players pay
		public static bool CanBePaid(ItemAndCount[] costs, GameObject singlePayingPlayer = null)
		{
			foreach (ItemAndCount cost in costs)
			{
				if (!CanBePaid(cost, singlePayingPlayer))
				{
					return false;
				}
			}

			return true;
		}

		///////////////////////////////////////////////////////////////////////////

		// null = all players pay
		public static void ChangeItemCount_AutoSelectInventories(ItemAndCount itemAndCount, bool subtract, GameObject singlePayingPlayer = null)
		{
			int signedItemCount = subtract ? -itemAndCount.count : itemAndCount.count;

			if (itemAndCount.itemData.isShared)
			{
				// Can Global inventory afford?
				Inventory sharedInventory = Inventory.sharedInventoryInstance;
				sharedInventory.ChangeItemCount(itemAndCount.itemData, signedItemCount);
			}
			else
			{
				if (singlePayingPlayer)
				{
					// Can single player afford?
					Inventory playerInventory = singlePayingPlayer.GetComponent<Inventory>();
					playerInventory.ChangeItemCount(itemAndCount.itemData, signedItemCount);
				}
				else
				{
					foreach (InputController player in PlayerManager.instance.allDeadOrAlivePlayers)
					{
						Inventory playerInventory = player.GetComponent<Inventory>();
						playerInventory.ChangeItemCount(itemAndCount.itemData, signedItemCount);
					}
				}
			}		
		}

		///////////////////////////////////////////////////////////////////////////

		// null = all players pay
		public static void ChangeItemCount_AutoSelectInventories(ItemAndCount[] itemsAndCounts, bool subtract, GameObject singlePayingPlayer = null)
		{
			foreach (ItemAndCount itemAndCount in itemsAndCounts)
			{
				ChangeItemCount_AutoSelectInventories(itemAndCount, subtract, singlePayingPlayer);
			}
		}

		///////////////////////////////////////////////////////////////////////////
    }
}