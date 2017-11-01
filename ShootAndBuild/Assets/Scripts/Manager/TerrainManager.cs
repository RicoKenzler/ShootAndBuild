using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class TerrainManager : MonoBehaviour 
	{
		[SerializeField] private UnityEngine.Terrain				m_Terrain;
						 private Terrain.RegionTile[,]				m_RegionGrid;
		[SerializeField] private Terrain.RegionMapTransformation	m_RegionMapTransformation;
		[SerializeField] private Vector2							m_TerrainSizeWS;

		///////////////////////////////////////////////////////////////////////////

		public UnityEngine.Terrain				terrain					{ get { return m_Terrain; } }
		public Terrain.RegionTile[,]			regionGrid				{ get { return m_RegionGrid; } }
		public Terrain.RegionMapTransformation	regionMapTransformation	{ get { return m_RegionMapTransformation; }}
		public Vector2							terrainSizeWS			{ get { return m_TerrainSizeWS; }}

		public static TerrainManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		public void ReplaceTerrain(UnityEngine.Terrain newTerrain, Terrain.RegionTile[,] regionGrid, Vector2 terrainSizeWS)
		{
			m_Terrain			= newTerrain;
			m_RegionGrid		= regionGrid;
			m_TerrainSizeWS		= terrainSizeWS;

			int regionGridResolution = (regionGrid == null) ? 1 : regionGrid.GetLength(0);
			m_RegionMapTransformation = new SAB.Terrain.RegionMapTransformation(terrainSizeWS, regionGridResolution);
		}

		///////////////////////////////////////////////////////////////////////////

		public Vector2 GetTerrainCenter2D()
		{
			return m_TerrainSizeWS * 0.5f;
		}

		///////////////////////////////////////////////////////////////////////////

		public Vector3 GetTerrainCenter3D()
		{
			Vector2 terrainCenter2D = GetTerrainCenter2D();
			return new Vector3(terrainCenter2D.x, GetInterpolatedHeight(terrainCenter2D.x, terrainCenter2D.y), terrainCenter2D.y);
		}

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this; 
		}

		///////////////////////////////////////////////////////////////////////////

		void Start () 
		{
			Debug.Assert(m_Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
		}
		
		///////////////////////////////////////////////////////////////////////////

		public float GetInterpolatedHeight(float xWS, float zWS)
		{
			if (m_Terrain == null)
			{
				Debug.Assert(m_Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
				return 0.0f;
			}

			float height = m_Terrain.SampleHeight(new Vector3(xWS, 0.5f, zWS));
			height += m_Terrain.gameObject.transform.position.y;

			return height;
		}

		///////////////////////////////////////////////////////////////////////////

		public Terrain.RegionTile GetRegionAt(float xWS, float zWS)
		{
			return new SAB.Terrain.RegionTile();
		}
	}
}