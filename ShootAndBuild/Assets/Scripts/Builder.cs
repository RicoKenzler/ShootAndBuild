using UnityEngine;

public class Builder : MonoBehaviour
{
	public Building towerPrefab;
	public float distance = 2;

	public AudioClip[] buildSounds;
	public AudioClip[] noMoneySounds;
	public AudioClip[] noSpaceSounds;

	public void TryBuild()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);

		if (!towerPrefab.IsPayable())
		{
			AudioManager.instance.PlayRandomOneShot(noMoneySounds, new OneShotParams(transform.position, 0.5f));
			return;
		}

		if (!Grid.instance.IsFree(towerPrefab.gameObject, pos))
		{
			AudioManager.instance.PlayRandomOneShot(noSpaceSounds, new OneShotParams(transform.position, 0.5f));
			return;
		}

		Build(pos);
	}

	private void Build(Vector3 pos)
	{
		GameObject instance = Instantiate(towerPrefab.gameObject);
		instance.transform.position = pos;

		AudioManager.instance.PlayRandomOneShot(buildSounds, new OneShotParams(transform.position, 0.5f));
		
		towerPrefab.Pay();
	}
}