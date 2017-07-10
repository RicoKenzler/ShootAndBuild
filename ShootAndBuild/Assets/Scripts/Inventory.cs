﻿using System.Collections.Generic;
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

    public class Inventory : MonoBehaviour
    {
        Dictionary<ItemType, int> itemCounts = new Dictionary<ItemType, int>();

        public int startGold = 10;
        public int startLives = 4;
        public int startGrenades = 4;

        public AudioData notEnoughResourcesSound;

        private Throwable throwable;
        private InputController inputController;
        private Attackable attackable;
        private PlayerMenu playerMenu;
		private Buffable buffable;

        // ReadOnly Dictionaries are not supported before .Net 4.5
        public Dictionary<ItemType, int> GetItemsReadOnly()
        {
            return itemCounts;
        }

        void Awake()
        {
            inputController = GetComponent<InputController>();
            attackable = GetComponent<Attackable>();
            playerMenu = GetComponent<PlayerMenu>();
            throwable = GetComponent<Throwable>();
			buffable = GetComponent<Buffable>();

            if (inputController)
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

        public void TriggerNotEnoughItemsSound()
        {
            // Only applicable for shared inventory (so we have ONE place where to balance it)
            Debug.Assert(!inputController);

            AudioManager.instance.PlayAudio(notEnoughResourcesSound);
        }

        public void TryUseActiveItem()
        {
            if (!inputController)
            {
                // not aplicable for shared inventory
                Debug.Assert(false);
                return;
            }

            PlayerPanel playerPanel = PlayerPanelGroup.instance.GetPlayerPanel(inputController.playerID);

            if ((GetItemCount(playerMenu.activeItemType) <= 0) && !CheatManager.instance.noResourceCosts)
            {
                // Item not usable
                sharedInventoryInstance.TriggerNotEnoughItemsSound();

                playerPanel.HighlightActiveItemCount();

                return;
            }

            // Use Item
            playerPanel.HighlightActiveItem();

            UseItem(playerMenu.activeItemType);

            if (!CheatManager.instance.noResourceCosts)
            {
                AddItem(playerMenu.activeItemType, -1);
            }
        }

        void UseItem(ItemType itemType, int count = 1)
        {
            ItemData itemInfos = ItemManager.instance.GetItemInfos(itemType);

            switch (itemInfos.usageCategory)
            {
                case ItemUsageCategory.PassiveItem:
                    Debug.Assert(false, "Passive Items are not usable");
                    break;
                case ItemUsageCategory.UsableItem:
                    throwable.Throw();
                    break;
                case ItemUsageCategory.Weapon:
                    Debug.Assert(false, "Weapons are not usable");
                    break;
                case ItemUsageCategory.StatChanger:
                    switch (itemInfos.itemType)
                    {
                        case ItemType.CheeseHeal:
                            attackable.Heal(1.0f);
                            break;

                        case ItemType.AppleHeal:
                            attackable.Heal(0.25f);
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

			buffable.AddBuffs(itemInfos.buffs);

            // Player Counter
            PlayerID? user = inputController ? (PlayerID?)inputController.playerID : null;
            CounterManager.instance.AddToCounters(user, CounterType.ItemsUsed, count, itemInfos.itemType.ToString());
        }

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

            if (itemCounts.ContainsKey(itemType))
            {
                itemCounts[itemType] += count;
            }
            else
            {
                itemCounts.Add(itemType, count);
            }
        }

        public int GetItemCount(ItemType itemType)
        {
            int itemAmount = 0;

            if (!itemCounts.TryGetValue(itemType, out itemAmount))
            {
                return 0;
            }

            return itemAmount;
        }

        public static Inventory sharedInventoryInstance
        {
            get; private set;
        }
    }


}