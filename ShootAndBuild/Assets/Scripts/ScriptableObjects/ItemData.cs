using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	[CreateAssetMenu(menuName = "Custom/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
        public ItemType itemType = ItemType.None;
        public ItemUsageCategory usageCategory = ItemUsageCategory.PassiveItem; //< what should we do with usagePrefab on itemUsage?

        public GameObject usagePrefab;								//< which item should we throw/use/activate on itemUsage?

        public bool isShared = false;                               //< does item go into shared inventory?
        public int initialCount = 0;                                //< whith how many items does player start?
        public bool useOnCollect = false;                           //< is the item used instantanously when collected?
        public Sprite icon;                                         //< icon to show in UI

        public WeaponData weaponData = null;                        // if the item is a wepon this is the weapon meta data

		public List<BuffData> buffs;
    }

}