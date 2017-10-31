using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class Building : MonoBehaviour
    {
        public int costs = 10;
        public Sprite icon;

        private Attackable attackable;
        private Renderer childsRenderer;

        ///////////////////////////////////////////////////////////////////////////

        // Use this for initialization
        void Start()
        {
            BuildingManager.instance.RegisterBuilding(this, false);

            this.attackable = this.GetComponent<Attackable>();
            this.childsRenderer = this.GetComponentInChildren<Renderer>();

            if (this.attackable == null)
            {
                Debug.LogWarning("Building without attackable");
            }
            this.attackable.OnDamage += OnDamage;
        }

        ///////////////////////////////////////////////////////////////////////////

        // Update is called once per frame
        void Update()
        {

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

            Inventory.sharedInventoryInstance.AddItem(ItemType.Gold, -costs);
        }

        ///////////////////////////////////////////////////////////////////////////

        public bool IsPayable()
        {
            int goldAmount = Inventory.sharedInventoryInstance.GetItemCount(ItemType.Gold);
            if (goldAmount >= costs)
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
            foreach (var m in childsRenderer.materials)
            {
                m.color = Color.Lerp(new Color(0.5f, 0.0f, 0.0f), Color.white, attackable.HealthNormalized);
            }
        }
    }
}