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
		
		public void OnDrawGizmosSelected()
		{
			for (int x = 0; x < terrainSizeWS.x; ++x)
			{
				for (int z = 0; z < terrainSizeWS.y; ++z)
				{
					Vector2 quadCenter = new Vector2(x + 0.5f, z + 0.5f);
					float height = GetInterpolatedHeight(quadCenter.x, quadCenter.y);

					Vector3 quadMin = new Vector3(x, height, z);
					Vector3 quadMax = new Vector3(x + 1, height, z + 1);

					float waterHeight		= 0.0f;
					float waterDifference	= height - waterHeight;
					float lerpValue			= 0.0f;

					if (waterDifference > 0)
					{
						lerpValue = Mathf.Clamp01(waterDifference / 5.0f);
						lerpValue = lerpValue * 0.5f + 0.5f;
					}
					else
					{
						lerpValue = 1.0f - Mathf.Clamp01(-waterDifference / 10.0f);
						lerpValue = lerpValue * 0.5f;
					}
					
					Color heightColor = Color.Lerp(new Color(0,0,1), new Color(0,1,0), lerpValue);
					DebugHelper.BufferQuad(quadMin, quadMax, heightColor);
				}

				DebugHelper.DrawBufferedTriangles();
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}
}