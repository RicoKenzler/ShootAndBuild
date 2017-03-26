using UnityEngine;

public class Collectable : MonoBehaviour
{
	public float collectRadius = 0.5f;
	public AudioClip[] collectSounds;

	void Update()
	{
		InputController[] players = FindObjectsOfType<InputController>();

		Vector3 selfPosition = transform.position;

		for (int i = 0; i < players.Length; ++i)
		{
			InputController player = players[i];

			Vector3 differenceVector = (player.transform.position - selfPosition);
			differenceVector.y = 0.0f;

			if (differenceVector.sqrMagnitude <= (collectRadius * collectRadius))
			{
				OnCollect(player);
				return;
			}
		}

		if (selfPosition.y > targetHeight)
		{
			selfPosition.y -= 0.1f;
			selfPosition.y = Mathf.Max(selfPosition.y, targetHeight);
			transform.position = selfPosition;
		}
	}

    public float targetHeight
    {
        get; set;
    }

    private void OnCollect(InputController player)
    {
        if (collectSounds.Length > 0)
        {
            int rndSoundIndex = Random.Range(0, collectSounds.Length);
            AudioClip rndSound = collectSounds[rndSoundIndex];
            AudioSource.PlayClipAtPoint(rndSound, transform.position, 0.5f);
        }

        Destroy(gameObject);
    }
}
