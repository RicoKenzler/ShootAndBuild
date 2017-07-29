using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;

namespace SAB
{
	[System.Serializable]
	public struct ModularTrack
	{
		public AudioClip BaseTrack;
		public AudioClip CalmTrack;
		public AudioClip CombatTrack;

		public AudioClip PositiveBuffLoop;
		public AudioClip NegativeBuffLoop;

		public float BPM;
	}

	//-------------------------------------------------

	public class MusicManager : MonoBehaviour 
	{
		//-------------------------------------------------	
		// Volumes
		//-------------------------------------------------	
		private const float MusicGroupVolumeMuted   = -80.0f;
		public  float		MusicGroupVolumeDefault = 0.0f;

		private const float TrackVolumeMuted		= 0.0f;
		private const float TrackVolumeDefault		= 1.0f;

		//-------------------------------------------------	
		// Mixer & Tracks
		//-------------------------------------------------	
		public AudioMixer		audioMixer;
		public ModularTrack		musicTrack;
		private AudioMixerGroup soundGroup;

		//-------------------------------------------------	
		// Combat Mood Parameter
		//-------------------------------------------------	
		public float keepCombatStateDuration = 1.0f;
		public float combatFadeInDuration    = 2.0f;
		public float combatFadeOutDuration   = 3.0f;

		//-------------------------------------------------	
		// Sources for Music Tracks
		//-------------------------------------------------	
		private AudioSource	baseTrackSource;
		private AudioSource	calmSource;
		private AudioSource	combatSource;

		private AudioSource positiveBuffLoopSource;
		private AudioSource negativeBuffLoopSource;

		//-------------------------------------------------	
		// Combat State
		//-------------------------------------------------	
		private float	lastCombatTime	= 0.0f;
		private bool	isInCombat		= false;
		private float	combatAmount	= 0.001f;

		//-------------------------------------------------	
		// Buff-Music State
		//-------------------------------------------------	
		private int playerPositiveBuffCount = 0;
		private int playerNegativeBuffCount = 0;

		public float TEST_BUFF_LOOP_OFFSET = 0.00f;

		public float LastCombatTime
		{
			get
			{
				return lastCombatTime;
			}
		}
		//-------------------------------------------------

		void Awake()
		{
			instance = this; 
		}

		//-------------------------------------------------

		void Start() 
		{
			const int sourceCount = 5;
			AudioSource[] audioSources = new AudioSource[sourceCount];

			for (int i = 0; i < sourceCount; ++i)
			{
				audioSources[i] = gameObject.AddComponent<AudioSource>();
			}

			soundGroup = audioMixer.FindMatchingGroups("Music")[0];
			
			baseTrackSource			= audioSources[0];
			calmSource				= audioSources[1];
			combatSource			= audioSources[2];
			positiveBuffLoopSource	= audioSources[3];
			negativeBuffLoopSource	= audioSources[4];

			baseTrackSource.outputAudioMixerGroup		= soundGroup;
			calmSource.outputAudioMixerGroup			= soundGroup;
			combatSource.outputAudioMixerGroup			= soundGroup;
			positiveBuffLoopSource.outputAudioMixerGroup = soundGroup;
			negativeBuffLoopSource.outputAudioMixerGroup = soundGroup;
			
			NextTrack();
		}
	
		//-------------------------------------------------

		public void SignalIsInCombat()
		{
			lastCombatTime = Time.time;
			isInCombat = true;
		}

		//-------------------------------------------------

		void Update() 
		{
			if (CheatManager.instance.disableMusic)
			{
				soundGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeMuted);
			}
			else
			{
				soundGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeDefault);
			}

			if (!baseTrackSource.isPlaying)
			{
				NextTrack();
			}

			TickCombatState();
			TickMoodFades();
		}

		//-------------------------------------------------

		public void NextTrack()
		{
			baseTrackSource.Stop();
			calmSource.Stop();
			combatSource.Stop();
			positiveBuffLoopSource.Stop();
			negativeBuffLoopSource.Stop();

			baseTrackSource.clip		= musicTrack.BaseTrack;
			calmSource.clip				= musicTrack.CalmTrack;
			combatSource.clip			= musicTrack.CombatTrack;
			positiveBuffLoopSource.clip	= musicTrack.PositiveBuffLoop;
			negativeBuffLoopSource.clip	= musicTrack.NegativeBuffLoop;

			baseTrackSource.loop		= false;
			calmSource.loop				= false;
			combatSource.loop			= false;
			positiveBuffLoopSource.loop	= true;
			negativeBuffLoopSource.loop	= true;

			if (musicTrack.BaseTrack.length != musicTrack.CalmTrack.length || musicTrack.BaseTrack.length != musicTrack.CombatTrack.length)
			{
				Debug.Log("Music Tracks do not have the same length: " + musicTrack.BaseTrack.length + " " + musicTrack.CalmTrack.length + " " + musicTrack.CombatTrack.length);
			}

			baseTrackSource.Play();
			calmSource.Play();
			combatSource.Play();

			UpdateBuffLoops();
		}

		//-------------------------------------------------

		float LinearToLogarithmic(float t)
		{
			// we want a logarithmic function that maps 0 to 0 and 1 to 1
			double c = 0.07f;

			double nominator	= System.Math.Log(c / (c + (double)t));
			double denominator	= System.Math.Log(c / (c + 1));

			return (float) (nominator / denominator);
		}

		//-------------------------------------------------

		private void TickMoodFades()
		{
			if (isInCombat)
			{
				if (combatAmount == 1.0f)
				{
					return;
				}

				combatAmount += Time.deltaTime * (1.0f / (combatFadeInDuration + float.Epsilon));
			}
			else
			{
				if (combatAmount == 0.0f)
				{
					return;
				}

				combatAmount -= Time.deltaTime * (1.0f / (combatFadeOutDuration + float.Epsilon));
			}

			combatAmount = Mathf.Clamp(combatAmount, 0.0f, 1.0f);

			calmSource.volume	= LinearToLogarithmic(1.0f - combatAmount);
			combatSource.volume = LinearToLogarithmic(combatAmount);
		}

		//-------------------------------------------------

		void StartAdditionalLoop(AudioSource additionalLoop)
		{
			additionalLoop.Stop();
		
			double currentPositionWithinTrack = (double) baseTrackSource.time;
			double beatsPassed = currentPositionWithinTrack * ((double) musicTrack.BPM / 60.0);

			// all 8 beats the loop may start
			double neededOffsetBeats = beatsPassed % 8.0;
			double neededOffsetSeconds = neededOffsetBeats / ((double) musicTrack.BPM / 60.0);

			neededOffsetSeconds += TEST_BUFF_LOOP_OFFSET;

			additionalLoop.time = (float) neededOffsetSeconds;
			additionalLoop.Play();		
		}

		//-------------------------------------------------

		public void OnAddPlayerBuffCount(int delta, bool isPositive)
		{
			if (isPositive)
			{
				playerPositiveBuffCount += delta;
				Debug.Assert(playerPositiveBuffCount >= 0);
			}
			else
			{
				playerNegativeBuffCount += delta;
				Debug.Assert(playerNegativeBuffCount >= 0);
			}
		
			UpdateBuffLoops();
		}

		//-------------------------------------------------

		void UpdateBuffLoops()
		{
			if (playerPositiveBuffCount > 0 && !positiveBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(positiveBuffLoopSource);
			}
			else if (playerPositiveBuffCount == 0)
			{
				positiveBuffLoopSource.Stop();
			}

			if (playerNegativeBuffCount > 0 && !negativeBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(negativeBuffLoopSource);
			}
			else if (playerNegativeBuffCount == 0)
			{
				negativeBuffLoopSource.Stop();
			}
		}

		//-------------------------------------------------

		void TickCombatState()
		{
			if (!isInCombat)
			{
				return;
			}

			if (Time.time > lastCombatTime + keepCombatStateDuration)
			{
				isInCombat = false;
			}
		}

		//-------------------------------------------------

		public static MusicManager instance
		{
			get; private set;
		}
	}

	[CustomEditor(typeof(MusicManager))]
	public class MusicManagerEditor : Editor
	{
		private bool positiveBuffTriggered = false;
		private bool negativeBuffTriggered = false;

		public override void OnInspectorGUI()
		{
			MusicManager musicManager = (MusicManager)target;

			DrawDefaultInspector();

			GUILayout.Label("Trigger", EditorStyles.boldLabel);
			if (GUILayout.Button((positiveBuffTriggered ? "(-)" : "(+)") + " Positive Buff"))
			{
				int delta = positiveBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, true);

				positiveBuffTriggered = !positiveBuffTriggered;
			}

			if (GUILayout.Button((negativeBuffTriggered ? "(-)" : "(+)") + " Negative Buff"))
			{
				int delta = negativeBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, false);

				negativeBuffTriggered = !negativeBuffTriggered;
			}

			if (GUILayout.Button("Next Track"))
			{
				musicManager.NextTrack();
			}

		}
	}
}