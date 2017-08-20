using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using CellIndex = System.Int32;

namespace SAB
{
	public class RegionMapGenerator 
	{
		List<RegionCell> RegionMap		= new List<RegionCell>();
		RegionParameters RegionParams	= new RegionParameters();

		public void GenerateRegions(List<VoronoiCell> voronoiCells, RegionParameters regionParams)
		{
			// 0) Init
			RegionParams = regionParams;
			RegionMap.Clear();
			RegionMap.Capacity = voronoiCells.Count;

			for (CellIndex c = 0; c < voronoiCells.Count; ++c)
			{
				RegionMap.Add(new RegionCell(voronoiCells[c], RegionType.Uninitialized));
			}

			// 1) Make outer cells Water
			for (CellIndex c = 0; c < RegionMap.Count; ++c)
			{
				RegionCell cell = RegionMap[c];
				
				bool isOuterCell = false;

				for (int n = 0; n < cell.VoronoiCell.NeighborCells.Count; ++n)
				{
					if (cell.VoronoiCell.NeighborCells[n].WasClamped)
					{
						isOuterCell = true;
						break;
					}
				}

				if (isOuterCell)
				{
					cell.RegionType = RegionType.Water;
				}
				else
				{
					cell.RegionType = RegionType.Inland;
				}
			}

		}

		public void DebugDraw()
		{
			float debugDrawHeight = 1.0f;

			if (RegionParams.ShowRegions)
			{
				for (CellIndex c = 0; c < RegionMap.Count; ++c)
				{
					VoronoiCell currentCell = RegionMap[c].VoronoiCell;
					RegionType  regionType	= RegionMap[c].RegionType;

					Color cellColor = Color.gray;

					switch(regionType)
					{
						case RegionType.Uninitialized:	cellColor = Color.gray;						break;
						case RegionType.Water:			cellColor = Color.blue;						break;
						case RegionType.Inland:			cellColor = new Color(0.6f, 0.6f, 0.0f);	break;
					}

					Vector3 centroid3D = new Vector3(currentCell.Centroid.x, debugDrawHeight, currentCell.Centroid.y);

					for (int p = 0; p < currentCell.NeighborCells.Count; ++p)
					{
						VoronoiNeighbor neighbor = currentCell.NeighborCells[p];

						Vector2 start	= neighbor.EdgeToNeighbor.Start;
						Vector2 end		= neighbor.EdgeToNeighbor.End;

						Vector3 start3D = new Vector3(start.x,	debugDrawHeight, start.y);
						Vector3 end3D   = new Vector3(end.x,	debugDrawHeight, end.y);

						Gizmos.color = Color.black;
						Gizmos.DrawLine(start3D, end3D);

						DebugHelper.BufferTriangle(start3D, centroid3D, end3D, cellColor);
					}

					if (RegionParams.ShowIndices)
					{
						DebugHelper.DrawText(currentCell.Centroid, debugDrawHeight, Color.black, c.ToString());
					}	
				} //< for all cells

				DebugHelper.DrawBufferedTriangles();
			}
		}
	}
}