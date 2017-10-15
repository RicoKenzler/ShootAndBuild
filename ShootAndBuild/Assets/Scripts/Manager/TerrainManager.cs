using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class TerrainManager : MonoBehaviour 
	{
		[Header("Auto Filled on Generation")]
		public UnityEngine.Terrain				Terrain;
		public Terrain.RegionTile[,]			RegionGrid;
		public Terrain.RegionMapTransformation	RegionMapTransformation;

		public Vector2					TerrainSizeWS;
		public void ReplaceTerrain(UnityEngine.Terrain newTerrain, Terrain.RegionTile[,] regionGrid, Vector2 terrainSizeWS)
		{
			Terrain			= newTerrain;
			RegionGrid		= regionGrid;
			TerrainSizeWS	= terrainSizeWS;

			int regionGridResolution = regionGrid == null ? 1 : regionGrid.GetLength(0);
			RegionMapTransformation = new SAB.Terrain.RegionMapTransformation(terrainSizeWS, regionGridResolution);
		}

		public Vector2 GetTerrainCenter2D()
		{
			return TerrainSizeWS * 0.5f;
		}

		public Vector3 GetTerrainCenter3D()
		{
			Vector2 terrainCenter2D = GetTerrainCenter2D();
			return new Vector3(terrainCenter2D.x, GetInterpolatedHeight(terrainCenter2D.x, terrainCenter2D.y), terrainCenter2D.y);
		}

		void Awake()
		{
			Instance = this; 
		}

		void Start () 
		{
			Debug.Assert(Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
		}
		
		void Update () 
		{
			
		}

		public float GetInterpolatedHeight(float xWS, float zWS)
		{
			if (Terrain == null)
			{
				Debug.Assert(Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
				return 0.0f;
			}

			float height = Terrain.SampleHeight(new Vector3(xWS, 0.5f, zWS));
			height += Terrain.gameObject.transform.position.y;

			return height;
		}

		public Terrain.RegionTile GetRegionAt(float xWS, float zWS)
		{
			return new SAB.Terrain.RegionTile();
		}

		public static TerrainManager Instance
		{
			get; private set;
		}
	}
}