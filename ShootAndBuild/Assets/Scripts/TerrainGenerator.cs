using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace SAB
{
	public enum TerrainTexturePairs
	{
		PlaneA,
		PlaneB,
		Rock,

		Count
	};

	[System.Serializable]
	public class VoronoiParameters
	{
		[Header("Generation")]
		[Range(3, 1000)]
		public int VoronoiPointCount = 600;

		[Range(0.0f, 4.0f)]
		public float RelaxationAmount = 4.0f;

		[Header("Debug")]
		public bool ShowDelauney		= false;
		public bool ShowVoronoi			= false;
		public bool ShowPoints			= false;
		public bool ShowIndices			= false;
		public bool ShowBorder			= false;
		public bool ShowVorArea			= false;
		public bool ShowVorOrigentation = false;

		[Range(-1, 50)]
		public int DebugDrawOnlyVertexIndex		= -1;

		[Range(-1, 50)]
		public int DebugDrawOnlyTriangleIndex	= -1;
		
		public bool SuppressClamping		= false;
		public bool SuppressNewBorderEdges	= false;
	}

	[System.Serializable]
	public class RegionParameters
	{
		[Header("Generation")]
		public bool Test				= true;

		[Header("Debug")]
		public bool ShowRegions	= false;
		public bool ShowIndices	= false;
	}

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
	public class TexturePair
	{
		public Texture2D Albedo1;
		public Texture2D Albedo2;

		public Texture2D Normal1;
		public Texture2D Normal2;

		[Range(0.5f, 20.0f)]
		public float Tiling1 = 4.0f;
		
		[Range(0.5f, 20.0f)]
		public float Tiling2 = 4.0f;

		[Range(1.0f, 20.0f)]
		public float BlendingSharpness12 = 7.0f;

		[Range(0.005f, 1.0f)]
		public float PerlinFrequency12	= 0.015f;

		[Range(0.000f, 1.0f)]
		public float Share2ndTexture = 0.4f;

		public SplatPrototype CreateSplatPrototype(bool second)
		{
			SplatPrototype prototype = new SplatPrototype();

			prototype.texture	= second ? Albedo2 : Albedo1;
			prototype.normalMap = second ? Normal2 : Normal1;

			float tiling = second ? Tiling2 : Tiling1;
			prototype.tileSize	= new Vector2(tiling, tiling);

			return prototype;
		}

		public float GetBlendValue12(float seed, float x, float z)
		{
			float perlinCoarse = Mathf.PerlinNoise(seed + x * PerlinFrequency12, seed + z * PerlinFrequency12);
			
			float amount2nd = TerrainGenerator.RedistributeBlendFactor(perlinCoarse, Share2ndTexture);

			return TerrainGenerator.ApplySharpness(amount2nd, BlendingSharpness12);
		}
	}

	[System.Serializable]
	public class TextureParameters
	{
		public TexturePair TexturePlaneA;
		public TexturePair TexturePlaneB;
		public TexturePair TextureRock;
		
		[Range(1.0f, 20.0f)]
		public float RockBlendingSharpness	= 7.0f;

		[Range(0.0f, 1.0f)]
		public float PlaneBShare = 0.5f;

		[Range(0.0f, 90.0f)]
		public float RockSteepnessAngle	= 55.0f;
	}

	// -----------------------------------------------------------------

	public class TerrainGenerator : MonoBehaviour
	{
		public int Resolution = 129;

		public bool UseTimeAsSeed = true;

		public int TerrainSeed = 1000;
		public int VoronoiSeed = 1000;
		public int RegionSeed  = 1000;

		public TransformParameters			TransformParams;
		public HeightGenerationParameters	HeightParams;
		public TextureParameters			TextureParams;
		public VoronoiParameters			VoronoiParams;
		public RegionParameters				RegionParams;

		GameObject TerrainObject;
		public VoronoiCreator VoronoiGenerator		= new VoronoiCreator();
		public RegionMapGenerator RegionGenerator	= new RegionMapGenerator();

		// -----------------------------------------------------------------

		public int TexturePairToSplatIndex(TerrainTexturePairs pair, bool second)
		{
			return ((int) pair) * 2 + (second ? 1 : 0);
		}

		// -----------------------------------------------------------------

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
		
		// -----------------------------------------------------------------

		public void OnDrawGizmosSelected()
		{
			VoronoiGenerator.DebugDraw(VoronoiParams);
			RegionGenerator.DebugDraw();
		}

		// -----------------------------------------------------------------

		public void RegenerateAll()
		{
			if (UseTimeAsSeed)
			{
				int timeSeed = (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000) % 100000;
				TerrainSeed = timeSeed;
				VoronoiSeed = timeSeed;
				RegionSeed  = timeSeed;
			}

			List<VoronoiCell> voronoiCells = VoronoiGenerator.GenerateVoronoi(VoronoiSeed, VoronoiParams, TransformParams.TerrainCenter, TransformParams.TerrainSizeWS);

			if (voronoiCells == null)
			{
				return;
			}

			RegionGenerator.GenerateRegions(voronoiCells, RegionParams);

			return;
/*
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
			TerrainData.splatPrototypes = new SplatPrototype[] 
			{
				TextureParams.TexturePlaneA.CreateSplatPrototype(false),
				TextureParams.TexturePlaneA.CreateSplatPrototype(true),
				TextureParams.TexturePlaneB.CreateSplatPrototype(false),
				TextureParams.TexturePlaneB.CreateSplatPrototype(true),
				TextureParams.TextureRock.CreateSplatPrototype(false),
				TextureParams.TextureRock.CreateSplatPrototype(true),
			};

			TerrainData.RefreshPrototypes();
			TerrainData.SetAlphamaps(0, 0, GenerateTextureMaps(TerrainData));

			//Create terrain
			TerrainObject = Terrain.CreateTerrainGameObject(TerrainData);

			TerrainObject.transform.position = new Vector3(
				TransformParams.TerrainCenter.x - TransformParams.TerrainSizeWS.x * 0.5f, 
				TransformParams.HeightMin, 
				TransformParams.TerrainCenter.y - TransformParams.TerrainSizeWS.y * 0.5f);

			TerrainObject.GetComponent<Terrain>().Flush();

			TerrainObject.transform.parent = this.transform;*/
		}

		// -----------------------------------------------------------------

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

		// -----------------------------------------------------------------
	
		public static float RedistributeBlendFactor(float blend12, float share2)
		{
			if (share2 == 0.0f) { return 0.0f; }
			if (share2 == 1.0f) { return 1.0f; }

			float amount1st = Mathf.InverseLerp(Mathf.Max(2.0f * share2 - 1.0f, 0.0f), Mathf.Min(2.0f * share2, 1.0f), blend12);

			return 1.0f - amount1st;
		}

		

		public static float ApplySharpness(float oldBlendValue, float blendingSharpness)
		{
			if (oldBlendValue < 0.5f)
			{
				return Mathf.Pow(oldBlendValue * 2.0f, blendingSharpness) * 0.5f;
			}
			
			return 1.0f - (Mathf.Pow((1.0f - oldBlendValue) * 2.0f, blendingSharpness) * 0.5f);
		}

		// -----------------------------------------------------------------

		public float GetAmountPlaneA(int x, int z)
		{
			// We later want to get this from District Map
			float plane2Amount = Mathf.PerlinNoise(TerrainSeed + 21 + x * 0.01f, TerrainSeed + 21 + z * 0.01f);

			plane2Amount = RedistributeBlendFactor(plane2Amount, TextureParams.PlaneBShare);

			plane2Amount = ApplySharpness(plane2Amount, 9.0f);

			return 1.0f - plane2Amount;
		}

		// -----------------------------------------------------------------

		public float[,,] GenerateTextureMaps(TerrainData TerrainData)
		{
			float[,,] textureMaps = new float[Resolution, Resolution, (int) TerrainTexturePairs.Count * 2];

			for (int x = 0; x < Resolution; x++)
			{
				for (int z = 0; z < Resolution; z++)
				{
					float normalizedX = (float)x / ((float)Resolution - 1f);
					float normalizedZ = (float)z / ((float)Resolution - 1f);

					float steepness = (TextureParams.RockSteepnessAngle == 0.0f) ? 1.0f : TerrainData.GetSteepness(normalizedX, normalizedZ) / TextureParams.RockSteepnessAngle;
					steepness = Mathf.Min(steepness, 1.0f);

					float rockAmount = ApplySharpness(steepness, TextureParams.RockBlendingSharpness);

					float blend12 = TextureParams.TextureRock.GetBlendValue12(TerrainSeed + 10, x, z);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.Rock, false)]	= rockAmount * (1.0f - blend12);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.Rock, true)]	= rockAmount * blend12;

					float planeAmount = 1.0f - rockAmount;

					float planeAAmount = planeAmount * GetAmountPlaneA(x, z);
					float planeBAmount = planeAmount - planeAAmount;

					blend12 = TextureParams.TexturePlaneA.GetBlendValue12(TerrainSeed + 20, x, z);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.PlaneA, false)]	= planeAAmount * (1.0f - blend12);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.PlaneA, true)]	= planeAAmount * blend12;

					blend12 = TextureParams.TexturePlaneB.GetBlendValue12(TerrainSeed + 20, x, z);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.PlaneB, false)]	= planeBAmount * (1.0f - blend12);
					textureMaps[z, x, TexturePairToSplatIndex(TerrainTexturePairs.PlaneB, true)]	= planeBAmount * blend12;

					float weightSum = 0.0f;

					for (int t = 0; t < (int) TerrainTexturePairs.Count * 2; ++t)
					{
						weightSum += textureMaps[z, x, t];
					}

					Debug.Assert(Mathf.Abs(weightSum - 1.0f) < 0.01f);
				}
			}
			return textureMaps;
		}

		// -----------------------------------------------------------------

		public float GetNormalizedHeight(float x, float z)
		{
			float perlinCoarse = Mathf.PerlinNoise(TerrainSeed + x * HeightParams.PerlinFrequencyCoarse,	TerrainSeed + z * HeightParams.PerlinFrequencyCoarse);
			float perlinFine   = Mathf.PerlinNoise(TerrainSeed + x * HeightParams.PerlinFrequencyFine,		TerrainSeed + z * HeightParams.PerlinFrequencyFine);
			
			float noiseTotal = perlinCoarse * HeightParams.PerlinWeightCoarse + perlinFine * HeightParams.PerlinWeightFine;
			noiseTotal /= (HeightParams.PerlinWeightFine + HeightParams.PerlinWeightCoarse);

			return noiseTotal;
		}

	}

	// -----------------------------------------------------------------

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
				EditorUtility.SetDirty(terrainGenerator);
			}
		}
	}
}