using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SAB
{
	enum TerrainTextures
	{
		Grass,
		Rock,

		Count
	};

	[System.Serializable]
	public class TransformParameters
	{
		public Vector2 TerrainCenter = new Vector2(0.0f, 0.0f);
		public Vector2 TerrainSizeWS = new Vector2(64.0f, 64.0f);

		[Range(-5.0f, 20.0f)]
		public float HeightMax =  6.0f;

		[Range(-10.0f, 5.0f)]
		public float HeightMin =  0.0f;
	}

	[System.Serializable]
	public class HeightGenerationParameters
	{
		[Range(0.005f, 0.1f)]
		public float PerlinFrequencyCoarse	= 0.015f;
		[Range(0.005f, 0.5f)]
		public float PerlinFrequencyFine	= 0.10f;

		[Range(0.0f, 1.0f)]
		public float PerlinWeightCoarse		= 0.50f;

		[Range(0.0f, 1.0f)]
		public float PerlinWeightFine		= 0.15f;
		public bool DebugHeightRange		= false;
	}

	[System.Serializable]
	public class TextureParameters
	{
		public Texture2D GrassAlbedo;
		public Texture2D GrassNormal;

		public Texture2D RockAlbedo;
		public Texture2D RockNormal;

		[Range(0.5f, 20.0f)]
		public float GrassTiling		= 4.0f;

		[Range(0.5f, 20.0f)]
		public float RockTiling			= 4.0f;

		[Range(1.0f, 20.0f)]
		public float BlendingSharpness	= 7.0f;

		[Range(0.0f, 90.0f)]
		public float RockSteepnessAngle	= 55.0f;
	}

	public class TerrainGenerator : MonoBehaviour
	{
		public int Resolution = 129;

		public bool UseTimeAsSeed = true;
		public int  Seed = 1000;

		public TransformParameters			TransformParams;
		public HeightGenerationParameters	HeightParams;
		public TextureParameters			TextureParams;
		
		GameObject TerrainObject;

		// Use this for initialization
		void Start ()
		{
			
		}

		public void DeleteTerrain()
		{
			if (!TerrainObject)
			{
				Terrain childTerrain = GetComponentInChildren<Terrain>();
				TerrainObject = childTerrain ? childTerrain.gameObject : null;
			}

			if (TerrainObject)
			{
				if (Application.isPlaying)
				{
					Destroy(TerrainObject);
				}
				else
				{
					DestroyImmediate(TerrainObject);
				}
			}
		}

		public void RegenerateAll()
		{
			if (UseTimeAsSeed)
			{
				Seed = System.DateTime.Now.Millisecond % 100000;
			}

			DeleteTerrain();

			TerrainData TerrainData = new TerrainData();
			TerrainData.alphamapResolution = Resolution;

			//Set heights
			TerrainData.heightmapResolution = Resolution;
			TerrainData.SetHeights(0, 0, GenerateHeightMap());
			TerrainData.size = new Vector3(
				TransformParams.TerrainSizeWS.x, 
				TransformParams.HeightMax - TransformParams.HeightMin, 
				TransformParams.TerrainSizeWS.y);

			//Set textures
			SplatPrototype Grass	= new SplatPrototype();
			SplatPrototype Rock		= new SplatPrototype();

			Grass.texture	= TextureParams.GrassAlbedo;
			Grass.normalMap = TextureParams.GrassNormal;
			Grass.tileSize	= new Vector2(TextureParams.GrassTiling, TextureParams.GrassTiling);

			Rock.texture	= TextureParams.RockAlbedo;
			Rock.normalMap	= TextureParams.RockNormal;
			Rock.tileSize	= new Vector2(TextureParams.RockTiling, TextureParams.RockTiling);
			
			TerrainData.splatPrototypes = new SplatPrototype[] { Grass, Rock };
			TerrainData.RefreshPrototypes();
			TerrainData.SetAlphamaps(0, 0, GenerateTextureMaps(TerrainData));

			//Create terrain
			TerrainObject = Terrain.CreateTerrainGameObject(TerrainData);
			TerrainObject.transform.position = new Vector3(
				TransformParams.TerrainCenter.x - TransformParams.TerrainSizeWS.x * 0.5f, 
				TransformParams.HeightMin, 
				TransformParams.TerrainCenter.y - TransformParams.TerrainSizeWS.y * 0.5f);
			TerrainObject.GetComponent<Terrain>().Flush();

			TerrainObject.transform.parent = this.transform;
		}


		public float[,] GenerateHeightMap()
		{
			float[,] Heightmap = new float[Resolution, Resolution];

			float maxHeight = 0.0f;
			float minHeight = 1.0f;

			for (int x = 0; x < Resolution; x++)
			{
				for (int z = 0; z < Resolution; z++)
				{
					float height = GetNormalizedHeight((float)x, (float)z);
					Heightmap[x, z] = height;

					maxHeight = Mathf.Max(maxHeight, height);
					minHeight = Mathf.Min(minHeight, height);
				}
			}

			float heightRange = (maxHeight - minHeight);
			float rHeightRange = 1.0f / heightRange;

			for (int x = 0; x < Resolution; x++)
			{
				for (int z = 0; z < Resolution; z++)
				{
					Heightmap[x,z] = rHeightRange * (Heightmap[x,z] - minHeight);

					if (HeightParams.DebugHeightRange && x < 50)
					{
						Heightmap[x,z] = (z > 50 ? 1.0f : 0.0f);
					}
				}
			}

			return Heightmap;
		}


		public float[,,] GenerateTextureMaps(TerrainData TerrainData)
		{
			float[,,] textureMaps = new float[Resolution, Resolution, (int) TerrainTextures.Count];

			for (int x = 0; x < Resolution; x++)
			{
				for (int z = 0; z < Resolution; z++)
				{
					float normalizedX = (float)x / ((float)Resolution - 1f);
					float normalizedZ = (float)z / ((float)Resolution - 1f);

					float steepness = TextureParams.RockSteepnessAngle == 0.0f ? 0.0f : TerrainData.GetSteepness(normalizedX, normalizedZ) / TextureParams.RockSteepnessAngle;

					float rockAmount = steepness;

					if (rockAmount < 0.5f)
					{
						rockAmount = Mathf.Pow(rockAmount * 2.0f, TextureParams.BlendingSharpness) * 0.5f;
					}
					else
					{
						rockAmount = 1.0f - (Mathf.Pow((1.0f - rockAmount) * 2.0f, TextureParams.BlendingSharpness) * 0.5f);
					}

					textureMaps[z, x, (int) TerrainTextures.Rock]	= rockAmount;
					textureMaps[z, x, (int) TerrainTextures.Grass]	= 1.0f - rockAmount;
				}
			}
			return textureMaps;
		}

		public float GetNormalizedHeight(float x, float z)
		{
			float perlinCoarse = Mathf.PerlinNoise(Seed + x * HeightParams.PerlinFrequencyCoarse,	Seed + z * HeightParams.PerlinFrequencyCoarse);
			float perlinFine   = Mathf.PerlinNoise(Seed + x * HeightParams.PerlinFrequencyFine,		Seed + z * HeightParams.PerlinFrequencyFine);
			
			float noiseTotal = perlinCoarse * HeightParams.PerlinWeightCoarse + perlinFine * HeightParams.PerlinWeightFine;
			noiseTotal /= (HeightParams.PerlinWeightFine + HeightParams.PerlinWeightCoarse);

			return noiseTotal;
		}

	}

	[CustomEditor(typeof(TerrainGenerator))]
	public class TerrainGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainGenerator terrainGenerator = (TerrainGenerator)target;

			DrawDefaultInspector();

			GUILayout.Label("Generate", EditorStyles.boldLabel);
			if (GUILayout.Button("Regenerate"))
			{
				terrainGenerator.RegenerateAll();
			}
		}
	}
}