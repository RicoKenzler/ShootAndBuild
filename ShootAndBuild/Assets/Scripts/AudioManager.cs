using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public AudioSource PlayOneShot(AudioClip clip, Vector3 pos, float volume = 1.0f, bool suppressDoppler = false, float amount3D = 1.0f, float pitch = 1.0f)
	{
		GameObject audioObject = Instantiate(oneShotPrefab, gameObject.transform);
		audioObject.name = "OneShot " + clip.name;

		audioObject.transform.position = pos;
		AudioSource audioSource = audioObject.GetComponent<AudioSource>();
		audioSource.clip			= clip;
		audioSource.volume			= volume;
		audioSource.dopplerLevel	= suppressDoppler ? 0.0f : 1.0f;
		audioSource.spatialBlend	= amount3D;
		audioSource.pitch			= pitch;
		audioSource.Play();

		Destroy(audioObject, clip.length);

		return audioSource;
	}
   
	public static AudioManager instance
    {
        get; private set;
    }
}
