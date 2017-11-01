using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SAB.Terrain;
using UnityEngine.Serialization;

namespace SAB
{
	public class TerrainGenerationManager : MonoBehaviour
	{
		[SerializeField] private int					m_Resolution = 65;

		[SerializeField] private bool					m_UseTimeAsSeed = false;

		[SerializeField] private int					m_TerrainSeed = 22400;
		[SerializeField] private int					m_VoronoiSeed = 22400;
		[SerializeField] private int					m_RegionSeed  = 22400;

		[FormerlySerializedAs("TransformParams")]
		[SerializeField] private TransformParameters	m_TransformParams;

		[FormerlySerializedAs("RegionDescParams")]
		[SerializeField] private RegionDescParameters	m_RegionDescParams;

		[FormerlySerializedAs("WaterParams")]
		[SerializeField] private WaterParameters		m_WaterParams;

		[FormerlySerializedAs("VoronoiParams")]
		[SerializeField] private VoronoiParameters		m_VoronoiParams;

		[FormerlySerializedAs("RegionParams")]
		[SerializeField] private RegionParameters		m_RegionParams;

		private GeneratedTerrain	m_TerrainObject;
		private VoronoiCreator		m_VoronoiGenerator		= new VoronoiCreator();
		private RegionMapGenerator	m_RegionGenerator		= new RegionMapGenerator();
		private RegionGridGenerator m_RegionGridGenerator	= new RegionGridGenerator();
		private TerrainGenerator	m_TerrainGenerator		= new TerrainGenerator();

		///////////////////////////////////////////////////////////////////////////

		public void DeleteTerrain()
		{
			if (!m_TerrainObject)
			{
				GeneratedTerrain childTerrain = GetComponentInChildren<GeneratedTerrain>();
				m_TerrainObject = childTerrain ? childTerrain : null;
			}

			if (m_TerrainObject)
			{
				HelperMethods.DestroyOrDestroyImmediate(m_TerrainObject.GetComponent<UnityEngine.Terrain>());
				HelperMethods.DestroyOrDestroyImmediate(m_TerrainObject.GetComponent<UnityEngine.TerrainCollider>());
				HelperMethods.DestroyOrDestroyImmediate(m_TerrainObject.gameObject);
			}
		}
		
		///////////////////////////////////////////////////////////////////////////

		public void OnDrawGizmosSelected()
		{
			m_VoronoiGenerator.DebugDraw(m_VoronoiParams);
			m_RegionGenerator.DebugDraw();
			m_RegionGridGenerator.DebugDraw();
			m_TerrainGenerator.DebugDraw();
		}

		///////////////////////////////////////////////////////////////////////////

		public void RegenerateAll()
		{
			if (m_UseTimeAsSeed)
			{
				int timeSeed = (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000) % 100000;
				m_TerrainSeed = timeSeed;
				m_VoronoiSeed = timeSeed;
				m_RegionSeed  = timeSeed;
			}

			List<VoronoiCell> voronoiCells = m_VoronoiGenerator.GenerateVoronoi(m_VoronoiSeed, m_VoronoiParams, m_TransformParams.TerrainSizeWS);

			if (voronoiCells == null)
			{
				return;
			}

			RegionMapTransformation regionMapTransformation = new RegionMapTransformation(m_TransformParams.TerrainSizeWS, m_Resolution);
			m_RegionGenerator.GenerateRegions(m_RegionSeed, voronoiCells, m_RegionParams, regionMapTransformation);

			m_RegionGridGenerator.GenerateRegionGrid(m_RegionGenerator.regionMap, regionMapTransformation, m_RegionParams);

			DeleteTerrain();

			m_TerrainObject = m_TerrainGenerator.GenerateTerrain(m_RegionGenerator.regionMap, m_RegionGridGenerator.regionGrid, regionMapTransformation, m_TransformParams, m_RegionGridGenerator.heightRangeY, m_RegionDescParams, m_WaterParams, m_Resolution, m_TerrainSeed);

			if (m_TerrainObject)
			{
				m_TerrainObject.transform.parent = this.transform;
			}

			AmbientSoundGrid soundGridObject = AmbientSoundGrid.CreateEmptyObject(m_TerrainObject);
			soundGridObject.GenerateAmbientGrid(m_RegionGridGenerator.regionGrid, m_RegionGenerator.regionMap, m_TransformParams.TerrainSizeWS);
		}
	}
}