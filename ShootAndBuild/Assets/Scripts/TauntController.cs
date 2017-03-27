using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TauntController : MonoBehaviour
{

	public AudioClip[]	tauntSounds;
	public AudioClip    singSound;
	
	private InputController	inputController;

	private int tauntStep = 0;

	// Use this for initialization
	void Start ()
	{
		inputController = GetComponent<InputController>();
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	public void PlayTaunt()
	{
		PlayerID playerID = inputController.playerID;

		if (playerID == PlayerID.Player1)
		{
			// Player1: Fart
			int rndSoundIndex = Random.Range(0, tauntSounds.Length);
			AudioClip rndSound = tauntSounds[rndSoundIndex];

			float rndPitch = Random.Range(0.5f, 2.0f);

			AudioManager.instance.PlayOneShot(rndSound, transform.position, 0.5f, true, 0.5f, rndPitch);
			return;
		}

		// Player2: Mozart
		int[] mozartSteps = {0, -5, 0, -5, 0, -5, 0, 4, 7,	5, 2, 5, 2, 5, 2, -1, 2, -5,
								0, 0, 4, 2, 0, 0, -1, -1,		2, 5, -1, 2, 0, 0,
									4, 2, 0, 0, -1, -1,      2, 5, -1, 0,	-24};

		// Player 3+: Mario
		int[] marioSteps = {16, 16, 16, 12, 16, 19, 7,
							0, -5, -8, -4, -1, -2, -3,
							-5, 4, 7, 9, 5, 7, 4, 0, 2, -1,
							0, -5, -8, -4, -1, -2, -3,
							-5, 4, 7, 9, 5, 7, 4, 0, 2, -1,
							7,6,5,3,4,  -4, -3, 0, -3, 0, 2,
							7,6,5,3,4,  12, 12,12,
							7,6,5,3,4,  -4, -3, 0, 2,    3, 2, 0,		-24};

		int[] steps = playerID == PlayerID.Player2 ? mozartSteps : marioSteps;

		// map halftone steps to pitch
		int index = tauntStep % steps.Length;
		int pitchHalftoneDelta = steps[index];

		float pitch = Mathf.Pow(2.0f, ((float) pitchHalftoneDelta / (float) 12.0f));

		AudioManager.instance.PlayOneShot(singSound, transform.position, 0.5f, true, 0.5f, pitch);

		tauntStep++;
	}
	
}
