using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class TerrainManager : MonoBehaviour 
	{
		[Header("Auto Filled on Generation")]
		public UnityEngine.Terrain		Terrain;
		public Terrain.RegionTile[,]	RegionGrid;

		public Vector2					TerrainSizeWS;

		public void ReplaceTerrain(UnityEngine.Terrain newTerrain, Terrain.RegionTile[,] regionGrid, Vector2 terrainSizeWS)
		{
			Terrain			= newTerrain;
			RegionGrid		= regionGrid;
			TerrainSizeWS	= terrainSizeWS;
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

			return 0.0f;

			// TODO: swizzle X<->Z
			// return Terrain.SampleHeight(new Vector3(xWS, 0.0f, zWS));
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