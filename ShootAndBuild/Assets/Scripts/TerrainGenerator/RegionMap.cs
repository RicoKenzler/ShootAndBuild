using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public enum RegionType
	{
		Uninitialized,

		Water,
		Inland,
	}

	public class RegionCell
	{
		public VoronoiCell VoronoiCell;	
		public RegionType  RegionType;

		public RegionCell(VoronoiCell cell, RegionType region)
		{
			VoronoiCell = cell;
			RegionType = region;
		}
	}
}