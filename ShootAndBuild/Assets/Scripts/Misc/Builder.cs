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
            pos = Grid.instance.ToTileCenter(pos);

            Building activeBuilding = playerMenu.activeBuildingPrefab;

            if (!activeBuilding)
            {
                return;
            }

            if (!activeBuilding.IsPayable())
            {
                Inventory.sharedInventoryInstance.TriggerNotEnoughItemsSound();

                GlobalPanel.instance.HighlightMoney();
                return;
            }

            if (!Grid.instance.IsFree(activeBuilding.gameObject, pos))
            {
                AudioManager.instance.PlayAudio(m_NoSpaceSound);
                return;
            }

            Build(activeBuilding, pos);
        }

		///////////////////////////////////////////////////////////////////////////

        private void Build(Building buildingPrefab, Vector3 pos)
        {
            GameObject newTower = Instantiate(buildingPrefab.gameObject, BuildingManager.instance.transform);
            newTower.transform.position = pos;
            newTower.GetComponent<Attackable>().faction = GetComponent<Attackable>().faction;

            AudioManager.instance.PlayAudio(m_BuildSound, transform.position);

            buildingPrefab.Pay();

            if (m_BuildEffect)
            {
                ParticleManager.instance.SpawnParticle(m_BuildEffect.gameObject, newTower, newTower.transform.position, null, false, 6.0f, true, false);
            }
        }
    }
}