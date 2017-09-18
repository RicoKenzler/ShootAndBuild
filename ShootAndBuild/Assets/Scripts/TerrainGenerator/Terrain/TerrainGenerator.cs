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
		public RegionDescParameters			RegionDescParams;
		public WaterParameters				WaterParams;
		RegionMapTransformation				RegionTransformation;
		List<RegionCell>					RegionMap;
		RegionTile[,]						RegionGrid;

		public float MinY;
		public float MaxY;
		
		public GameObject GenerateTerrain(List<RegionCell> regionMap, RegionTile[,] regionGrid, RegionMapTransformation regionMapTransformation, TransformParameters transformParams, Vector2 cellHeightRangeY, RegionDescParameters regionDescParams, WaterParameters waterParams, int resolution, int terrainSeed)
		{
			// 0) Init Members
			TerrainObject			= null;
			Resolution				= resolution;
			TerrainSeed				= terrainSeed;
			TransformParams			= transformParams;
			RegionDescParams		= regionDescParams;
			WaterParams				= waterParams;
			RegionTransformation	= regionMapTransformation;
			RegionMap				= regionMap;
			RegionGrid				= regionGrid;


			if (RegionDescParams.RegionDescs.Length != (int) RegionType.Count)
			{
				Debug.LogWarning("You did not specify a RegionDesc for every RegionType " + "(" + RegionDescParams.RegionDescs.Length + " / " + RegionType.Count + ")");
				return null;
			} 

			float maxAdditionalRandomHeight = 0.0f;

			for (int r = 0; r < (int) RegionType.Count; ++r)
			{
				maxAdditionalRandomHeight = Mathf.Max(RegionDescParams.RegionDescs[r].HeightDesc.MaxAdditionalRandomAbsHeight, maxAdditionalRandomHeight);
			}

			MinY = RegionTransformation.CoordsMin.x; //< Hack: get rid of warning without tossing for-future-use-member

			MinY = cellHeightRangeY.x;
			MaxY = cellHeightRangeY.y + maxAdditionalRandomHeight;

			if (MinY == MaxY)
			{
				MaxY = MinY + 0.001f;
			}

			// 1) Init TerrainData
			TerrainData terrainData = new TerrainData();

			//Set heights
			terrainData.heightmapResolution = Resolution; 
			terrainData.alphamapResolution	= Resolution;

			if (Resolution != terrainData.heightmapResolution)
			{
				Debug.LogWarning("Resolution " + Resolution + " is not supported. Unity suggests: " + terrainData.heightmapResolution);
				return null;
			}

			terrainData.SetHeights(0, 0, GenerateHeightMap());
			terrainData.size = new Vector3(
				TransformParams.TerrainSizeWS.x, 
				cellHeightRangeY.y - cellHeightRangeY.x, 
				TransformParams.TerrainSizeWS.y);

			//Set textures
			SplatPrototype[] splatPrototypes = new SplatPrototype[(int)RegionType.Count * 2];

			for (int r = 0; r < (int) RegionType.Count; ++r)
			{
				splatPrototypes[TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) r, false)]		= RegionDescParams.RegionDescs[r].TextureDesc.CreateSplatPrototype(false);
				splatPrototypes[TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) r, true)]		= RegionDescParams.RegionDescs[r].TextureDesc.CreateSplatPrototype(true);
			}

			terrainData.splatPrototypes = splatPrototypes;
		
			terrainData.RefreshPrototypes();
			terrainData.SetAlphamaps(0, 0, GenerateTextureMaps(terrainData));

			//Create terrain
			if ((MaxY - MinY) <= 0)
			{
				Debug.LogError("Min/Max heights not correctly set (Max has to be > Min, Collider can only be created by unity when HeightMax != HeightMin)");
				return null;
			}

			TerrainObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);

			TerrainObject.transform.position = new Vector3(
				TransformParams.TerrainCenter.x - TransformParams.TerrainSizeWS.x * 0.5f, 
				MinY, 
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
					float height = GetNormalizedHeight(x, z);
					Heightmap[x, z] = height;

					maxHeight = Mathf.Max(maxHeight, height);
					minHeight = Mathf.Min(minHeight, height);
				}
			}

			Debug.Assert(minHeight >= 0.0f && maxHeight <= 1.0f);

			return Heightmap;
		}

		// -----------------------------------------------------------------

		public float[,,] GenerateTextureMaps(TerrainData TerrainData)
		{
			float[,,] textureMaps = new float[Resolution, Resolution, (int) RegionType.Count * 2];

			for (int x = 0; x < Resolution; x++)
			{
				for (int z = 0; z < Resolution; z++)
				{
					RegionTile curTile		 = RegionGrid[x,z];
					RegionCell curCell		 = RegionMap[curTile.Cell];
				
					float weightSum = 0.0f;

					for (int region = 0; region < curTile.RegionAmounts.Count; ++region)
					{
						float curAmount = curTile.RegionAmounts[region];

						if (curAmount == 0.0f)
						{
							continue;
						}

						RegionDesc curRegionDesc = RegionDescParams.RegionDescs[region];
						RegionTextureDesc curTextureDesc = curRegionDesc.TextureDesc;

						float blend12 = curTextureDesc.GetBlendValue12(TerrainSeed, x, z);
						int splatIndex1 = TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) region, false);
						int splatIndex2 = TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) region, true);

						Debug.Assert(textureMaps[x, z, splatIndex1] == 0.0f && textureMaps[x, z, splatIndex2] == 0.0f);

						textureMaps[x, z, splatIndex1] = curAmount * (1.0f - blend12);
						textureMaps[x, z, splatIndex2] = curAmount * blend12;

						weightSum += curAmount;
					}

					Debug.Assert(Mathf.Abs(weightSum - 1.0f) < 0.01f);
				}
			}
			return textureMaps;
		}

		// -----------------------------------------------------------------

		public float GetNormalizedHeight(int x, int z)
		{
			RegionTile regionTile = RegionGrid[x,z];
			RegionCell regionCell = RegionMap[regionTile.Cell];

			float baseHeight = regionTile.Height;

			RegionDesc regionDesc = RegionDescParams.RegionDescs[(int) regionCell.RegionType];

			float totalHeightAbs = baseHeight + regionDesc.HeightDesc.GenerateRandomAdditionalHeightAbs(TerrainSeed, x, z);

			float totalHeightRel = (totalHeightAbs - MinY) / (MaxY - MinY);

			return totalHeightRel;
		}

		// -----------------------------------------------------------------

		public void DebugDraw()
		{
			
		}

	} //< end terrain generator
}