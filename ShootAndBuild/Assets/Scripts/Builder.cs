using UnityEngine;

public class Builder : MonoBehaviour
{
	public GameObject towerPrefab;
	public float distance = 2;

	public AudioClip[] buildSounds;

	public void Build()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);

		if (Grid.instance.IsFree(towerPrefab, pos))
		{
			GameObject instance = Instantiate(towerPrefab);
			instance.transform.position = pos;

			if (buildSounds.Length > 0)
            {
                int rndSoundIndex = Random.Range(0, buildSounds.Length);
                AudioClip rndSound = buildSounds[rndSoundIndex];
                AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
            }
		}
	}
}