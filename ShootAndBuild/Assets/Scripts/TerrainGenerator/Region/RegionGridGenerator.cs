using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using CellIndex = System.Int32;

namespace SAB.Terrain
{
	public class RegionTile
	{
		public List<float> Amounts	= new List<float>((int)RegionType.Count);
		public CellIndex Cell		= -1;

		public RegionTile()
		{
			for (int t = 0; t < (int) RegionType.Count; ++t)
			{
				Amounts.Add(0.0f);
            }

			Amounts[(int) RegionType.Uninitialized] = 1.0f;
		}

		public Color GetDebugColor()
		{
			Color col = new Color(0,0,0, 1);

			for (int t = 0; t < (int) RegionType.Count; ++t)
			{
				float amount = Amounts[t];
				Color typeCol = RegionMapTypes.GetDebugColor((RegionType)t);

				col.r += amount*typeCol.r;
				col.g += amount*typeCol.g;
				col.b += amount*typeCol.b;
			}		

			return col;
		}
	}

	// ------------------------------------

	public class RegionGridGenerator
	{
		public RegionTile[,] RegionGrid;
		List<RegionCell> RegionCells = null;

		RegionParameters RegionParams;
		RegionMapTransformation MapTransformation;

		public void GenerateRegionGrid(List<RegionCell> regionCells, RegionMapTransformation mapTransformation, RegionParameters regionParameters)
		{
			// 0) Init
			RegionCells			= regionCells;
			RegionParams		= regionParameters;
			MapTransformation	= mapTransformation;

			RegionGrid = new RegionTile[mapTransformation.Resolution, mapTransformation.Resolution];

			for (int iy = 0; iy < RegionGrid.GetLength(1); iy++)
			{
				for (int ix = 0; ix < RegionGrid.GetLength(0); ix++)
				{
					RegionGrid[ix, iy] = new RegionTile();
				}
			}

			// 1) Assign Cells
			AssignCellsToTiles();
			AssignAmountsToTiles();

			// 1) Check consistency
			for (int iy = 0; iy < RegionGrid.GetLength(1); iy++)
			{
				for (int ix = 0; ix < RegionGrid.GetLength(0); ix++)
				{
					float amountSum = 0.0f;
					for (int t = 0; t < (int) RegionType.Count; ++t)
					{
						RegionTile tile = RegionGrid[ix, iy];
						amountSum += tile.Amounts[t];
					}

					if (amountSum < 0.99f || amountSum > 1.01f)
					{
						Debug.Assert(false, "Inconsistent Region Grid");
						break;
					}
				}
			}
		}

		// -----------------------------------------------------------

		public void AssignCellsToTiles()
		{
			for (CellIndex c = 0; c < RegionCells.Count; ++c)
            {
				RegionCell cell = RegionCells[c];
				Rect aabb = cell.VoronoiCell.CalculateAABB();

				int minX, minZ, maxX, maxZ;
				MapTransformation.GetIndexRect(aabb, out minX, out minZ, out maxX, out maxZ);

				for (int iZ = minZ; iZ < maxZ; ++iZ)
				{
					for (int iX = minX; iX < maxX; ++iX)
					{
						RegionTile curTile = RegionGrid[iX, iZ];

						if (curTile.Cell != -1)
						{
							continue;
						}

						Vector2 tileCenter = MapTransformation.GetTileCenter(iX, iZ);

						if (cell.VoronoiCell.IsInside(tileCenter))
						{
							curTile.Cell = c;
						}
					}
				}
			}
		}

		// -----------------------------------------------------------

		public void AssignAmountsToTiles()
		{
			for (int iY = 0; iY < RegionGrid.GetLength(1); iY++)
			{
				for (int iX = 0; iX < RegionGrid.GetLength(0); iX++)
				{
					RegionTile curTile = RegionGrid[iX, iY];

					if (curTile.Cell == -1)
					{
						Debug.Assert(false);
						curTile.Cell = 0;
					}

					RegionCell curCell = RegionCells[curTile.Cell];

					curTile.Amounts[(int) RegionType.Uninitialized] = 0.0f;
					curTile.Amounts[(int) curCell.RegionType] = 1.0f;
				}
			}
		}

		// -----------------------------------------------------------

		public void DebugDraw()
		{
			if (RegionGrid == null)
			{
				return;
			}

			const int MAX_ALLOWED_INDICES = 40;
			int maxIndexX = Mathf.Min(RegionGrid.GetLength(0), MAX_ALLOWED_INDICES);
			int maxIndexZ = Mathf.Min(RegionGrid.GetLength(1), MAX_ALLOWED_INDICES);

			if (RegionParams.ShowRegionGrid)
			{
				float debugDrawHeight = 1.0f;

				for (int iZ = 0; iZ < maxIndexZ; iZ++)
				{
					for (int iX = 0; iX < maxIndexX; iX++)
					{
						Vector2 cellMin = MapTransformation.GetTileMin(iX, iZ);
						Vector2 cellMax = MapTransformation.GetTileMin(iX + 1, iZ + 1);
					
						Vector2 cellMinOffsetted = Vector2.Lerp(cellMin, cellMax, 0.05f);
						Vector2 cellMaxOffsetted = Vector2.Lerp(cellMin, cellMax, 0.95f);

						RegionTile tile = RegionGrid[iX, iZ];

						Color col = tile.GetDebugColor();			

						DebugHelper.BufferQuad(new Vector3(cellMinOffsetted.x, debugDrawHeight, cellMinOffsetted.y), new Vector3(cellMaxOffsetted.x, debugDrawHeight, cellMaxOffsetted.y), col);

						/*if (RegionParams.ShowIndices)
						{
							Vector2 cellCenter = (cellMin + cellMax) * 0.5f;
							DebugHelper.DrawText(cellCenter, debugDrawHeight + 4.0f, Color.white, iX + "|" + iZ);
						}*/
					}
				}

				DebugHelper.DrawBufferedTriangles();
			}

			if (RegionParams.ShowRegionGrid && RegionParams.ShowIndices)
			{
				float debugDrawHeight = 1.0f;

				for (int iZ = 0; iZ < maxIndexZ; iZ++)
				{
					for (int iX = 0; iX < maxIndexX; iX++)
					{
						Vector2 cellMin = MapTransformation.GetTileMin(iX, iZ);
						Vector2 cellMax = MapTransformation.GetTileMin(iX + 1, iZ + 1);
					
						Vector2 cellCenter = (cellMin + cellMax) * 0.5f;
						DebugHelper.DrawText(cellCenter, debugDrawHeight, Color.white, iX + "|" + iZ);
					}
				}
			}
		} //< end debug draw
	
	} //< end region grid generator


}