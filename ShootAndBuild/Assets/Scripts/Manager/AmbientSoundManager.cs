using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

///////////////////////////////////////////////////////////////////////////

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

	enum AmbientSoundType
	{
		Grass,
		Water,
		Ruins,

		Invalid
	}

	///////////////////////////////////////////////////////////////////////////
	// Helper struct: Cell within the sound grid
	///////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	struct AmbientSoundCell
	{
		public AmbientSoundType	SoundType;

		///////////////////////////////////////////////////////////////////////////

		public AmbientSoundCell(AmbientSoundType soundType)
		{
			SoundType = soundType;
		}

		///////////////////////////////////////////////////////////////////////////

		public static Color GetDebugColor(AmbientSoundType soundType)
		{
			switch (soundType)
			{
				case AmbientSoundType.Water:
					return Color.blue;
				case AmbientSoundType.Grass:
					return Color.green;
				case AmbientSoundType.Ruins:
					return Color.gray;
			}

			return Color.white;
		}

		///////////////////////////////////////////////////////////////////////////

		public Color GetDebugColor()
		{
			return GetDebugColor(SoundType);
		}
	}

	///////////////////////////////////////////////////////////////////////////
	// Ambient Sound Manager
	///////////////////////////////////////////////////////////////////////////
	public class AmbientSoundManager : MonoBehaviour 
	{
		///////////////////////////////////////////////////////////////////////////	
		// Audio sources
		///////////////////////////////////////////////////////////////////////////	
		private const int AUDIO_SOURCE_COUNT = 2;
		private AudioSource[]		m_AudioSources;
		private AmbientSoundType[]	m_CurPlayingSoundTypes;
		private float[]				m_SourceMaxVolume;

		///////////////////////////////////////////////////////////////////////////	
		// Audio clips
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private AmbientSoundData m_AudioLoopWater;
		[SerializeField] private AmbientSoundData m_AudioLoopGrass;
		[SerializeField] private AmbientSoundData m_AudioLoopRuins;

		///////////////////////////////////////////////////////////////////////////	
		// Mixer & Tracks
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private AudioMixer		m_AudioMixer;
		private AudioMixerGroup					m_AmbientMixerGroup;

		///////////////////////////////////////////////////////////////////////////	
		// What does it mean to "mute" "unmute"?
		///////////////////////////////////////////////////////////////////////////	
		private const float					AMBIENT_GROUP_VOLUME_MUTED	= -80.0f;
		[SerializeField] private  float		m_AmbientGroupVolumeDefault	= 0.0f;
		[SerializeField] private  float		m_FadeOutTime				= 2.0f;
		[SerializeField] private  float		m_FadeInTime				= 3.0f;
		[SerializeField] private  float		m_PanoramaFadeTime			= 3.0f;

		[Range(0,1)]
		[SerializeField] private  float		m_MaxStereo					= 0.7f;

		///////////////////////////////////////////////////////////////////////////	
		// Ambient Grid
		///////////////////////////////////////////////////////////////////////////	
		[SerializeField] private  int		m_AmbientGridCellSize		= 16;

		[SerializeField] private int		m_AmbientGridDimension		= 0;

		[SerializeField] AmbientSoundCell[]	m_AmbientGrid;

		[SerializeField] Vector2			m_TerrainSizeWS;

		///////////////////////////////////////////////////////////////////////////

		List<Vector2>						m_DebugListenerPositions = new List<Vector2>();

		///////////////////////////////////////////////////////////////////////////

		public static AmbientSoundManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
		}

		///////////////////////////////////////////////////////////////////////////	

		void Start() 
		{
			Debug.Assert(m_TerrainSizeWS == TerrainManager.instance.terrainSizeWS, "Please Regenerate Map!");

			m_AmbientMixerGroup = m_AudioMixer.FindMatchingGroups("Ambient")[0];

			Debug.Assert(gameObject.GetComponent<AudioSource>() == null);

			m_AudioSources			= new AudioSource[AUDIO_SOURCE_COUNT];
			m_CurPlayingSoundTypes	= new AmbientSoundType[AUDIO_SOURCE_COUNT];
			m_SourceMaxVolume			= new float[AUDIO_SOURCE_COUNT];

			for (int i = 0; i < AUDIO_SOURCE_COUNT; ++i)
			{
				m_AudioSources[i]				= gameObject.AddComponent<AudioSource>();
				m_CurPlayingSoundTypes[i]		= AmbientSoundType.Invalid;
				m_SourceMaxVolume[i]			= 1.0f;

				m_AudioSources[i].outputAudioMixerGroup = m_AmbientMixerGroup;
			}
		}
		
		///////////////////////////////////////////////////////////////////////////	

		struct SamplePosition
		{
			public SamplePosition(Vector2 pos, float priority, int leftAmount, int rightAmount)
			{
				Pos			= pos;
				Priority	= priority;
				LeftAmount	= leftAmount;
				RightAmount = rightAmount;
			}

			public Vector2	Pos;
			public float	Priority;
			public int		LeftAmount;
			public int		RightAmount;
		}

		void Update() 
		{
			if (m_AmbientGrid == null)
			{ 
				Debug.Log("AmbientGrid not Active. Please Regenerate Terrain!");
				return;
			}

			// 0) Mute / Adjust Volume?
			if (CheatManager.instance.disableAudio)
			{
				m_AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", AMBIENT_GROUP_VOLUME_MUTED);
			}
			else
			{
				m_AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", m_AmbientGroupVolumeDefault);
			}

			// 1) Get sub-listener positions
			Vector3 listenerPosition	= CameraController.instance.GetListenerPosition();
			float	listenerWidth		= CameraController.instance.GetListenerWidth();

			listenerWidth = Mathf.Max(listenerWidth, 10.0f);

			List<SamplePosition> samplePositionsByPriority = new List<SamplePosition>();

			// upper
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2(-1, 1) * listenerWidth, 0.1f, 1, 0));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2( 0, 1) * listenerWidth, 0.1f, 1, 1));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2( 1, 1) * listenerWidth, 0.1f, 0, 1));

			// middle
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2(-1, 0) * listenerWidth, 0.5f, 2, 0));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz()							           , 1.0f, 2, 2));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2( 1, 0) * listenerWidth, 0.5f, 0, 2));

			// lower
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2(-1,-1) * listenerWidth, 0.5f, 1, 0));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2( 0,-1) * listenerWidth, 0.5f, 1, 1));
			samplePositionsByPriority.Add(new SamplePosition(listenerPosition.xz() + new Vector2( 1,-1) * listenerWidth, 0.1f, 0, 1));
			

			// 2) find out which 2 sound types dominate and whether they are l/r or balanced
			AmbientSoundType[] soundType	= {AmbientSoundType.Invalid, AmbientSoundType.Invalid};
			int[] leftAmount				= {0, 0};
			int[] rightAmount				= {0, 0};
			
			m_DebugListenerPositions.Clear();

			// bottom left and bottom right have highest priority!
			foreach (SamplePosition samplePosition in samplePositionsByPriority)
			{
				AmbientSoundCell curCell = GetAmbientCellSafe(samplePosition.Pos);

				// 2.1) Add new Types
				for (int a = 0; a < 2; ++a)
				{
					if (soundType[a] == curCell.SoundType)
					{
						// not present yet
						break;
					}
					else if (soundType[a] == AmbientSoundType.Invalid)
					{
						// not found until now: only invalids follow. so add.
						soundType[a] = curCell.SoundType;
						break;
					}
				}

				// 2.2) Add up amounts
				for (int a = 0; a < 2; ++a)
				{
					if (soundType[a] == curCell.SoundType)
					{
						leftAmount[a]	+= samplePosition.LeftAmount;
						rightAmount[a]	+= samplePosition.RightAmount;
						break;
					}
				}

				m_DebugListenerPositions.Add(samplePosition.Pos);
			}

			int[]   totalAmounts	= {0, 0};
			float[] panoramas		= {0.0f, 0.0f};
			float[] intensities		= {0.0f, 0.0f};
			int		totalAmountSum	= 0;

			for (int a = 0; a < 2; ++a)
			{
				totalAmounts[a]	= leftAmount[a] + rightAmount[a];
				panoramas[a]	= GetPanoramaAmount(leftAmount[a], rightAmount[a]);
				totalAmountSum += totalAmounts[a];
			}

			for (int a = 0; a < 2; ++a)
			{
				float relativeAmount = (float) totalAmounts[a] / (float) totalAmountSum;
				intensities[a] = Mathf.Pow(relativeAmount, 0.3f);
			}

			int primaryIndex	= (totalAmounts[0] >= totalAmounts[1]) ? 0 : 1;
			int secondaryIndex	= (totalAmounts[0] >= totalAmounts[1]) ? 1 : 0;
			
			// 3) Update Audio Sources
			UpdateAudioSources(soundType[primaryIndex], panoramas[primaryIndex], intensities[primaryIndex], soundType[secondaryIndex], panoramas[secondaryIndex], intensities[secondaryIndex]);
		}

		///////////////////////////////////////////////////////////////////////////	

		float GetPanoramaAmount(int amountLeft, int amountRight)
		{
			if (amountLeft <= 0 && amountRight <= 0)
			{
				return 0.0f;
			}

			// [left, right] in [0,1]
			float normalizedPanorama = (amountRight / (float) (amountLeft + amountRight));

			// [left, right] in [-1, 1]
			float panorama = (normalizedPanorama * 2.0f) - 1.0f;
			panorama *= m_MaxStereo;

			return panorama;
		}

		///////////////////////////////////////////////////////////////////////////	

		void UpdateAudioSources(AmbientSoundType primaryType, float primaryPanorama, float primaryIntensity, AmbientSoundType secondaryType, float secondaryPanorama, float secondaryIntensity)
		{
			bool primaryTypeAlreadyPlaying		= (primaryType		== AmbientSoundType.Invalid);
			bool secondaryTypeAlreadyPlaying	= (secondaryType	== AmbientSoundType.Invalid);

			// 1) Fade out old sources
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				bool isPrimary		= (m_CurPlayingSoundTypes[s] == primaryType)		&& (primaryType != AmbientSoundType.Invalid);
				bool isSecondary	= (m_CurPlayingSoundTypes[s] == secondaryType)	&& (secondaryType != AmbientSoundType.Invalid);

				if (isPrimary)
				{
					primaryTypeAlreadyPlaying = true;
				}
				else if (isSecondary)
				{
					secondaryTypeAlreadyPlaying = true;
				}
				else 
				{
					// slowly fade out sound
					float newVolume = FadeAudioSource(m_AudioSources[s], null, m_SourceMaxVolume[s], 0.0f);

					if (newVolume == 0.0f)
					{
						m_AudioSources[s].Stop();
						m_CurPlayingSoundTypes[s] = AmbientSoundType.Invalid;
					}
				}
			}

			// 2) Start new sources
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				if (m_CurPlayingSoundTypes[s] == AmbientSoundType.Invalid)
				{
					bool startPrimaryNext	= !primaryTypeAlreadyPlaying;
					bool startSecondaryNext = !startPrimaryNext && !secondaryTypeAlreadyPlaying ;

					if (!startPrimaryNext && !startSecondaryNext)
					{
						break;
					}

					AmbientSoundType startType;
					float startPanorama;

					if (startPrimaryNext)
					{
						startType		= primaryType;
						startPanorama	= primaryPanorama;
						primaryTypeAlreadyPlaying = true;
					}
					else
					{
						startType		= secondaryType;
						startPanorama	= secondaryPanorama;
						secondaryTypeAlreadyPlaying = true;
					}

					StartAudioSource(s, startType, startPanorama);
				}
			}

			// 3) Fade in / adjust panorama
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				if (m_CurPlayingSoundTypes[s] == AmbientSoundType.Invalid)
				{
					continue;
				}

				bool isPrimary		= (m_CurPlayingSoundTypes[s] == primaryType);
				bool isSecondary	= (m_CurPlayingSoundTypes[s] == secondaryType);

				if (isPrimary || isSecondary)
				{
					float intensity = isPrimary ? primaryIntensity : secondaryIntensity;

					FadeAudioSource(m_AudioSources[s], isPrimary ? primaryPanorama : secondaryPanorama, m_SourceMaxVolume[s], m_SourceMaxVolume[s] * intensity);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////	

		void StartAudioSource(int audioSourceIndex, AmbientSoundType ambientType, float initPanorama)
		{
			AudioSource audioSource = m_AudioSources[audioSourceIndex];

			AmbientSoundData ambientSoundData;

			switch (ambientType)
			{
				case AmbientSoundType.Water:
					ambientSoundData = m_AudioLoopWater;
					break;
				case AmbientSoundType.Grass:
					ambientSoundData = m_AudioLoopGrass;
					break;
				case AmbientSoundType.Ruins:
					ambientSoundData = m_AudioLoopRuins;
					break;
				default:
					Debug.Assert(false);
					return;
			}

			if (ambientSoundData == null)
			{
				Debug.Assert(false, "not all ambient sounds set");
				return;
			}

			AudioClip audioClip = ambientSoundData.audioClip;

			audioSource.clip		= audioClip;
			audioSource.loop		= true;
			audioSource.panStereo	= initPanorama;
			audioSource.volume		= 0.0f;
			audioSource.Play();

			m_CurPlayingSoundTypes[audioSourceIndex]	= ambientType;
			m_SourceMaxVolume[audioSourceIndex]			= ambientSoundData.volumeFactor;
		}

		///////////////////////////////////////////////////////////////////////////	

		float FadeAudioSource(AudioSource audioSource, float? newTargetPanorama, float maxVolume, float targetVolume)
		{
			float oldVolume = audioSource.volume;

			bool isFadeIn = targetVolume > oldVolume;
			float fadeTime = isFadeIn ? m_FadeInTime : m_FadeOutTime;
			float fadePerSecond	= (1.0f / Mathf.Max(fadeTime, 0.001f)) * (isFadeIn ? 1.0f : -1.0f);

			// (fade/second) * (second/tick) = fade/tick
			float fadePerTick = fadePerSecond * Time.unscaledDeltaTime;

			float newVolume = audioSource.volume + fadePerTick;

			if (isFadeIn)
			{
				newVolume = Mathf.Min(newVolume, targetVolume);
			}
			else
			{
				newVolume = Mathf.Max(newVolume, targetVolume);
			}

			newVolume = Mathf.Clamp(newVolume, 0.0f, maxVolume);

			audioSource.volume = newVolume;
			
			if (newTargetPanorama != null)
			{
				float oldPanorama = audioSource.panStereo;
				float panoramaFadePerSecond = (1.0f / Mathf.Max(m_PanoramaFadeTime, 0.001f));
				panoramaFadePerSecond *= (newTargetPanorama.Value > oldPanorama) ? 1.0f : -1.0f;

				float panoramaFadePerTick = panoramaFadePerSecond * Time.unscaledDeltaTime;

				float newPanorama = oldPanorama + panoramaFadePerTick;

				if ((panoramaFadePerTick > 0.0f) && (newPanorama > newTargetPanorama) || (panoramaFadePerTick <= 0.0f) && (newPanorama < newTargetPanorama))
				{
					newPanorama = newTargetPanorama.Value;
				}

				audioSource.panStereo = newPanorama;
			}

			return newVolume;
		}

		///////////////////////////////////////////////////////////////////////////	

		AmbientSoundCell GetAmbientCellSafe(Vector2 posWS)
		{
			int iX = (int) (posWS.x / m_AmbientGridCellSize);
			int iZ = (int) (posWS.y / m_AmbientGridCellSize);

			iX = Mathf.Clamp(iX, 0, m_AmbientGridDimension - 1);
			iZ = Mathf.Clamp(iZ, 0, m_AmbientGridDimension - 1);

			return m_AmbientGrid[iX * m_AmbientGridDimension + iZ];
		}

		///////////////////////////////////////////////////////////////////////////	

		public void OnDrawGizmosSelected()
		{
			if (m_AmbientGrid == null)
			{ 
				Debug.Log("AmbientGrid not Active. Please Regenerate Terrain!");
				return;
			}

			float debugDrawHeight = 1.0f;

			for (int iX = 0; iX < m_AmbientGridDimension; iX++)
			{
				for (int iZ = 0; iZ < m_AmbientGridDimension; iZ++)
				{
					Vector2 cellMin = new Vector2(iX * m_AmbientGridCellSize, iZ * m_AmbientGridCellSize);
					Vector2 cellMax = cellMin + new Vector2(m_AmbientGridCellSize, m_AmbientGridCellSize);
					
					Vector2 cellMinOffsetted = Vector2.Lerp(cellMin, cellMax, 0.05f);
					Vector2 cellMaxOffsetted = Vector2.Lerp(cellMin, cellMax, 0.95f);

					AmbientSoundCell curCell = m_AmbientGrid[iX * m_AmbientGridDimension + iZ];

					Color col = curCell.GetDebugColor();		
					col.a = 0.5f;	

					DebugHelper.BufferQuad(new Vector3(cellMinOffsetted.x, debugDrawHeight, cellMinOffsetted.y), new Vector3(cellMaxOffsetted.x, debugDrawHeight, cellMaxOffsetted.y), col);
				}
			}

			for (int i = 0; i < m_DebugListenerPositions.Count; ++i)
			{
				Vector2 pos2D = m_DebugListenerPositions[i];
				
				Color debugColor = AmbientSoundCell.GetDebugColor(GetAmbientCellSafe(pos2D).SoundType);
				
				Gizmos.color = debugColor;
				Gizmos.DrawCube(pos2D.To3D(debugDrawHeight), new Vector3(1, 1, 1)); 
			}

			DebugHelper.DrawBufferedTriangles();
		}

		///////////////////////////////////////////////////////////////////////////	

		public void GenerateAmbientGrid(Terrain.RegionTile[,] regionTiles, List<Terrain.RegionCell> regionCells, Vector2 terrainSizeWS)
		{
			if (regionTiles == null)
			{
				m_AmbientGrid				= null;
				m_TerrainSizeWS			= new Vector2(0.0f, 0.0f);
				m_AmbientGridDimension	= 0;
				return;
			}

			m_TerrainSizeWS = terrainSizeWS;

			m_AmbientGridDimension = (int) Mathf.Ceil(Mathf.Max(terrainSizeWS.x, terrainSizeWS.y) / (float) m_AmbientGridCellSize);
			m_AmbientGridDimension = Mathf.Max(m_AmbientGridDimension, 1);
			
			m_AmbientGrid = new AmbientSoundCell[m_AmbientGridDimension * m_AmbientGridDimension];

			int regionDimensionX = regionTiles.GetLength(0);
			int regionDimensionZ = regionTiles.GetLength(1);
			Vector2 regionTileSize = new Vector2(terrainSizeWS.x / regionDimensionX, terrainSizeWS.y / regionDimensionZ);

			for (int ambientX = 0 ; ambientX < m_AmbientGridDimension; ++ambientX)
			{
				float xWS		= (ambientX + 0.5f) * m_AmbientGridCellSize;
				int regionX		= (int) (xWS / regionTileSize.x);
				regionX			= Mathf.Min(regionX, regionDimensionX - 1);

				for (int ambientZ = 0 ; ambientZ < m_AmbientGridDimension; ++ambientZ)
				{
					float zWS		= (ambientZ + 0.5f) * m_AmbientGridCellSize;
					int regionZ		= (int) (zWS / regionTileSize.y);
					regionZ			= Mathf.Min(regionZ, regionDimensionZ - 1);

					Terrain.RegionTile cellsCenterTile = regionTiles[regionX, regionZ];
					Terrain.RegionType regionType = regionCells[cellsCenterTile.Cell].RegionType;

					AmbientSoundCell curCell = new AmbientSoundCell(AmbientSoundType.Grass);

					switch (regionType)
					{
						case Terrain.RegionType.Bricks:
							curCell.SoundType = AmbientSoundType.Ruins;
							break;

						case Terrain.RegionType.Beach:
						case Terrain.RegionType.Water:
							curCell.SoundType = AmbientSoundType.Water;
							break;
					}

					m_AmbientGrid[ambientX * m_AmbientGridDimension + ambientZ] = curCell;
				}
			}
		}
	}
}