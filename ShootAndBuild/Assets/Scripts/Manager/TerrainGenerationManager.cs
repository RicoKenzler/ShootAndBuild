using SAB.Terrain;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
	public class TerrainGenerationManager : MonoBehaviour
	{
		[SerializeField] private int					m_Resolution = 65;

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

		public static TerrainGenerationManager instance { get; private set; }

		private GeneratedTerrain	m_TerrainObject;
		private VoronoiCreator		m_VoronoiGenerator		= new VoronoiCreator();
		private RegionMapGenerator	m_RegionGenerator		= new RegionMapGenerator();
		private RegionGridGenerator m_RegionGridGenerator	= new RegionGridGenerator();
		private TerrainGenerator	m_TerrainGenerator		= new TerrainGenerator();

		private int	m_TerrainSeed = 0;
		private int	m_VoronoiSeed = 0;
		private int	m_RegionSeed  = 0;

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
		}

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

		public void RegenerateAll(int seed)
		{
			m_TerrainSeed = seed;
			m_VoronoiSeed = seed;
			m_RegionSeed  = seed;

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
				m_TerrainObject.transform.parent = transform;
			}

			AmbientSoundGrid soundGridObject = AmbientSoundGrid.CreateEmptyObject(m_TerrainObject);
			soundGridObject.GenerateAmbientGrid(m_RegionGridGenerator.regionGrid, m_RegionGenerator.regionMap, m_TransformParams.TerrainSizeWS);
		}

		///////////////////////////////////////////////////////////////////////////
		
	}
}