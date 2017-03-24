using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectable : MonoBehaviour
{
	public float collectRadius = 0.5f;

	public AudioClip[] CollectSounds;

	private float targetHeight = 0.0f;

	public void SetTargetHeight(float newTargetHeight)
	{
		targetHeight = newTargetHeight;
	}

	// Use this for initialization
	void Start ()
	{
		
	}
	
	void OnCollect(InputController player)
	{
		if (CollectSounds.Length > 0)
		{
			int rndSoundIndex = Random.Range(0, CollectSounds.Length -1);
			AudioClip rndSound = CollectSounds[rndSoundIndex];
			AudioSource.PlayClipAtPoint(rndSound, transform.position, 0.5f);
		}

		Destroy(gameObject);
	}

	// Update is called once per frame
	void Update ()
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
}
