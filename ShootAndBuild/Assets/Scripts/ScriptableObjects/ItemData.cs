using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	[CreateAssetMenu(menuName = "Custom/ItemData", fileName = "ItemData")]
    public class ItemData : ScriptableObject
    {
		[FormerlySerializedAs("itemType")]
        [SerializeField] private ItemType m_ItemType = ItemType.None;

		[FormerlySerializedAs("usageCategory")]
        [SerializeField] private ItemUsageCategory m_UsageCategory = ItemUsageCategory.PassiveItem; //< what should we do with usagePrefab on itemUsage?

		[FormerlySerializedAs("usagePrefab")]
        [SerializeField] private GameObject m_UsagePrefab;								//< which item should we throw/use/activate on itemUsage?

		[FormerlySerializedAs("isShared")]
        [SerializeField] private bool m_IsShared = false;                               //< does item go into shared inventory?

		[FormerlySerializedAs("initialCount")]
        [SerializeField] private int m_InitialCount = 0;                                //< whith how many items does player start?

		[FormerlySerializedAs("useOnCollect")]
        [SerializeField] private bool m_UseOnCollect = false;                           //< is the item used instantanously when collected?

		[FormerlySerializedAs("icon")]
        [SerializeField] private Sprite m_Icon;                                         //< icon to show in UI

		[FormerlySerializedAs("weaponData")]
        [SerializeField] private WeaponData m_WeaponData = null;                        // if the item is a wepon this is the weapon meta data

		[FormerlySerializedAs("buffs")]
		[SerializeField] private List<BuffData> m_Buffs;
		
		public ItemType				itemType		{ get { return m_ItemType; } }
		public ItemUsageCategory	usageCategory	{ get { return m_UsageCategory; } }
		public GameObject			usagePrefab		{ get { return m_UsagePrefab; } }
		public bool					isShared		{ get { return m_IsShared; } }
		public int					initialCount	{ get { return m_InitialCount; } }
		public bool					useOnCollect	{ get { return m_UseOnCollect; } }
		public Sprite				icon			{ get { return m_Icon; } }
		public WeaponData			weaponData		{ get { return m_WeaponData; } }
		public List<BuffData>		buffs			{ get { return m_Buffs; } }
    }

}