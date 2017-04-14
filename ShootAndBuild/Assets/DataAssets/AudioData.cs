using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CreateAssetMenu(menuName = "AudioData", fileName = "AudioData")]
public class AudioData : ScriptableObject
{
	public AudioClip[]	audioClips;

	[Range(0.0f, 10.0f)]
	public float		volume					= 1.0f;	

	public enum PitchMode
	{
		Default,
		MusicalRandom,
		Mario,
		Mozart
	}

	public bool			rndMusicalPitch			= false;

	[Range(-24.0f, 24.0f)]
	public float		pitchOffsetSemitones	= 0.0f;

	[Range(-24.0f, 24.0f)]
	public float		pitchRangeSemitones		= 2.0f;

	public bool			isUISound				= false;
	public bool			suppressDoppler			= false;

	[Range(0.0f, 1.0f)]
	public float		amount3D				= 1.0f;

	// Use this for initialization
	void Start()
	{

	}

	// Update is called once per frame
	void Update()
	{

	}
}

[CustomEditor(typeof(AudioData))]
public class AudioDataEditor : Editor
{
	public override void OnInspectorGUI()
    {
		AudioData audioData = (AudioData)target;

		List<string> hideFields = new List<string>();
		
		if (audioData.isUISound)
		{
			hideFields.Add("suppressDoppler");
			hideFields.Add("amount3D");
		}	
     
		if (audioData.rndMusicalPitch)
		{
			hideFields.Add("pitchRangeSemitones");
			hideFields.Add("pitchOffsetSemitones");
		}

		DrawPropertiesExcluding(serializedObject, hideFields.ToArray());
		serializedObject.ApplyModifiedProperties();

		// Audio Manager only exist (and works) during play
		GUI.enabled = Application.isPlaying;

		if (GUILayout.Button("Spawn Sound at (2,0,0)"))
		{
			Vector3 pos = new Vector3(2.0f, 0.0f, 0.0f);
			AudioManager.instance.PlayAudio(audioData, pos);
		}

		GUI.enabled = true;		
    }
}
