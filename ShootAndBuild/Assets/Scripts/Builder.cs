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
			float pitch = AudioManager.instance.GetRandomMusicalPitch();
			AudioManager.instance.PlayRandomOneShot(noMoneySounds, new OneShotParams(transform.position, 0.5f, false, 1.0f, pitch));
			return;
		}

		if (!Grid.instance.IsFree(towerPrefab.gameObject, pos))
		{
			float pitch = AudioManager.instance.GetRandomMusicalPitch();
			AudioManager.instance.PlayRandomOneShot(noSpaceSounds, new OneShotParams(transform.position, 0.5f, false, 1.0f, pitch));
			return;
		}

		Build(pos);
	}

	private void Build(Vector3 pos)
	{
		GameObject instance = Instantiate(towerPrefab.gameObject);
		instance.transform.position = pos;

		AudioManager.instance.PlayRandomOneShot(buildSounds, new OneShotParams(transform.position));
		
		towerPrefab.Pay();
	}
}