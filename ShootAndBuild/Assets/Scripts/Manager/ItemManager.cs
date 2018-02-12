using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

    [System.Serializable]
    public struct ItemDrop
    {
        public GameObject itemPrefab;
        public float dropProbability;
        public int minDropAmount;
        public int maxDropAmount;
    }

	[System.Serializable]
	public struct StartItem
	{
		public StorableItemData item;
		public int				count;
	}
	
	///////////////////////////////////////////////////////////////////////////

    public class ItemManager : MonoBehaviour
    {
        [SerializeField] private StartItem[]	m_StartItems;
		[SerializeField] private WeaponData[]	m_StartWeapons;

		[SerializeField] private float m_ItemFadeOutTime = 20.0f;

		///////////////////////////////////////////////////////////////////////////

		public float		itemFadeOutTime { get { return m_ItemFadeOutTime; } }
		public StartItem[]	startItem		{ get { return m_StartItems; }}
		public WeaponData[]	startWeapons	{ get { return m_StartWeapons; }}

		public static ItemManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

        void Start()
        {
            InitSharedStartGoods();
        }

		///////////////////////////////////////////////////////////////////////////

        void InitSharedStartGoods()
        {
            foreach (StartItem startItem in m_StartItems)
            {
				if (startItem.item.isShared)
				{
					Inventory.sharedInventoryInstance.ChangeItemCount(startItem.item, startItem.count);
				}
            }
        }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
        }
    }
}