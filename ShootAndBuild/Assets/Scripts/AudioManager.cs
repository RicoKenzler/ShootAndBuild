using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct OneShotParams
{
	public Vector3	position;
	public float	volume;	
	public bool		suppressDoppler;
	public float	amount3D;
	public float	pitch;

	public OneShotParams(Vector3 pos, float volume = 1.0f, bool suppressDoppler = false, float amount3D = 1.0f, float pitch = 1.0f)
	{
		this.position = pos;
		this.volume = volume;
		this.suppressDoppler = suppressDoppler;
		this.amount3D = amount3D;
		this.pitch = pitch;
	}
}

public class AudioManager : MonoBehaviour
{
	public GameObject oneShotPrefab;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		
	}

	void Awake()
    {
        instance = this;
    }

	public float HalftoneToPitch(int halfTone)
	{
		float pitch = Mathf.Pow(2.0f, ((float) halfTone / (float) 12.0f));
		return pitch;
	}

	public float RandomPitchFromHalftones(int[] halftones)
	{
		if (halftones.Length == 0)
		{
			return 1.0f;
		}

		int rndSoundIndex = Random.Range(0, halftones.Length);
		int halftone = halftones[rndSoundIndex];

		return HalftoneToPitch(halftone);
	}

	public float GetRandomMusicalPitch()
	{
		int[] niceHalftones = { 0, 2, 4, 5, 7, 9, 11, 12,  0, 4, 7, 12, 0, 7,   5};
		return RandomPitchFromHalftones(niceHalftones);
	}

	public AudioSource PlayRandomOneShot(AudioClip[] audioClips, OneShotParams oneShotParams)
	{
		if (audioClips.Length <= 0)
		{
			return null;
		}
		
		int rndSoundIndex = Random.Range(0, audioClips.Length);
		AudioClip rndSound = audioClips[rndSoundIndex];
		return PlayOneShot(rndSound, oneShotParams);
	}

	public AudioSource PlayOneShot(AudioClip clip, OneShotParams oneShotParams)
	{
		GameObject audioObject = Instantiate(oneShotPrefab, gameObject.transform);
		audioObject.name = "OneShot " + clip.name;

		audioObject.transform.position = oneShotParams.position;
		AudioSource audioSource = audioObject.GetComponent<AudioSource>();
		audioSource.clip			= clip;
		audioSource.volume			= oneShotParams.volume;
		audioSource.dopplerLevel	= oneShotParams.suppressDoppler ? 0.0f : 1.0f;
		audioSource.spatialBlend	= oneShotParams.amount3D;
		audioSource.pitch			= oneShotParams.pitch;
		audioSource.Play();

		Destroy(audioObject, clip.length);

		return audioSource;
	}
   
	public static AudioManager instance
    {
        get; private set;
    }
}
