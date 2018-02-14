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

        private PlayerMenu playerMenu;
		
		///////////////////////////////////////////////////////////////////////////

		public List<Building> buildingPrefabs { get { return m_BuildingPrefabs; } }

		///////////////////////////////////////////////////////////////////////////

        private void Start()
        {
            playerMenu = GetComponent<PlayerMenu>();
        }

		///////////////////////////////////////////////////////////////////////////

        public void TryBuild()
        {
            Vector3 pos = transform.position + transform.rotation * (m_Distance * Vector3.forward);
            pos = BlockerGrid.instance.ToTileCenter(pos);

            Building activeBuilding = playerMenu.activeBuildingPrefab;

            if (!activeBuilding)
            {
                return;
            }

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

			Material material = newTower.GetComponentInChildren<MeshRenderer>().material;
			material.SetFloat("_Mode", 3.0f);
			material.color = new Color(1.0f, 0.0f, 1.0f, 0.4f);

			material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
			material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
			material.EnableKeyword("_ALPHABLEND_ON");
			material.renderQueue = 3000;

			AudioManager.instance.PlayAudio(m_BuildSound, transform.position);

            Inventory.ChangeItemCount_AutoSelectInventories(buildingPrefab.costs, true, gameObject);

            if (m_BuildEffect)
            {
                ParticleManager.instance.SpawnParticle(m_BuildEffect.gameObject, newTower, newTower.transform.position, null, false, 6.0f, true, false);
            }
        }
    }
}