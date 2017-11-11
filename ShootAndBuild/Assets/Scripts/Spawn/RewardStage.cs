using System;
using UnityEngine;

namespace SAB.Spawn
{
	[Serializable]
	public class RewardStage : SpawnWaveStage
	{
		[SerializeField] private int m_Gold = 0;
		[SerializeField] private Collectable m_Reward = null;

		///////////////////////////////////////////////////////////////////////////

		public int gold				{ get { return m_Gold; }	set { m_Gold = value; } }
		public Collectable reward	{ get { return m_Reward; }	set { m_Reward = value; } }

		///////////////////////////////////////////////////////////////////////////

		public override bool isCompleted
		{
			get
			{
				return true;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public override void Start()
		{
			base.Start();

			if (m_Gold >= 0)
			{
				Inventory.sharedInventoryInstance.AddItem(ItemType.Gold, m_Gold);
			}

			ItemType rewardType = m_Reward ? m_Reward.itemType : ItemType.None;

			if (rewardType != ItemType.None)
			{
				ItemData itemData = ItemManager.instance.GetItemInfos(rewardType);

				if (itemData.isShared)
				{
					Inventory.sharedInventoryInstance.AddItem(rewardType, 1);
				}
				else
				{
					foreach (InputController player in PlayerManager.instance.allDeadOrAlivePlayers)
					{
						Inventory inventory = player.GetComponent<Inventory>();
						inventory.AddItem(rewardType, 1);
					}
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

	}
}