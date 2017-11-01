using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	///////////////////////////////////////////////////////////////////////////

	public class TerrainGenerator
	{
		private GameObject	m_TerrainObject;
		private int			m_Resolution	= 1;
		private int			m_TerrainSeed	= 0;
		
		private TransformParameters			m_TransformParams;
		private RegionDescParameters		m_RegionDescParams;
		private WaterParameters				m_WaterParams;
		private RegionMapTransformation		m_RegionTransformation;
		private List<RegionCell>			m_RegionMap;
		private RegionGrid					m_RegionGrid;

		private float MinY;
		private float MaxY;
		
		public GeneratedTerrain GenerateTerrain(List<RegionCell> regionMap, RegionGrid regionGrid, RegionMapTransformation regionMapTransformation, TransformParameters transformParams, Vector2 cellHeightRangeY, RegionDescParameters regionDescParams, WaterParameters waterParams, int resolution, int terrainSeed)
		{
			// 0) Init Members
			m_TerrainObject			= null;
			m_Resolution			= resolution;
			m_TerrainSeed			= terrainSeed;
			m_TransformParams		= transformParams;
			m_RegionDescParams		= regionDescParams;
			m_WaterParams			= waterParams;
			m_RegionTransformation	= regionMapTransformation;
			m_RegionMap				= regionMap;
			m_RegionGrid			= regionGrid;

			if (m_RegionDescParams.RegionDescs.Length != (int) RegionType.Count)
			{
				Debug.LogWarning("You did not specify a RegionDesc for every RegionType " + "(" + m_RegionDescParams.RegionDescs.Length + " / " + RegionType.Count + ")");
				return null;
			} 

			float maxAdditionalRandomHeight = 0.0f;

			for (int r = 0; r < (int) RegionType.Count; ++r)
			{
				maxAdditionalRandomHeight = Mathf.Max(m_RegionDescParams.RegionDescs[r].HeightDesc.MaxAdditionalRandomAbsHeight, maxAdditionalRandomHeight);
			}

			MinY = m_RegionTransformation.CellSize.x; //< Hack: get rid of warning without tossing for-future-use-member

			MinY = cellHeightRangeY.x;
			MaxY = cellHeightRangeY.y + maxAdditionalRandomHeight;

			if (MinY == MaxY)
			{
				MaxY = MinY + 0.001f;
			}

			// 1) Init TerrainData
			TerrainData terrainData = new TerrainData();

			//Set heights
			terrainData.heightmapResolution = m_Resolution; 
			terrainData.alphamapResolution	= m_Resolution;

			if (m_Resolution != terrainData.heightmapResolution)
			{
				Debug.LogWarning("Resolution " + m_Resolution + " is not supported. Unity suggests: " + terrainData.heightmapResolution);
				return null;
			}

			terrainData.SetHeights(0, 0, GenerateHeightMap());
			terrainData.size = new Vector3(
				m_TransformParams.TerrainSizeWS.x, 
				MaxY - MinY, 
				m_TransformParams.TerrainSizeWS.y);

			//Set textures
			SplatPrototype[] splatPrototypes = new SplatPrototype[(int)RegionType.Count * 2];

			for (int r = 0; r < (int) RegionType.Count; ++r)
			{
				splatPrototypes[TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) r, false)]		= m_RegionDescParams.RegionDescs[r].TextureDesc.CreateSplatPrototype(false);
				splatPrototypes[TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) r, true)]		= m_RegionDescParams.RegionDescs[r].TextureDesc.CreateSplatPrototype(true);
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

			m_TerrainObject = UnityEngine.Terrain.CreateTerrainGameObject(terrainData);
			m_TerrainObject.name = "Terrain (Generated)";

			m_TerrainObject.transform.position = new Vector3(
				0.0f, 
				MinY, 
				0.0f);

			UnityEngine.Terrain terrainComponent = m_TerrainObject.GetComponent<UnityEngine.Terrain>();
			terrainComponent.Flush();

			UnityEngine.TerrainCollider terrainCollider = m_TerrainObject.GetComponent<UnityEngine.TerrainCollider>();
			terrainCollider.terrainData = terrainData; 

			// 2) Create Water Plane
			CreateWaterPlane(m_TerrainObject);

			// 3) Create GeneratedTerrainProperty
			GeneratedTerrain generatedTerrainComponent = m_TerrainObject.AddComponent<GeneratedTerrain>();
			m_TerrainObject.tag = GeneratedTerrain.GENERATED_TERRAIN_TAG;

			generatedTerrainComponent.sizeWS		= m_TransformParams.TerrainSizeWS;
			generatedTerrainComponent.terrain		= terrainComponent;
			generatedTerrainComponent.regionGrid	= regionGrid;
			
			return generatedTerrainComponent;
		}

		///////////////////////////////////////////////////////////////////////////

		public void CreateWaterPlane(GameObject terrainObject)
		{
			if (m_WaterParams.UseWater)
			{
				GameObject waterPlaneObject = MonoBehaviour.Instantiate(m_WaterParams.WaterPlanePrefab, terrainObject.transform);
				waterPlaneObject.transform.localScale	= new Vector3(m_TransformParams.TerrainSizeWS.x, 1.0f, m_TransformParams.TerrainSizeWS.y);
				waterPlaneObject.transform.position		= new Vector3(m_TransformParams.TerrainSizeWS.x * 0.5f, m_WaterParams.WaterHeight, m_TransformParams.TerrainSizeWS.y * 0.5f);		
				waterPlaneObject.name = "WaterPlane";
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public float[,] GenerateHeightMap()
		{
			float[,] Heightmap = new float[m_Resolution, m_Resolution];

			float maxHeight = 0.0f;
			float minHeight = 1.0f;

			for (int x = 0; x < m_Resolution; x++)
			{
				for (int z = 0; z < m_Resolution; z++)
				{
					float height = GetNormalizedHeight(x, z);

					// Terrain Generation interprets array[i,j] as array[x,z], UnityTerrain interprets it as [z, x]
					Heightmap[z, x] = height;

					maxHeight = Mathf.Max(maxHeight, height);
					minHeight = Mathf.Min(minHeight, height);
				}
			}

			Debug.Assert(minHeight >= 0.0f && maxHeight <= 1.0f);

			return Heightmap;
		}

		///////////////////////////////////////////////////////////////////////////

		public float[,,] GenerateTextureMaps(TerrainData TerrainData)
		{
			float[,,] textureMaps = new float[m_Resolution, m_Resolution, (int) RegionType.Count * 2];

			for (int x = 0; x < m_Resolution; x++)
			{
				for (int z = 0; z < m_Resolution; z++)
				{
					RegionTile curTile		 = m_RegionGrid.GetAt(x,z);
					RegionCell curCell		 = m_RegionMap[curTile.Cell];
				
					float weightSum = 0.0f;

					for (int region = 0; region < curTile.RegionAmounts.Count; ++region)
					{
						float curAmount = curTile.RegionAmounts[region];

						if (curAmount == 0.0f)
						{
							continue;
						}

						RegionDesc curRegionDesc = m_RegionDescParams.RegionDescs[region];
						RegionTextureDesc curTextureDesc = curRegionDesc.TextureDesc;

						float blend12 = curTextureDesc.GetBlendValue12(m_TerrainSeed, x, z);
						int splatIndex1 = TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) region, false);
						int splatIndex2 = TerrainTextureTypes.RegionTypeToSplatIndex((RegionType) region, true);

						Debug.Assert(textureMaps[z, x, splatIndex1] == 0.0f && textureMaps[z, x, splatIndex2] == 0.0f);

						// Terrain Generation interprets array[i,j] as array[x,z], UnityTerrain interprets it as [z, x]
						textureMaps[z, x, splatIndex1] = curAmount * (1.0f - blend12);
						textureMaps[z, x, splatIndex2] = curAmount * blend12;

						weightSum += curAmount;
					}

					Debug.Assert(Mathf.Abs(weightSum - 1.0f) < 0.01f);
				}
			}
			return textureMaps;
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetNormalizedHeight(int x, int z)
		{
			RegionTile regionTile = m_RegionGrid.GetAt(x,z);
			RegionCell regionCell = m_RegionMap[regionTile.Cell];

			float baseHeight = regionTile.Height;

			RegionDesc regionDesc = m_RegionDescParams.RegionDescs[(int) regionCell.RegionType];

			float totalHeightAbs = baseHeight + regionDesc.HeightDesc.GenerateRandomAdditionalHeightAbs(m_TerrainSeed, x, z);

			float totalHeightRel = (totalHeightAbs - MinY) / (MaxY - MinY);

			return totalHeightRel;
		}

		///////////////////////////////////////////////////////////////////////////

		public void DebugDraw()
		{
			
		}

	} //< end terrain generator
}