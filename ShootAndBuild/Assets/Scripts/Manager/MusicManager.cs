using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
		public AudioClip DangerLoop;

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
		public AudioMixer		AudioMixer;
		public ModularTrack[]	MusicTracks;
		private AudioMixerGroup MusicMixerGroup;

		//-------------------------------------------------	
		// Combat Mood Parameter
		//-------------------------------------------------	
		public float keepCombatStateDuration = 1.0f;
		public float combatFadeInDuration    = 2.0f;
		public float combatFadeOutDuration   = 3.0f;

		//-------------------------------------------------	
		// Pause Parameter
		//-------------------------------------------------	
		public float pauseFadeInDuration	 = 1.0f;
		public float pauseFadeOutDuration	 = 0.5f;
		
		//-------------------------------------------------	
		// Danger Parameter
		//-------------------------------------------------	
		public float keepDangerDuration		 = 3.0f;
		public float dangerFadeInDuration    = 0.3f;
		public float dangerFadeOutDuration   = 3.0f;

		//-------------------------------------------------	
		// Sources for Music Tracks
		//-------------------------------------------------	
		private AudioSource	baseTrackSource;
		private AudioSource	calmSource;
		private AudioSource	combatSource;

		private AudioSource positiveBuffLoopSource;
		private AudioSource negativeBuffLoopSource;
		private AudioSource dangerLoopSource;

		//-------------------------------------------------	
		// Combat State
		//-------------------------------------------------	
		private float	lastCombatTime	= 0.0f;
		private bool	isInCombat		= false;
		private float	combatAmount	= 0.001f;

		//-------------------------------------------------	
		// Pause State
		//-------------------------------------------------	
		private float pauseAmount = 0.0f;

		//-------------------------------------------------	
		// Danger State
		//-------------------------------------------------	
		private float lastDangerTime	= 0.0f;
		private bool  isInDanger		= false;
		private float dangerAmount		= 0.001f;

		//-------------------------------------------------	
		// Buff-Music State
		//-------------------------------------------------	
		private int playerPositiveBuffCount = 0;
		private int playerNegativeBuffCount = 0;

		//-------------------------------------------------	
		// Playlist
		//-------------------------------------------------	
		int			currentTrackIndex = -1;
		List<int>	currentPlaylistIndices = new List<int>();

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
			const int sourceCount = 6;
			AudioSource[] audioSources = new AudioSource[sourceCount];

			for (int i = 0; i < sourceCount; ++i)
			{
				audioSources[i] = gameObject.AddComponent<AudioSource>();
			}

			MusicMixerGroup = AudioMixer.FindMatchingGroups("Music")[0];
			
			baseTrackSource			= audioSources[0];
			calmSource				= audioSources[1];
			combatSource			= audioSources[2];
			positiveBuffLoopSource	= audioSources[3];
			negativeBuffLoopSource	= audioSources[4];
			dangerLoopSource		= audioSources[5];

			baseTrackSource.outputAudioMixerGroup			= MusicMixerGroup;
			calmSource.outputAudioMixerGroup				= MusicMixerGroup;
			combatSource.outputAudioMixerGroup				= MusicMixerGroup;
			positiveBuffLoopSource.outputAudioMixerGroup	= MusicMixerGroup;
			negativeBuffLoopSource.outputAudioMixerGroup	= MusicMixerGroup;
			dangerLoopSource.outputAudioMixerGroup			= MusicMixerGroup;
			
			currentPlaylistIndices = Enumerable.Range(0,MusicTracks.Length).ToList();

			NextTrack();
		}
	
		//-------------------------------------------------

		public void SignalIsInCombat()
		{
			lastCombatTime = Time.time;
			isInCombat = true;
		}

		//-------------------------------------------------

		public void SignalIsDanger()
		{
			lastDangerTime = Time.time;
			isInDanger = true;
		}

		//-------------------------------------------------

		void Update() 
		{
			if (CheatManager.instance.disableMusic)
			{
				MusicMixerGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeMuted);
			}
			else
			{
				MusicMixerGroup.audioMixer.SetFloat("MusicVolume", MusicGroupVolumeDefault);
			}

			if (!baseTrackSource.isPlaying)
			{
				NextTrack();
			}

			TickCombatState();
			TickPauseState();
			TickMoodFades();

			TickDangerState();
			TickDangerVolume();

			UpdateAllLoops();
		}

		//-------------------------------------------------

		private void TickPauseState()
		{
			if (GameManager.Instance.Status == GameStatus.Running)
			{
				pauseAmount -= Time.unscaledDeltaTime / (pauseFadeOutDuration + 0.001f);
				pauseAmount = Mathf.Max(pauseAmount, 0.0f);
			}
			else
			{
				pauseAmount += Time.unscaledDeltaTime / (pauseFadeInDuration + 0.001f);
				pauseAmount = Mathf.Min(pauseAmount, 1.0f);
			}
		}

		//-------------------------------------------------

		ModularTrack GetCurrentTrack()
		{
			int realIndex = currentPlaylistIndices[currentTrackIndex];
			return MusicTracks[realIndex];
		}

		//-------------------------------------------------

		public void NextTrack()
		{
			baseTrackSource.Stop();
			calmSource.Stop();
			combatSource.Stop();
			positiveBuffLoopSource.Stop();
			negativeBuffLoopSource.Stop();
			dangerLoopSource.Stop();

			int oldTrackIndex = currentTrackIndex;
			currentTrackIndex = (currentTrackIndex + 1) % MusicTracks.Length;

			if (currentTrackIndex == 0)
			{
				// shuffle
				currentPlaylistIndices = currentPlaylistIndices.OrderBy(a => Random.Range(0, 10000)).ToList();

				// make sure we do not have the same song twice in a row
				if (currentPlaylistIndices.Count >= 2 && currentPlaylistIndices[0] == oldTrackIndex)
				{
					int swapWithIndex = currentPlaylistIndices.Count - 1;
					currentPlaylistIndices[0] = currentPlaylistIndices[swapWithIndex];
					currentPlaylistIndices[swapWithIndex] = oldTrackIndex;
				}
			}

			ModularTrack currentTrack = GetCurrentTrack();

			baseTrackSource.clip		= currentTrack.BaseTrack;
			calmSource.clip				= currentTrack.CalmTrack;
			combatSource.clip			= currentTrack.CombatTrack;
			positiveBuffLoopSource.clip	= currentTrack.PositiveBuffLoop;
			negativeBuffLoopSource.clip	= currentTrack.NegativeBuffLoop;
			dangerLoopSource.clip		= currentTrack.DangerLoop;

			baseTrackSource.loop		= false;
			calmSource.loop				= false;
			combatSource.loop			= false;
			positiveBuffLoopSource.loop	= true;
			negativeBuffLoopSource.loop	= true;

			if (currentTrack.BaseTrack.length != currentTrack.CalmTrack.length 
			 || currentTrack.BaseTrack.length != currentTrack.CombatTrack.length)
			{
				Debug.Log("Music Tracks do not have the same length: " + currentTrack.BaseTrack.length + " " + currentTrack.CalmTrack.length + " " + currentTrack.CombatTrack.length);
			}

			baseTrackSource.Play();
			calmSource.Play();
			combatSource.Play();
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
				combatAmount += Time.unscaledDeltaTime * (1.0f / (combatFadeInDuration + float.Epsilon));
			}
			else
			{
				combatAmount -= Time.unscaledDeltaTime * (1.0f / (combatFadeOutDuration + float.Epsilon));
			}

			combatAmount = Mathf.Clamp(combatAmount, 0.0f, 1.0f);

			calmSource.volume	= LinearToLogarithmic(1.0f - combatAmount);
			combatSource.volume = LinearToLogarithmic(combatAmount);

			// pause will let calm/combat track fade out
			calmSource.volume	*= (1.0f - pauseAmount);
			combatSource.volume *= (1.0f - pauseAmount);
		}

		//-------------------------------------------------

		private void TickDangerVolume()
		{
			if (isInDanger)
			{
				if (dangerAmount == 1.0f)
				{
					return;
				}

				dangerAmount += Time.unscaledDeltaTime * (1.0f / (dangerFadeInDuration + float.Epsilon));
			}
			else
			{
				if (dangerAmount == 0.0f)
				{
					return;
				}

				dangerAmount -= Time.unscaledDeltaTime * (1.0f / (dangerFadeOutDuration + float.Epsilon));
			}

			dangerAmount = Mathf.Clamp(dangerAmount, 0.0f, 1.0f);

			dangerLoopSource.volume	= dangerAmount;
		}

		//-------------------------------------------------

		void StartAdditionalLoop(AudioSource additionalLoop)
		{
			additionalLoop.Stop();
		
			double currentPositionWithinTrack = (double) baseTrackSource.time;
			double beatsPassed = currentPositionWithinTrack * ((double) GetCurrentTrack().BPM / 60.0);

			// all 8 beats the loop may start
			double neededOffsetBeats = beatsPassed % 8.0;
			double neededOffsetSeconds = neededOffsetBeats / ((double) GetCurrentTrack().BPM / 60.0);

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
		
			UpdateAllLoops();
		}

		//-------------------------------------------------

		void UpdateAllLoops()
		{
			// Positive Buffs
			if (playerPositiveBuffCount > 0 && !positiveBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(positiveBuffLoopSource);
			}
			else if (playerPositiveBuffCount == 0)
			{
				positiveBuffLoopSource.Stop();
			}

			// Negative Buffs
			if (playerNegativeBuffCount > 0 && !negativeBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(negativeBuffLoopSource);
			}
			else if (playerNegativeBuffCount == 0)
			{
				negativeBuffLoopSource.Stop();
			}

			// Danger
			if (dangerAmount > 0.0f && !dangerLoopSource.isPlaying)
            {
				StartAdditionalLoop(dangerLoopSource);
			}
			else if (dangerAmount == 0.0f)
			{
				dangerLoopSource.Stop();
			}

			positiveBuffLoopSource.volume	= 1.0f - pauseAmount;
			negativeBuffLoopSource.volume	= 1.0f - pauseAmount;
			dangerLoopSource.volume			= 1.0f - pauseAmount;
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

		void TickDangerState()
		{
			if (!isInDanger)
			{
				return;
			} 

			if (Time.time > lastDangerTime + keepDangerDuration)
			{
				isInDanger = false;
				UpdateAllLoops();
			}
		}

		//-------------------------------------------------

		public static MusicManager instance
		{
			get; private set;
		}
	}
}