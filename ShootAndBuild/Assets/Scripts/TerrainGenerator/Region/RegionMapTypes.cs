using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	public enum RegionType
	{
		Uninitialized,

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
			bool isInside1 = false;
			bool isInside2 = false;

			float t0_1, t0_2, t1_1, t1_2, t2_1, t2_2;

			{
				Vector2 v0 = P2 - P0;
				Vector2 v1 = P1 - P0;
				Vector2 v2 = referencePoint - P0;

				float dot00 = Vector2.Dot(v0, v0);
				float dot01 = Vector2.Dot(v0, v1);
				float dot02 = Vector2.Dot(v0, v2);
				float dot11 = Vector2.Dot(v1, v1);
				float dot12 = Vector2.Dot(v1, v2);

				float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
				t0_1 = (dot11 * dot02 - dot01 * dot12) * invDenom;
				t1_1 = (dot00 * dot12 - dot01 * dot02) * invDenom;
				t2_1 = 1.0f - t0_1 - t1_1;
				 
				isInside1 = (t0_1 >= 0) && (t1_1 >= 0) && (t2_1 >= 0);
			}
			{
				Vector2 v0	= P1 - P0;
				Vector2 v1	= P2 - P0;
				Vector2 v2	= referencePoint - P0;

				float rDenominator = 1.0f / (v0.x * v1.y - v1.x * v0.y);
				t0_2 = (v2.x * v1.y - v1.x * v2.y) * rDenominator;
				t1_2 = (v0.x * v2.y - v2.x * v0.y) * rDenominator;
				t2_2 = 1.0f - t0_2 - t1_2;

				isInside2 = (t0_2 >= 0.0f) && (t0_2 <= 1.0f) && (t1_2 >= 0.0f) && (t1_2 <= 1.0f) && (t2_2 >= 0.0f) && (t2_2 <= 1.0f);
			}

			t0 = t0_1;
			t1 = t1_2;
			t2 = t2_2;

			return isInside1;
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
				case RegionType.Uninitialized:	return Color.gray;					
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