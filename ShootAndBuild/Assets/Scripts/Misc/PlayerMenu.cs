using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class PlayerMenu : MonoBehaviour
    {
        [SerializeField] private AudioData m_MenuSelectionRightSound;
        [SerializeField] private AudioData m_MenuSelectionLeftSound;
        [SerializeField] private AudioData m_MenuSelectionUpSound;
        [SerializeField] private AudioData m_MenuSelectionDownSound;
        [SerializeField] private AudioData m_MenuSelectionFailedSound;

		///////////////////////////////////////////////////////////////////////////
      
        private InventorySelectionCategory m_ActiveSelectionCategory = InventorySelectionCategory.Item;
        private Inventory inventory;
        private Builder builder;
        private Shooter shootable;

		///////////////////////////////////////////////////////////////////////////

		public InventorySelectionCategory	activeSelectionCategory		{ get { return m_ActiveSelectionCategory; } }
        public float						lastMenuInteractionTime		{ get; private set; }
		public StorableItemData				activeItemData				{ get; private set; }
        public Building						activeBuildingPrefab		{ get; private set; }
		public Weapon						activeWeapon				{ get { return shootable.currentWeapon; } }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
			inventory = GetComponent<Inventory>();
            builder = GetComponent<Builder>();
            shootable = GetComponent<Shooter>();

            lastMenuInteractionTime = 0.0f;
            activeItemData = null;
        }

		///////////////////////////////////////////////////////////////////////////

        void InitActiveItemType()
        {
            Dictionary<StorableItemData, int> allItems = inventory.GetItemsReadOnly();

            activeItemData = null;

            foreach (KeyValuePair<StorableItemData, int> item in allItems)
            {
                if (item.Value <= 0)
                {
                    continue;
                }

                if (item.Key.CanBeUsedActively())
                {
                    activeItemData = item.Key;
                    break;
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////

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

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
            InitActiveItemType();
            InitActiveBuildingType();
        }
		
		///////////////////////////////////////////////////////////////////////////

        private bool TryCycleThroughItems(bool positiveOrder)
        {
            // Not implemented yet
            return false;
        }

		///////////////////////////////////////////////////////////////////////////

        private bool TryCycleThroughWeapons(bool positiveOrder)
        {
            return shootable.CycleWeapons(positiveOrder);
        }

		///////////////////////////////////////////////////////////////////////////

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

		///////////////////////////////////////////////////////////////////////////

        public void CycleThroughCategory(bool positiveOrder)
        {
            bool success = false;

            switch (m_ActiveSelectionCategory)
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
                AudioManager.instance.PlayAudio(positiveOrder ? m_MenuSelectionUpSound : m_MenuSelectionDownSound);
            }
            else
            {
                AudioManager.instance.PlayAudio(m_MenuSelectionFailedSound);
            }

            lastMenuInteractionTime = Time.time;
        }

		///////////////////////////////////////////////////////////////////////////

        public void ChangeSelectionCategory(bool positiveOrder)
        {
            m_ActiveSelectionCategory += positiveOrder ? 1 : -1;

            if (m_ActiveSelectionCategory >= InventorySelectionCategory.Count)
            {
                m_ActiveSelectionCategory = 0;
            }
            else if (m_ActiveSelectionCategory < 0)
            {
                m_ActiveSelectionCategory = InventorySelectionCategory.Count - 1;
            }

            AudioManager.instance.PlayAudio(positiveOrder ? m_MenuSelectionRightSound : m_MenuSelectionLeftSound);

            lastMenuInteractionTime = Time.time;
        }	
    }
}