using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{

    public class PlayerMenu : MonoBehaviour
    {
        public AudioData menuSelectionRightSound;
        public AudioData menuSelectionLeftSound;
        public AudioData menuSelectionUpSound;
        public AudioData menuSelectionDownSound;
        public AudioData menuSelectionFailedSound;

        [System.NonSerialized]
        public InventorySelectionCategory activeSelectionCategory = InventorySelectionCategory.Item;

        private Inventory inventory;
        private Builder builder;
        private Shootable shootable;

        public float lastMenuInteractionTime
        {
            get; private set;
        }

        void Awake()
        {
            lastMenuInteractionTime = 0.0f;
            activeItemType = ItemType.None;
        }

        void InitActiveItemType()
        {
            Dictionary<ItemType, int> allItems = inventory.GetItemsReadOnly();

            activeItemType = ItemType.Granades;

            foreach (KeyValuePair<ItemType, int> item in allItems)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                ItemData itemData = ItemManager.instance.GetItemInfos(item.Key);

                if (itemData.usageCategory == ItemUsageCategory.UsableItem)
                {
                    activeItemType = item.Key;
                    break;
                }
            }
        }

        void InitActiveBuildingType()
        {
            if (builder.buildingPrefabs.Count == 0)
            {
                activeBuildingPrefab = null;
            }
            else
            {
                activeBuildingPrefab = builder.buildingPrefabs[0];
            }
        }

        void Start()
        {
            inventory = GetComponent<Inventory>();
            builder = GetComponent<Builder>();
            shootable = GetComponent<Shootable>();

            InitActiveItemType();
            InitActiveBuildingType();
        }

        void Update()
        {

        }

        private bool TryCycleThroughItems(bool positiveOrder)
        {
            // Not implemented yet
            return false;
        }

        private bool TryCycleThroughWeapons(bool positiveOrder)
        {

            return shootable.CycleWeapons(positiveOrder);
        }

        private bool TryCycleThroughBuildings(bool positiveOrder)
        {
            bool foundCurrentBuilding = false;

            if (positiveOrder)
            {
                Building validBuildingAfterCurrent = null;

                for (int i = 0; i <= 1; ++i)
                {
                    foreach (Building building in builder.buildingPrefabs)
                    {
                        if (foundCurrentBuilding)
                        {
                            validBuildingAfterCurrent = building;
                            i = 2;
                            break;
                        }

                        if (building == activeBuildingPrefab)
                        {
                            foundCurrentBuilding = true;
                        }
                    }
                }

                if (validBuildingAfterCurrent && (validBuildingAfterCurrent != activeBuildingPrefab))
                {
                    activeBuildingPrefab = validBuildingAfterCurrent;
                    return true;
                }

                return false;
            }
            else
            {
                Building validBuildingBeforeCurrent = null;

                for (int i = 0; i <= 1; ++i)
                {
                    foreach (Building building in builder.buildingPrefabs)
                    {
                        if (building == activeBuildingPrefab)
                        {
                            if (foundCurrentBuilding)
                            {
                                i = 2;
                                break;
                            }

                            foundCurrentBuilding = true;
                        }

                        validBuildingBeforeCurrent = building;
                    }
                }

                if (validBuildingBeforeCurrent && (validBuildingBeforeCurrent != activeBuildingPrefab))
                {
                    activeBuildingPrefab = validBuildingBeforeCurrent;
                    return true;
                }

                return false;
            }
        }

        public void CycleThroughCategory(bool positiveOrder)
        {
            bool success = false;

            switch (activeSelectionCategory)
            {
                case InventorySelectionCategory.Item:
                    success = TryCycleThroughItems(positiveOrder);
                    break;
                case InventorySelectionCategory.Weapon:
                    success = TryCycleThroughWeapons(positiveOrder);
                    break;
                case InventorySelectionCategory.Building:
                    success = TryCycleThroughBuildings(positiveOrder);
                    break;
            }

            if (success)
            {
                AudioManager.instance.PlayAudio(positiveOrder ? menuSelectionUpSound : menuSelectionDownSound);
            }
            else
            {
                AudioManager.instance.PlayAudio(menuSelectionFailedSound);
            }

            lastMenuInteractionTime = Time.time;
        }

        public void ChangeSelectionCategory(bool positiveOrder)
        {
            activeSelectionCategory += positiveOrder ? 1 : -1;

            if (activeSelectionCategory >= InventorySelectionCategory.Count)
            {
                activeSelectionCategory = 0;
            }
            else if (activeSelectionCategory < 0)
            {
                activeSelectionCategory = InventorySelectionCategory.Count - 1;
            }

            AudioManager.instance.PlayAudio(positiveOrder ? menuSelectionRightSound : menuSelectionLeftSound);

            lastMenuInteractionTime = Time.time;
        }

        public ItemType activeItemType
        {
            get; private set;
        }

        public Building activeBuildingPrefab
        {
            get; private set;
        }
    }
}