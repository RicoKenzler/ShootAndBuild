using UnityEngine;
using System.Collections.Generic;

public class Builder : MonoBehaviour
{
	public List<Building> buildingPrefabs;
	public float distance = 2;

	public AudioData buildSound;
	public ParticleSystem buildEffect;

	public AudioData noSpaceSound;

	private PlayerMenu playerMenu;

	void Awake()
	{
		
	}

	private void Start()
	{
		playerMenu = GetComponent<PlayerMenu>();
	}

	public void TryBuild()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);
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
			AudioManager.instance.PlayAudio(noSpaceSound);
			return;
		}

		Build(activeBuilding, pos);
	}

	private void Build(Building buildingPrefab, Vector3 pos)
	{
		GameObject newTower = Instantiate(buildingPrefab.gameObject, BuildingManager.instance.transform);
		newTower.transform.position = pos;

		AudioManager.instance.PlayAudio(buildSound, transform.position);
		
		buildingPrefab.Pay();

		if (buildEffect)
		{
			ParticleManager.instance.SpawnParticle(buildEffect, newTower, newTower.transform.position, Quaternion.identity, false, 6.0f, true, false);
		}
	}
}