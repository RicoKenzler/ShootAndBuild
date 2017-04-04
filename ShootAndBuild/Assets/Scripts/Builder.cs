using UnityEngine;

public class Builder : MonoBehaviour
{
	public Building towerPrefab;
	public float distance = 2;

	public AudioClip[] buildSounds;

	public void TryBuild()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);

		if (!towerPrefab.IsPayable())
		{
			return;
		}

		if (!Grid.instance.IsFree(towerPrefab.gameObject, pos))
		{
			return;
		}

		Build(pos);
	}

	private void Build(Vector3 pos)
	{
		GameObject instance = Instantiate(towerPrefab.gameObject);
		instance.transform.position = pos;

		if (buildSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, buildSounds.Length);
            AudioClip rndSound = buildSounds[rndSoundIndex];
            AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
        }

		towerPrefab.Pay();
	}
}