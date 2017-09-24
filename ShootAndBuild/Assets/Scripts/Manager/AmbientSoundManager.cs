using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// ExtensionMethods-Helper to allow for syntax vector3.xz();
// TODO: Put in separate file
public static class Vector3Extensions
{
     public static Vector2 xz(this Vector3 vec3D)
     {
         return new Vector2(vec3D.x, vec3D.z);
     }

	 public static Vector3 To3D(this Vector2 vec2D, float y = 0.0f)
     {
         return new Vector3(vec2D.x, y, vec2D.y);
     }
}

namespace SAB
{
	enum AmbientSoundType
	{
		Grass,
		Water,

		Invalid
	}

	struct AmbientSoundCell
	{
		public AmbientSoundType	SoundType;

		public AmbientSoundCell(AmbientSoundType soundType)
		{
			SoundType = soundType;
		}

		public static Color GetDebugColor(AmbientSoundType soundType)
		{
			switch (soundType)
			{
				case AmbientSoundType.Water:
					return new Color(0,0,1);
				case AmbientSoundType.Grass:
					return new Color(0,1,0);
			}

			return new Color(0.5f, 0.5f, 0.5f);
		}

		public Color GetDebugColor()
		{
			return GetDebugColor(SoundType);
		}
	}


	public class AmbientSoundManager : MonoBehaviour 
	{
		//-------------------------------------------------	
		// Audio sources
		//-------------------------------------------------	
		private const int AUDIO_SOURCE_COUNT = 2;
		private AudioSource[]		AudioSources;
		private AmbientSoundType[]	CurPlayingSoundTypes;

		//-------------------------------------------------	
		// Audio clips
		//-------------------------------------------------	
		public AudioClip	AudioLoopWater;
		public AudioClip	AudioLoopGrass;

		//-------------------------------------------------	
		// Mixer & Tracks
		//-------------------------------------------------	
		public AudioMixer		AudioMixer;
		private AudioMixerGroup AmbientMixerGroup;

		//-------------------------------------------------	
		// What does it mean to "mute" "unmute"?
		//-------------------------------------------------	
		private const float AmbientGroupVolumeMuted		= -80.0f;
		public float		AmbientGroupVolumeDefault	= 0.0f;
		public float		FadeOutTime					= 2.0f;
		public float		FadeInTime					= 3.0f;
		public float		PanoramaFadeTime			= 3.0f;

		[Range(0,1)]
		public float		MaxStereo					= 0.7f;

		//-------------------------------------------------	
		// Ambient Grid
		//-------------------------------------------------	
		public	int	AmbientGridCellSize		= 16;
		private int AmbientGridDimension	= 0;

		AmbientSoundCell[,] AmbientGrid;
		Vector2				TerrainSizeWS;

		//-------------------------------------------------	
		// Listeners
		//-------------------------------------------------	
		Vector2[]			SubListenerPositions = new Vector2[4];

		void Awake()
		{
			Instance = this;
		}

		//-------------------------------------------------	

		void Start() 
		{
			Debug.Assert(TerrainSizeWS == TerrainManager.Instance.TerrainSizeWS, "Please Regenerate Map!");

			AmbientMixerGroup = AudioMixer.FindMatchingGroups("Ambient")[0];

			Debug.Assert(gameObject.GetComponent<AudioSource>() == null);

			AudioSources			= new AudioSource[AUDIO_SOURCE_COUNT];
			CurPlayingSoundTypes	= new AmbientSoundType[AUDIO_SOURCE_COUNT];

			for (int i = 0; i < AUDIO_SOURCE_COUNT; ++i)
			{
				AudioSources[i]			= gameObject.AddComponent<AudioSource>();
				CurPlayingSoundTypes[i]	= AmbientSoundType.Invalid;
			}
		}
		
		//-------------------------------------------------	

		void Update() 
		{
			if (AmbientGrid == null)
			{ 
				Debug.Log("AmbientGrid not Active. Please Regenerate Terrain!");
				return;
			}

			// 0) Mute / Adjust Volume?
			if (CheatManager.instance.disableMusic)
			{
				AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", AmbientGroupVolumeMuted);
			}
			else
			{
				AmbientMixerGroup.audioMixer.SetFloat("AmbientVolume", AmbientGroupVolumeDefault);
			}

			// 1) Get sub-listener positions
			Vector3 listenerPosition	= CameraController.Instance.GetListenerPosition();
			float	listenerWidth		= CameraController.Instance.GetListenerWidth();

			int iBottomLeft		= 0;
			int iBottomRight	= 1;
			int iTopLeft		= 2;
			int iTopRight		= 3;

			SubListenerPositions[iBottomLeft]	= listenerPosition.xz() + new Vector2(-1,-1) * listenerWidth;
			SubListenerPositions[iBottomRight]	= listenerPosition.xz() + new Vector2( 1,-1) * listenerWidth;
			SubListenerPositions[iTopLeft]		= listenerPosition.xz() + new Vector2(-1, 1) * listenerWidth;
			SubListenerPositions[iTopRight]		= listenerPosition.xz() + new Vector2( 1, 1) * listenerWidth;

			// 2) find out which 2 sound types dominate and whether they are l/r or balanced
			AmbientSoundType[] soundType	= {AmbientSoundType.Invalid, AmbientSoundType.Invalid};
			int[] leftAmount				= {0, 0};
			int[] rightAmount				= {0, 0};
			
			// bottom left and bottom right have highest priority!
			for (int i = 0; i < 4; ++i)
			{
				AmbientSoundCell curCell = GetAmbientCellSafe(SubListenerPositions[i]);

				for (int a = 0; a < 2; ++a)
				{
					if (soundType[a] == AmbientSoundType.Invalid)
					{
						soundType[a] = curCell.SoundType;
						break;
					}
				}

				for (int a = 0; a < 2; ++a)
				{
					if (soundType[a] == curCell.SoundType)
					{
						bool isLeft = (i == iBottomLeft) || (i == iTopLeft);

						if (isLeft)
						{
							leftAmount[a]++;
						}
						else
						{
							rightAmount[a]++;
						}
						break;
					}
				}
			}

			int[]   totalAmount = {0, 0};
			float[] panoramas = {0.0f, 0.0f};

			for (int a = 0; a < 2; ++a)
			{
				totalAmount[a]	= leftAmount[a] + rightAmount[a];
				panoramas[a]	= GetPanoramaAmount(leftAmount[a], rightAmount[a]);
			}

			int primaryIndex	= (totalAmount[0] >= totalAmount[1]) ? 0 : 1;
			int secondaryIndex	= (totalAmount[0] >= totalAmount[1]) ? 1 : 0;
			
			// 3) Update Audio Sources
			UpdateAudioSources(soundType[primaryIndex], panoramas[primaryIndex], soundType[secondaryIndex], panoramas[secondaryIndex]);
		}

		//-------------------------------------------------	

		float GetPanoramaAmount(int amountLeft, int amountRight)
		{
			if (amountLeft <= 0 || amountRight <= 0)
			{
				return 0.0f;
			}

			// [left, right] in [0,1]
			float normalizedPanorama = (amountRight / (float) (amountLeft + amountRight));

			// [left, right] in [-1, 1]
			float panorama = (normalizedPanorama * 2.0f) - 1.0f;
			panorama *= MaxStereo;

			return panorama;
		}

		//-------------------------------------------------	

		void UpdateAudioSources(AmbientSoundType primaryType, float primaryPanorama, AmbientSoundType secondaryType, float secondaryPanorama)
		{
			bool primaryTypeAlreadyPlaying		= false;
			bool secondaryTypeAlreadyPlaying	= false;

			// 1) Fade out old sources
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				bool isPrimary		= (CurPlayingSoundTypes[s] == primaryType);
				bool isSecondary	= (CurPlayingSoundTypes[s] == secondaryType);

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
					float newVolume = FadeAudioSource(AudioSources[s], true, null);

					if (newVolume == 0.0f)
					{
						AudioSources[s].Stop();
						CurPlayingSoundTypes[s] = AmbientSoundType.Invalid;
					}
				}
			}

			// 2) Start new sources
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				if (CurPlayingSoundTypes[s] == AmbientSoundType.Invalid)
				{
					bool startPrimaryNext	= !primaryTypeAlreadyPlaying;
					bool startSecondaryNext = !startPrimaryNext && !secondaryTypeAlreadyPlaying;

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

					StartAudioSource(AudioSources[s], startType, startPanorama);
				}
			}

			// 3) Fade in / adjust panorama
			for (int s = 0; s < AUDIO_SOURCE_COUNT; ++s)
			{
				if (CurPlayingSoundTypes[s] == AmbientSoundType.Invalid)
				{
					continue;
				}

				bool isPrimary		= (CurPlayingSoundTypes[s] == primaryType);
				bool isSecondary	= (CurPlayingSoundTypes[s] == secondaryType);

				if (isPrimary || isSecondary)
				{
					FadeAudioSource(AudioSources[s], true, isPrimary ? primaryPanorama : secondaryPanorama);
				}
			}
		}

		//-------------------------------------------------	

		void StartAudioSource(AudioSource audioSource, AmbientSoundType ambientType, float initPanorama)
		{
			AudioClip audioClip = null;

			switch (ambientType)
			{
				case AmbientSoundType.Water:
					audioClip = AudioLoopWater;
					break;
				case AmbientSoundType.Grass:
					audioClip = AudioLoopGrass;
					break;
				default:
					Debug.Assert(false);
					break;
			}

			audioSource.clip		= audioClip;
			audioSource.loop		= true;
			audioSource.panStereo	= initPanorama;
			audioSource.Play();
		}

		//-------------------------------------------------	

		float FadeAudioSource(AudioSource audioSource, bool fadeIn, float? newTargetPanorama)
		{
			float fadeTime = fadeIn ? FadeInTime : FadeOutTime;
			float fadePerSecond	= (1.0f / Mathf.Max(fadeTime, 0.001f)) * (fadeIn ? 1.0f : -1.0f);

			// (fade/second) * (second/tick) = fade/tick
			float fadePerTick = fadePerSecond * Time.unscaledDeltaTime;

			float newVolume = audioSource.volume + fadePerTick;
			newVolume = Mathf.Clamp01(newVolume);

			audioSource.volume = newVolume;
			
			if (newTargetPanorama != null)
			{
				float oldPanorama = audioSource.panStereo;
				float panoramaFadePerSecond = (1.0f / Mathf.Max(PanoramaFadeTime, 0.001f));
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

		//-------------------------------------------------	

		AmbientSoundCell GetAmbientCellSafe(Vector2 posWS)
		{
			int iX = (int) (posWS.x / AmbientGridCellSize);
			int iZ = (int) (posWS.y / AmbientGridCellSize);

			iX = Mathf.Clamp(iX, 0, AmbientGridDimension - 1);
			iZ = Mathf.Clamp(iZ, 0, AmbientGridDimension - 1);

			return AmbientGrid[iX, iZ];
		}

		//-------------------------------------------------	

		public void OnDrawGizmosSelected()
		{
			if (AmbientGrid == null)
			{ 
				Debug.Log("AmbientGrid not Active. Please Regenerate Terrain!");
				return;
			}

			float debugDrawHeight = 1.0f;

			for (int iX = 0; iX < AmbientGridDimension; iX++)
			{
				for (int iZ = 0; iZ < AmbientGridDimension; iZ++)
				{
					Vector2 cellMin = new Vector2(iX * AmbientGridCellSize, iZ * AmbientGridCellSize);
					Vector2 cellMax = cellMin + new Vector2(AmbientGridCellSize, AmbientGridCellSize);
					
					Vector2 cellMinOffsetted = Vector2.Lerp(cellMin, cellMax, 0.05f);
					Vector2 cellMaxOffsetted = Vector2.Lerp(cellMin, cellMax, 0.95f);

					AmbientSoundCell curCell = AmbientGrid[iX, iZ];

					Color col = curCell.GetDebugColor();			

					DebugHelper.BufferQuad(new Vector3(cellMinOffsetted.x, debugDrawHeight, cellMinOffsetted.y), new Vector3(cellMaxOffsetted.x, debugDrawHeight, cellMaxOffsetted.y), col);
				}
			}

			for (int i = 0; i < 4; ++i)
			{
				Vector2 pos2D = SubListenerPositions[i];
				
				Color debugColor = AmbientSoundCell.GetDebugColor(GetAmbientCellSafe(pos2D).SoundType);
				
				Gizmos.color = debugColor;
				Gizmos.DrawCube(pos2D.To3D(debugDrawHeight), new Vector3(1, 1, 1)); 
			}

			DebugHelper.DrawBufferedTriangles();
		}

		//-------------------------------------------------	

		public static AmbientSoundManager Instance
		{
			get; private set;
		}

		//-------------------------------------------------	

		public void GenerateAmbientGrid(Terrain.RegionTile[,] regionTiles, List<Terrain.RegionCell> regionCells, Vector2 terrainSizeWS)
		{
			if (regionTiles == null)
			{
				AmbientGrid				= null;
				TerrainSizeWS			= new Vector2(0.0f, 0.0f);
				AmbientGridDimension	= 0;
				return;
			}

			TerrainSizeWS = terrainSizeWS;

			AmbientGridDimension = (int) Mathf.Ceil(Mathf.Max(terrainSizeWS.x, terrainSizeWS.y) / (float) AmbientGridCellSize);
			AmbientGridDimension = Mathf.Max(AmbientGridDimension, 1);
			
			AmbientGrid = new AmbientSoundCell[AmbientGridDimension, AmbientGridDimension];

			int regionDimensionX = regionTiles.GetLength(0);
			int regionDimensionZ = regionTiles.GetLength(1);
			Vector2 regionTileSize = new Vector2(terrainSizeWS.x / regionDimensionX, terrainSizeWS.y / regionDimensionZ);

			for (int ambientX = 0 ; ambientX < AmbientGridDimension; ++ambientX)
			{
				float xWS		= (ambientX + 0.5f) * AmbientGridCellSize;
				int regionX		= (int) (xWS / regionTileSize.x);
				regionX			= Mathf.Min(regionX, regionDimensionX - 1);

				for (int ambientZ = 0 ; ambientZ < AmbientGridDimension; ++ambientZ)
				{
					float zWS		= (ambientZ + 0.5f) * AmbientGridCellSize;
					int regionZ		= (int) (zWS / regionTileSize.y);
					regionZ			= Mathf.Min(regionZ, regionDimensionZ - 1);

					Terrain.RegionTile cellsCenterTile = regionTiles[regionX, regionZ];
					Terrain.RegionType regionType = regionCells[cellsCenterTile.Cell].RegionType;

					AmbientSoundCell curCell = new AmbientSoundCell(AmbientSoundType.Grass);

					switch (regionType)
					{
						case Terrain.RegionType.Beach:
						case Terrain.RegionType.Water:
							curCell.SoundType = AmbientSoundType.Water;
							break;
					}

					AmbientGrid[ambientX, ambientZ] = curCell;
				}
			}
		}
	}
}