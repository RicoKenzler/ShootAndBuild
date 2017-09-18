using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	public enum RegionType
	{
		Water,
		Beach,
		Inland,

		Count,
	}

	public struct Triangle
	{
		Vector2 P0;
		Vector2 P1;
		Vector2 P2;

		public Triangle(Vector2 p0, Vector2 p1, Vector2 p2)
		{
			P0 = p0;
			P1 = p1;
			P2 = p2;
		}

		public Rect CalculateAABB()
		{
			Rect aabb = new Rect();
			aabb.xMin = Mathf.Min(P0.x, P1.x, P2.x);
			aabb.yMin = Mathf.Min(P0.y, P1.y, P2.y);
			aabb.xMax = Mathf.Max(P0.x, P1.x, P2.x);
			aabb.yMax = Mathf.Max(P0.y, P1.y, P2.y);

			return aabb;
		}

		public bool TryGetBarycentricCoordinates(Vector2 referencePoint, out float t0, out float t1, out float t2)
		{
			Vector2 v0	= P1 - P0;
			Vector2 v1	= P2 - P0;
			Vector2 v2	= referencePoint - P0;

			float rDenominator = 1.0f / (v0.x * v1.y - v1.x * v0.y);
			t1 = (v2.x * v1.y - v1.x * v2.y) * rDenominator;
			t2 = (v0.x * v2.y - v2.x * v0.y) * rDenominator;
			t0 = 1.0f - t1 - t2;

			// is inside?
			return (t1 >= 0.0f) && (t2 >= 0.0f) && (t0 >= 0.0f);
		}
	}

	public class RegionCell
	{
		public const float UNINITIALIZED_DISTANCE = float.MaxValue;

		public VoronoiCell	VoronoiCell;	
		public RegionType	RegionType;
		public float		NormalizedDistanceToWater = UNINITIALIZED_DISTANCE;

		public RegionCell(VoronoiCell cell, RegionType region)
		{
			VoronoiCell = cell;
			RegionType	= region;
		}
	}

	class RegionMapTypes
	{
		public static Color GetDebugColor(RegionType regionType)
		{
			switch(regionType)
			{
				case RegionType.Count:	return Color.gray;					
				case RegionType.Water:			return Color.blue;					
				case RegionType.Beach:			return Color.yellow;				
				case RegionType.Inland:			return new Color(0.6f, 0.6f, 0.0f);	
			}

			return Color.black;
		}
	}

	public class RegionMapTransformation
	{
		public Vector2 CoordsMin;
		public Vector2 CoordsMax;
		public Vector2 Dimensions;
		public int Resolution;
			
		public Vector2 CellSize;

		public RegionMapTransformation(Vector2 mapCenter, Vector2 mapSize, int resolution)
		{
			CoordsMin = mapCenter - mapSize * 0.5f;
			CoordsMax = mapCenter + mapSize * 0.5f;
			Dimensions = CoordsMax - CoordsMin;
			Resolution = resolution;

			CellSize = Dimensions / (float) Resolution;
		}

		public float GetNormalizedDistance(float distance)
		{
			return distance / (0.5f * (Dimensions.x + Dimensions.y));
		}

		public float NormalizedDistanceToWS(float distance)
		{
			return distance * (0.5f * (Dimensions.x + Dimensions.y));
		}

		public Vector2 NormalizedCoordinateToWS(Vector2 normalizedCoordinate)
		{
			Vector2 posWS = normalizedCoordinate;
			posWS.x *= Dimensions.x;
			posWS.y *= Dimensions.y;
			posWS += CoordsMin;

			return posWS;
		}

		public Vector2 GetTileMin(int x, int z)
		{
			return CoordsMin + new Vector2(x * CellSize.x, z * CellSize.y);
		}

		public Vector2 GetTileCenter(int x, int z)
		{
			return CoordsMin + new Vector2((x + 0.5f) * CellSize.x, (z + 0.5f) * CellSize.y);
		}

		public void GetIndexRect(Rect aabbWS, out int minX, out int minZ, out int maxXExcluded, out int maxZExcluded)
		{
			Debug.Assert(aabbWS.size.x > 0 && aabbWS.size.y > 0);

			const float eps = 0.0001f;

			minX			= (int) ((aabbWS.xMin - CoordsMin.x - eps) / CellSize.x);
			minZ			= (int) ((aabbWS.yMin - CoordsMin.y - eps) / CellSize.y);
			maxXExcluded	= (int) ((aabbWS.xMax - CoordsMin.x + eps) / CellSize.x) + 1;
			maxZExcluded	= (int) ((aabbWS.yMax - CoordsMin.y + eps) / CellSize.y) + 1;

			minX			= Mathf.Max(minX,			0);
			minZ			= Mathf.Max(minZ,			0);
			maxXExcluded	= Mathf.Min(maxXExcluded,	Resolution);
			maxZExcluded	= Mathf.Min(maxZExcluded,	Resolution);
		}
	}
}