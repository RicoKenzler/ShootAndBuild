﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    public class Building : MonoBehaviour
    {
		[FormerlySerializedAs("costs")]
        [SerializeField] private int		m_Costs = 10;

		[FormerlySerializedAs("icon")]
        [SerializeField] private Sprite		m_Icon;

        private Attackable	m_Attackable;
        private Renderer	m_ChildsRenderer;

        ///////////////////////////////////////////////////////////////////////////

		public Sprite icon { get { return m_Icon; } }

		///////////////////////////////////////////////////////////////////////////

        // Use this for initialization
        void Start()
        {
            BuildingManager.instance.RegisterBuilding(this, false);

            this.m_Attackable = this.GetComponent<Attackable>();
            this.m_ChildsRenderer = this.GetComponentInChildren<Renderer>();

            if (this.m_Attackable == null)
            {
                Debug.LogWarning("Building without attackable");
            }
            this.m_Attackable.OnDamage += OnDamage;
        }

        ///////////////////////////////////////////////////////////////////////////

        void OnDisable()
        {
            BuildingManager.instance.RegisterBuilding(this, true);
        }

        ///////////////////////////////////////////////////////////////////////////

        public void Pay()
        {
            if (CheatManager.instance.noResourceCosts)
            {
                return;
            }

            Inventory.sharedInventoryInstance.AddItem(ItemType.Gold, -m_Costs);
        }

        ///////////////////////////////////////////////////////////////////////////

        public bool IsPayable()
        {
            int goldAmount = Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold);
            if (goldAmount >= m_Costs)
            {
                return true;
            }

            if (CheatManager.instance.noResourceCosts)
            {
                return true;
            }

            return false;
        }

        ///////////////////////////////////////////////////////////////////////////

        private void OnDamage()
        {
            foreach (var m in m_ChildsRenderer.materials)
            {
                m.color = Color.Lerp(new Color(0.5f, 0.0f, 0.0f), Color.white, m_Attackable.healthNormalized);
            }
        }
    }
}