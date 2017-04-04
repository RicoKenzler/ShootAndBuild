using UnityEngine;

public class Builder : MonoBehaviour
{
	public Building towerPrefab;
	public float distance = 2;

	public AudioClip[] buildSounds;
	public AudioClip[] noMoneySound;
	public AudioClip[] noSpaceSound;

	public void TryBuild()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);

		if (!towerPrefab.IsPayable())
		{
			if (noMoneySound.Length > 0)
			{
				int rndSoundIndex = Random.Range(0, noMoneySound.Length);
				AudioClip rndSound = noMoneySound[rndSoundIndex];
				AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
			}

			return;
		}

		if (!Grid.instance.IsFree(towerPrefab.gameObject, pos))
		{
			if (noSpaceSound.Length > 0)
			{
				int rndSoundIndex = Random.Range(0, noSpaceSound.Length);
				AudioClip rndSound = noSpaceSound[rndSoundIndex];
				AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f);
			}

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