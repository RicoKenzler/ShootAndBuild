using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	// ------------------------------------------

	public class TerrainGenerator
	{
		GameObject TerrainObject;

		public int Resolution = 1;

		public int TerrainSeed = 0;
		
		public TransformParameters			TransformParams;
		public HeightGenerationParameters	HeightParams;
		public TextureParameters			TextureParams;
		public WaterParameters				WaterParams;
		RegionMapTransformation				RegionTransformation;
		
		public GameObject GenerateTerrain(List<RegionCell> regionMap, RegionTile[,] regionGrid, RegionMapTransformation regionMapTransformation, TransformParameters transformParams, HeightGenerationParameters heightParams, TextureParameters textureParams, WaterParameters waterParams, int resolution, int terrainSeed)
		{
			// 0) Init Members
			TerrainObject			= null;
			Resolution				= resolution;
			TerrainSeed				= terrainSeed;
			TransformParams			= transformParams;
			HeightParams			= heightParams;
			TextureParams			= textureParams;
			WaterParams				= waterParams;
			RegionTransformation	= regionMapTransformation;

			// 1) Init TerrainData
			TerrainData terrainData = new TerrainData();

			//Set heights
			terrainData.heightmapResolution = Resolution; 
			terrainData.alphamapResolution = Resolution;

			if (Resolution != terrainData.heightmapResolution)
			{
				Debug.LogWarning("Resolution " + Resolution + " is not supported. Unity suggests: " + terrainData.heightmapResolution);
				return null;
			}

			terrainData.SetHeights(0, 0, GenerateHeightMap());
			terrainData.size = new Vector3(
				TransformParams.TerrainSizeWS.x, 
				TransformParams.HeightMax - TransformParams.HeightMin, 
				TransformParams.TerrainSizeWS.y);

			//Set textures
			terrainData.splatPrototypes = new SplatPrototype[] 
			{
				TextureParams.TexturePlaneA.CreateSplatPrototype(false),
				TextureParams.TexturePlaneA.CreateSplatPrototype(true),
				TextureParams.TexturePlaneB.CreateSplatPrototype(false),
				TextureParams.TexturePlaneB.CreateSplatPrototype(true),
				TextureParams.TextureRock.CreateSplatPrototype(false),
				TextureParams.TextureRock.CreateSplatPrototype(true),
			};

			terrainData.RefreshPrototypes();
			terrainData.SetAlphamaps(0, 0, GenerateTextureMaps(terrainData));

			//Create terrain
			if ((TransformParams.HeightMax - TransformParams.HeightMin) <= 0)
			{
				Debug.LogError("Min/Max heights not correctly set (Max has to be > Min, Collider can only be created by unity when HeightMax != HeightMin)");
				return null;
			}

			TerrainObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);

			TerrainObject.transform.position = new Vector3(
				TransformParams.TerrainCenter.x - TransformParams.TerrainSizeWS.x * 0.5f, 
				TransformParams.HeightMin, 
				TransformParams.TerrainCenter.y - TransformParams.TerrainSizeWS.y * 0.5f);

			UnityEngine.Terrain terrainComponent = TerrainObject.GetComponent<UnityEngine.Terrain>();
			terrainComponent.Flush();

			UnityEngine.TerrainCollider terrainCollider = TerrainObject.GetComponent<UnityEngine.TerrainCollider>();
			terrainCollider.terrainData = terrainData; 

			// 2) Create Water Plane
			CreateWaterPlane(TerrainObject);

			return TerrainObject;
		}

		// -----------------------------------------------------------------

		public void CreateWaterPlane(GameObject terrainObject)
		{
			if (WaterParams.UseWater)
			{
				GameObject waterPlaneObject = MonoBehaviour.Instantiate(WaterParams.WaterPlanePrefab, terrainObject.transform);
				waterPlaneObject.transform.localScale = new Vector3(TransformParams.TerrainSizeWS.x, TransformParams.TerrainSizeWS.y, 1.0f);
				waterPlaneObject.transform.position = new Vector3(TransformParams.TerrainCenter.x, WaterParams.WaterHeight, TransformParams.TerrainCenter.y);		
			}
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

		public float GetAmountPlaneA(int x, int z)
		{
			// We later want to get this from District Map
			float plane2Amount = Mathf.PerlinNoise(TerrainSeed + 21 + x * 0.01f, TerrainSeed + 21 + z * 0.01f);

			plane2Amount = TerrainTextureTypes.RedistributeBlendFactor(plane2Amount, TextureParams.PlaneBShare);

			plane2Amount = TerrainTextureTypes.ApplySharpness(plane2Amount, 9.0f);

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

					float rockAmount = TerrainTextureTypes.ApplySharpness(steepness, TextureParams.RockBlendingSharpness);

					float blend12 = TextureParams.TextureRock.GetBlendValue12(TerrainSeed + 10, x, z);

					int splatIndexRock0 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.Rock, false);
					int splatIndexRock1 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.Rock, true);

					textureMaps[z, x, splatIndexRock0]	= rockAmount * (1.0f - blend12);
					textureMaps[z, x, splatIndexRock1]	= rockAmount * blend12;

					float planeAmount = 1.0f - rockAmount;

					float planeAAmount = planeAmount * GetAmountPlaneA(x, z);
					float planeBAmount = planeAmount - planeAAmount;

					blend12 = TextureParams.TexturePlaneA.GetBlendValue12(TerrainSeed + 20, x, z);
					int splatIndexPlaneA0 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.PlaneA, false);
					int splatIndexPlaneA1 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.PlaneA, true);
					textureMaps[z, x, splatIndexPlaneA0]	= planeAAmount * (1.0f - blend12);
					textureMaps[z, x, splatIndexPlaneA1]	= planeAAmount * blend12;

					blend12 = TextureParams.TexturePlaneB.GetBlendValue12(TerrainSeed + 20, x, z);
					int splatIndexPlaneB0 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.PlaneB, false);
					int splatIndexPlaneB1 = TerrainTextureTypes.TexturePairToSplatIndex(TerrainTexturePairs.PlaneB, true);
					textureMaps[z, x, splatIndexPlaneB0]	= planeBAmount * (1.0f - blend12);
					textureMaps[z, x, splatIndexPlaneB1]	= planeBAmount * blend12;

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

		// -----------------------------------------------------------------

		public void DebugDraw()
		{
			
		}

	} //< end terrain generator
}