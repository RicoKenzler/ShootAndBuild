using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Serialization;

namespace SAB
{
    public class Builder : MonoBehaviour
    {
		[FormerlySerializedAs("buildingPrefabs")]
        [SerializeField] private List<Building> m_BuildingPrefabs;
        [SerializeField] private float			m_Distance		= 1.5f;

        [SerializeField] private AudioData		m_BuildSound;
        [SerializeField] private ParticleSystem m_BuildEffect;

        [SerializeField] private AudioData		m_NoSpaceSound;

        private PlayerMenu playerMenu = null;
		private Building preview = null;
		
		///////////////////////////////////////////////////////////////////////////

		public List<Building> buildingPrefabs { get { return m_BuildingPrefabs; } }

		///////////////////////////////////////////////////////////////////////////

        private void Start()
        {
            playerMenu = GetComponent<PlayerMenu>();
		}

		///////////////////////////////////////////////////////////////////////////

		void Update()
		{
			if (preview)
			{
				preview.transform.position = CalculateBuildPosition();
				preview.transform.position += new Vector3(0.0f, 0.1f, 0.0f);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void ShowBuildPreview()
		{
			Building activeBuilding = playerMenu.activeBuildingPrefab;
			if (!activeBuilding)
			{
				return;
			}

			preview = Instantiate(activeBuilding, BuildingManager.instance.transform);
			preview.gameObject.transform.position = CalculateBuildPosition();
			preview.SetBuildPreview(true);
		}

		///////////////////////////////////////////////////////////////////////////

		public void HideBuildPreview()
		{
			if (preview)
			{
				Destroy(preview.gameObject);
				preview = null;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void TryBuild()
        {
            Building activeBuilding = playerMenu.activeBuildingPrefab;
            if (!activeBuilding)
            {
                return;
            }

			Vector3 pos = CalculateBuildPosition();

			PlayerPanel playerPanel = PlayerPanelGroup.instance.GetPlayerPanel(GetComponent<InputController>().playerID);

            if (!Inventory.CanBePaid(activeBuilding.costs))
            {
                Inventory.sharedInventoryInstance.TriggerNotEnoughItemsSound();

                GlobalPanel.instance.HighlightMoney();
				playerPanel.HightlightBuildingCostCount();
                return;
            }

            if (!BlockerGrid.instance.IsFree(activeBuilding.gameObject, pos))
            {
                AudioManager.instance.PlayAudio(m_NoSpaceSound);
                return;
            }

            Build(activeBuilding, pos);

			playerPanel.HighlightActiveBuilding();
        }

		///////////////////////////////////////////////////////////////////////////

        private void Build(Building buildingPrefab, Vector3 pos)
        {
            GameObject newTower = Instantiate(buildingPrefab.gameObject, BuildingManager.instance.transform);
            newTower.transform.position = pos;
            newTower.GetComponent<Attackable>().faction = GetComponent<Attackable>().faction;

			AudioManager.instance.PlayAudio(m_BuildSound, transform.position);

            Inventory.ChangeItemCount_AutoSelectInventories(buildingPrefab.costs, true, gameObject);

            if (m_BuildEffect)
            {
                ParticleManager.instance.SpawnParticle(m_BuildEffect.gameObject, newTower, newTower.transform.position, null, false, 6.0f, true, false);
            }
        }

		///////////////////////////////////////////////////////////////////////////

		private Vector3 CalculateBuildPosition()
		{
			Vector3 pos = transform.position + transform.rotation * (m_Distance * Vector3.forward);
			pos = BlockerGrid.instance.ToTileCenter(pos);
			return pos;
		}
	}
}