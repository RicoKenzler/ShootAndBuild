using UnityEngine;

public class Builder : MonoBehaviour
{
	public Building towerPrefab;
	public float distance = 2;

	public AudioData buildSound;
	public ParticleSystem buildEffect;

	public AudioData noSpaceSound;

	public void TryBuild()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);
		pos = Grid.instance.ToTileCenter(pos);

		if (!towerPrefab.IsPayable())
		{
			Inventory.sharedInventoryInstance.TriggerNotEnoughItemsSound();

			GlobalPanel.instance.HighlightMoney();
			return;
		}

		if (!Grid.instance.IsFree(towerPrefab.gameObject, pos))
		{
			AudioManager.instance.PlayAudio(noSpaceSound);
			return;
		}

		Build(pos);
	}

	private void Build(Vector3 pos)
	{
		GameObject newTower = Instantiate(towerPrefab.gameObject);
		newTower.transform.position = pos;

		AudioManager.instance.PlayAudio(buildSound, transform.position);
		
		towerPrefab.Pay();

		if (buildEffect)
		{
			ParticleManager.instance.SpawnParticle(buildEffect, newTower, newTower.transform.position, Quaternion.identity, false, 6.0f, true, false);
		}
	}
}