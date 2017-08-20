using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public enum RegionType
	{
		Uninitialized,

		Water,
		Beach,
		Inland,
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
			RegionType = region;
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
}