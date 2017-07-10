using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{

    public enum ItemType
    {
        None = 0,

        Gold = 1,
        Granades = 2,
        ExtraLifes = 3,
        CheeseHeal = 4,
        AppleHeal = 5,

        WeaponDefault = 100,
        WeaponRailgun = 101,
    }

    public enum ItemUsageCategory
    {
        PassiveItem = 1,        //< only counts are relevant (e.g. gold, xtraLives)
        StatChanger = 2,        //< e.g. health pack

        UsableItem = 3,     //< e.g. grenade
        Weapon = 4,     //< e.g. shotgun
    }

    [System.Serializable]
    public struct ItemDrop
    {
        public GameObject itemPrefab;
        public float dropProbability;
        public int minDropAmount;
        public int maxDropAmount;
    }

    public class ItemManager : MonoBehaviour
    {
        [SerializeField]
        private ItemData[] allItemDatas;

		public float itemFadeOutTime = 10.0f;

        void Start()
        {
            InitItemDatasAndStartGoods();
        }

        void InitItemDatasAndStartGoods()
        {
            foreach (ItemData itemData in allItemDatas)
            {
                // 1) store into map
                if (itemDataMap.ContainsKey(itemData.itemType))
                {
                    Debug.LogWarning("Item type " + itemData.itemType + " is configured multiple times.");
                    continue;
                }

                itemDataMap[itemData.itemType] = itemData;

                // 2) add initialCount
                if (itemData.initialCount > 0)
                {
                    if (itemData.isShared)
                    {
                        // start items of other players are added in inventory creation
                        Inventory.sharedInventoryInstance.AddItem(itemData.itemType, itemData.initialCount);
                    }
                }
            }

            foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
            {
                if (!itemDataMap.ContainsKey(itemType))
                {
                    Debug.Log("You forgot to add " + itemType + " configuration to itemManager");
                }
            }
        }

        void Awake()
        {
            instance = this;
            itemDataMap = new Dictionary<ItemType, ItemData>();
        }

        public ItemData GetItemInfos(ItemType itemType)
        {
            ItemData outItemData;
            if (!itemDataMap.TryGetValue(itemType, out outItemData))
            {
                Debug.LogWarning("No item configured for itemType " + itemType);
            }

            return outItemData;
        }

        public static ItemManager instance
        {
            get; private set;
        }

        public Dictionary<ItemType, ItemData> itemDataMap
        {
            get; private set;
        }

    }
}