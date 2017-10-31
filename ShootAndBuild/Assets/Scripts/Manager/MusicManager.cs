using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;

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

	///////////////////////////////////////////////////////////////////////////

	public class MusicManager : MonoBehaviour 
	{
		///////////////////////////////////////////////////////////////////////////	
		// Volumes
		///////////////////////////////////////////////////////////////////////////	
		private const float MUSIC_GROUP_VOLUME_MUTED	= -80.0f;
		public  float		m_MusicGroupVolumeDefault	= -4.8f;

		private const float TRACK_VOLUME_MUTED			= 0.0f;
		private const float TRACK_VOLUME_DEFAULT		= 1.0f;

		///////////////////////////////////////////////////////////////////////////	
		// Mixer & Tracks
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private AudioMixer			m_AudioMixer;

		[FormerlySerializedAs("MusicTracks")]
		[SerializeField] private ModularTrack[]		m_MusicTracks;
		private					 AudioMixerGroup	m_MusicMixerGroup;

		///////////////////////////////////////////////////////////////////////////	
		// Combat Mood Parameter
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private float m_KeepCombatStateDuration = 1.0f;
		[SerializeField] private float m_CombatFadeInDuration    = 2.0f;
		[SerializeField] private float m_CombatFadeOutDuration   = 4.0f;

		///////////////////////////////////////////////////////////////////////////	
		// Pause Parameter
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private float m_PauseFadeInDuration	 = 3.0f;
		[SerializeField] private float m_PauseFadeOutDuration	 = 1.0f;
		
		///////////////////////////////////////////////////////////////////////////	
		// Danger Parameter
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private float m_KeepDangerDuration		= 1.0f;
		[SerializeField] private float m_DangerFadeInDuration	= 0.3f;
		[SerializeField] private float m_DangerFadeOutDuration	= 3.0f;

		///////////////////////////////////////////////////////////////////////////	
		// Sources for Music Tracks
		///////////////////////////////////////////////////////////////////////////	
		private AudioSource	m_BaseTrackSource;
		private AudioSource	m_CalmSource;
		private AudioSource	m_CombatSource;

		private AudioSource m_PositiveBuffLoopSource;
		private AudioSource m_NegativeBuffLoopSource;
		private AudioSource m_DangerLoopSource;

		///////////////////////////////////////////////////////////////////////////	
		// Combat State
		///////////////////////////////////////////////////////////////////////////	
		private float	m_LastCombatTime	= 0.0f;
		private bool	m_IsInCombat		= false;
		private float	m_CombatAmount		= 0.001f;

		///////////////////////////////////////////////////////////////////////////	
		// Pause State
		///////////////////////////////////////////////////////////////////////////	
		private float m_PauseAmount = 0.0f;

		///////////////////////////////////////////////////////////////////////////	
		// Danger State
		///////////////////////////////////////////////////////////////////////////	
		private float m_LastDangerTime	= 0.0f;
		private bool  m_IsInDanger		= false;
		private float m_DangerAmount		= 0.001f;

		///////////////////////////////////////////////////////////////////////////	
		// Buff-Music State
		///////////////////////////////////////////////////////////////////////////	
		private int m_PlayerPositiveBuffCount = 0;
		private int m_PlayerNegativeBuffCount = 0;

		///////////////////////////////////////////////////////////////////////////	
		// Playlist
		///////////////////////////////////////////////////////////////////////////	
		int			m_CurrentTrackIndex = -1;
		List<int>	m_CurrentPlaylistIndices = new List<int>();

		///////////////////////////////////////////////////////////////////////////

		public float LastCombatTime			{ get { return m_LastCombatTime; } }
		public static MusicManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;  
		}

		///////////////////////////////////////////////////////////////////////////

		void Start() 
		{
			const int sourceCount = 6;
			AudioSource[] audioSources = new AudioSource[sourceCount];

			for (int i = 0; i < sourceCount; ++i)
			{
				audioSources[i] = gameObject.AddComponent<AudioSource>();
			}

			m_MusicMixerGroup = m_AudioMixer.FindMatchingGroups("Music")[0];
			
			m_BaseTrackSource			= audioSources[0];
			m_CalmSource				= audioSources[1];
			m_CombatSource			= audioSources[2];
			m_PositiveBuffLoopSource	= audioSources[3];
			m_NegativeBuffLoopSource	= audioSources[4];
			m_DangerLoopSource		= audioSources[5];

			m_BaseTrackSource.outputAudioMixerGroup			= m_MusicMixerGroup;
			m_CalmSource.outputAudioMixerGroup				= m_MusicMixerGroup;
			m_CombatSource.outputAudioMixerGroup				= m_MusicMixerGroup;
			m_PositiveBuffLoopSource.outputAudioMixerGroup	= m_MusicMixerGroup;
			m_NegativeBuffLoopSource.outputAudioMixerGroup	= m_MusicMixerGroup;
			m_DangerLoopSource.outputAudioMixerGroup			= m_MusicMixerGroup;
			
			m_CurrentPlaylistIndices = Enumerable.Range(0,m_MusicTracks.Length).ToList();

			NextTrack();
		}
	
		///////////////////////////////////////////////////////////////////////////

		public void SignalIsInCombat()
		{
			m_LastCombatTime = Time.time;
			m_IsInCombat = true;
		}

		///////////////////////////////////////////////////////////////////////////

		public void SignalIsDanger()
		{
			m_LastDangerTime = Time.time;
			m_IsInDanger = true;
		}

		///////////////////////////////////////////////////////////////////////////

		void Update() 
		{
			if (CheatManager.instance.disableMusic)
			{
				m_MusicMixerGroup.audioMixer.SetFloat("MusicVolume", MUSIC_GROUP_VOLUME_MUTED);
			}
			else
			{
				m_MusicMixerGroup.audioMixer.SetFloat("MusicVolume", m_MusicGroupVolumeDefault);
			}

			if (!m_BaseTrackSource.isPlaying)
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

		///////////////////////////////////////////////////////////////////////////

		private void TickPauseState()
		{
			if (GameManager.instance.Status == GameStatus.Running)
			{
				m_PauseAmount -= Time.unscaledDeltaTime / (m_PauseFadeOutDuration + 0.001f);
				m_PauseAmount = Mathf.Max(m_PauseAmount, 0.0f);
			}
			else
			{
				m_PauseAmount += Time.unscaledDeltaTime / (m_PauseFadeInDuration + 0.001f);
				m_PauseAmount = Mathf.Min(m_PauseAmount, 1.0f);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		ModularTrack GetCurrentTrack()
		{
			int realIndex = m_CurrentPlaylistIndices[m_CurrentTrackIndex];
			return m_MusicTracks[realIndex];
		}

		///////////////////////////////////////////////////////////////////////////

		public void NextTrack()
		{
			m_BaseTrackSource.Stop();
			m_CalmSource.Stop();
			m_CombatSource.Stop();
			m_PositiveBuffLoopSource.Stop();
			m_NegativeBuffLoopSource.Stop();
			m_DangerLoopSource.Stop();

			int oldTrackIndex = m_CurrentTrackIndex;
			m_CurrentTrackIndex = (m_CurrentTrackIndex + 1) % m_MusicTracks.Length;

			if (m_CurrentTrackIndex == 0)
			{
				// shuffle
				m_CurrentPlaylistIndices = m_CurrentPlaylistIndices.OrderBy(a => Random.Range(0, 10000)).ToList();

				// make sure we do not have the same song twice in a row
				if (m_CurrentPlaylistIndices.Count >= 2 && m_CurrentPlaylistIndices[0] == oldTrackIndex)
				{
					int swapWithIndex = m_CurrentPlaylistIndices.Count - 1;
					m_CurrentPlaylistIndices[0] = m_CurrentPlaylistIndices[swapWithIndex];
					m_CurrentPlaylistIndices[swapWithIndex] = oldTrackIndex;
				}
			}

			ModularTrack currentTrack = GetCurrentTrack();

			m_BaseTrackSource.clip		= currentTrack.BaseTrack;
			m_CalmSource.clip				= currentTrack.CalmTrack;
			m_CombatSource.clip			= currentTrack.CombatTrack;
			m_PositiveBuffLoopSource.clip	= currentTrack.PositiveBuffLoop;
			m_NegativeBuffLoopSource.clip	= currentTrack.NegativeBuffLoop;
			m_DangerLoopSource.clip		= currentTrack.DangerLoop;

			m_BaseTrackSource.loop		= false;
			m_CalmSource.loop				= false;
			m_CombatSource.loop			= false;
			m_PositiveBuffLoopSource.loop	= true;
			m_NegativeBuffLoopSource.loop	= true;

			if (currentTrack.BaseTrack.length != currentTrack.CalmTrack.length 
			 || currentTrack.BaseTrack.length != currentTrack.CombatTrack.length)
			{
				Debug.Log("Music Tracks do not have the same length: " + currentTrack.BaseTrack.length + " " + currentTrack.CalmTrack.length + " " + currentTrack.CombatTrack.length);
			}

			m_BaseTrackSource.Play();
			m_CalmSource.Play();
			m_CombatSource.Play();
		}

		///////////////////////////////////////////////////////////////////////////

		float LinearToLogarithmic(float t)
		{
			// we want a logarithmic function that maps 0 to 0 and 1 to 1
			double c = 0.07f;

			double nominator	= System.Math.Log(c / (c + (double)t));
			double denominator	= System.Math.Log(c / (c + 1));

			return (float) (nominator / denominator);
		}

		///////////////////////////////////////////////////////////////////////////

		private void TickMoodFades()
		{
			if (m_IsInCombat)
			{
				m_CombatAmount += Time.unscaledDeltaTime * (1.0f / (m_CombatFadeInDuration + float.Epsilon));
			}
			else
			{
				m_CombatAmount -= Time.unscaledDeltaTime * (1.0f / (m_CombatFadeOutDuration + float.Epsilon));
			}

			m_CombatAmount = Mathf.Clamp(m_CombatAmount, 0.0f, 1.0f);

			m_CalmSource.volume	= LinearToLogarithmic(1.0f - m_CombatAmount);
			m_CombatSource.volume = LinearToLogarithmic(m_CombatAmount);

			// pause will let calm/combat track fade out
			m_CalmSource.volume	*= (1.0f - m_PauseAmount);
			m_CombatSource.volume *= (1.0f - m_PauseAmount);
		}

		///////////////////////////////////////////////////////////////////////////

		private void TickDangerVolume()
		{
			if (m_IsInDanger)
			{
				if (m_DangerAmount == 1.0f)
				{
					return;
				}

				m_DangerAmount += Time.unscaledDeltaTime * (1.0f / (m_DangerFadeInDuration + float.Epsilon));
			}
			else
			{
				if (m_DangerAmount == 0.0f)
				{
					return;
				}

				m_DangerAmount -= Time.unscaledDeltaTime * (1.0f / (m_DangerFadeOutDuration + float.Epsilon));
			}

			m_DangerAmount = Mathf.Clamp(m_DangerAmount, 0.0f, 1.0f);

			m_DangerLoopSource.volume	= m_DangerAmount;
		}

		///////////////////////////////////////////////////////////////////////////

		void StartAdditionalLoop(AudioSource additionalLoop)
		{
			additionalLoop.Stop();
		
			double currentPositionWithinTrack = (double) m_BaseTrackSource.time;
			double beatsPassed = currentPositionWithinTrack * ((double) GetCurrentTrack().BPM / 60.0);

			// all 8 beats the loop may start
			double neededOffsetBeats = beatsPassed % 8.0;
			double neededOffsetSeconds = neededOffsetBeats / ((double) GetCurrentTrack().BPM / 60.0);

			additionalLoop.time = (float) neededOffsetSeconds;
			additionalLoop.Play();		
		}

		///////////////////////////////////////////////////////////////////////////

		public void OnAddPlayerBuffCount(int delta, bool isPositive)
		{
			if (isPositive)
			{
				m_PlayerPositiveBuffCount += delta;
				Debug.Assert(m_PlayerPositiveBuffCount >= 0);
			}
			else
			{
				m_PlayerNegativeBuffCount += delta;
				Debug.Assert(m_PlayerNegativeBuffCount >= 0);
			}
		
			UpdateAllLoops();
		}

		///////////////////////////////////////////////////////////////////////////

		void UpdateAllLoops()
		{
			// Positive Buffs
			if (m_PlayerPositiveBuffCount > 0 && !m_PositiveBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(m_PositiveBuffLoopSource);
			}
			else if (m_PlayerPositiveBuffCount == 0)
			{
				m_PositiveBuffLoopSource.Stop();
			}

			// Negative Buffs
			if (m_PlayerNegativeBuffCount > 0 && !m_NegativeBuffLoopSource.isPlaying)
			{
				StartAdditionalLoop(m_NegativeBuffLoopSource);
			}
			else if (m_PlayerNegativeBuffCount == 0)
			{
				m_NegativeBuffLoopSource.Stop();
			}

			// Danger
			if (m_DangerAmount > 0.0f && !m_DangerLoopSource.isPlaying)
            {
				StartAdditionalLoop(m_DangerLoopSource);
			}
			else if (m_DangerAmount == 0.0f)
			{
				m_DangerLoopSource.Stop();
			}

			m_PositiveBuffLoopSource.volume	= 1.0f - m_PauseAmount;
			m_NegativeBuffLoopSource.volume	= 1.0f - m_PauseAmount;
			m_DangerLoopSource.volume			= 1.0f - m_PauseAmount;
		}

		///////////////////////////////////////////////////////////////////////////

		void TickCombatState()
		{
			if (!m_IsInCombat)
			{
				return;
			}

			if (Time.time > m_LastCombatTime + m_KeepCombatStateDuration)
			{
				m_IsInCombat = false;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void TickDangerState()
		{
			if (!m_IsInDanger)
			{
				return;
			} 

			if (Time.time > m_LastDangerTime + m_KeepDangerDuration)
			{
				m_IsInDanger = false;
				UpdateAllLoops();
			}
		}
	}
}