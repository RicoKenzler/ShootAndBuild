using UnityEngine;

namespace SAB
{
	public class TerrainManager : MonoBehaviour 
	{
		[SerializeField] private Terrain.RegionMapTransformation	m_RegionMapTransformation;

		private UnityEngine.Terrain				m_Terrain;
		private Terrain.GeneratedTerrain		m_GeneratedTerrain;
		private Vector2							m_TerrainSizeWS;

		///////////////////////////////////////////////////////////////////////////

		public UnityEngine.Terrain				terrain					{ get { return m_Terrain; } }
		public Vector2							terrainSizeWS			{ get { return m_TerrainSizeWS; }}

		public static TerrainManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		public Vector2 GetTerrainCenter2D()
		{
			return m_TerrainSizeWS * 0.5f;
		}

		///////////////////////////////////////////////////////////////////////////

		public Vector3 GetTerrainCenter3D()
		{
			Vector2 terrainCenter2D = GetTerrainCenter2D();
			return new Vector3(terrainCenter2D.x, m_GeneratedTerrain.GetInterpolatedHeight(terrainCenter2D.x, terrainCenter2D.y), terrainCenter2D.y);
		}

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this; 

			m_GeneratedTerrain = Terrain.GeneratedTerrain.FindInScene();

			m_Terrain			= m_GeneratedTerrain.GetComponent<UnityEngine.Terrain>();
			m_TerrainSizeWS		= m_GeneratedTerrain.sizeWS;
		}

		///////////////////////////////////////////////////////////////////////////

		void Start() 
		{
			Debug.Assert(m_GeneratedTerrain != null && m_Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetInterpolatedHeight(float xWS, float zWS)
		{
			return m_GeneratedTerrain.GetInterpolatedHeight(xWS, zWS);
		}

		///////////////////////////////////////////////////////////////////////////
	}
}