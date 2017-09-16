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
					RegionTile tile = RegionGrid[ix, iy];

					float amountSum = 0.0f;
					for (int t = 0; t < (int) RegionType.Count; ++t)
					{
						amountSum += tile.Amounts[t];
					}

					if (amountSum < 0.99f || amountSum > 1.01f)
					{
						Debug.Assert(false, "Inconsistent Region Grid");
						break;
					}

					Debug.Assert(tile.Amounts[(int) RegionType.Uninitialized] == 0.0f);
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
			// for all cells...
			for (CellIndex c = 0; c < RegionCells.Count; ++c)
            {
				RegionCell curCell = RegionCells[c];
				
				List<VoronoiNeighbor> allNeighbors = curCell.VoronoiCell.NeighborCellsCCW;

				// ... create triangles with consecutive neighbors
				for (int n = 0; n < allNeighbors.Count; ++n)
				{
					int nextNeighborIndex = (n+1) % allNeighbors.Count;
					int prevNeighborIndex = (n- 1 + allNeighbors.Count) % allNeighbors.Count;

					VoronoiNeighbor prevNeighbor		= allNeighbors[prevNeighborIndex];
					VoronoiNeighbor curNeighbor			= allNeighbors[n];
					VoronoiNeighbor nextNeighbor		= allNeighbors[nextNeighborIndex];

					// cur, neigh, nextneigh
					Triangle curTriangle = new Triangle(curCell.VoronoiCell.Centroid, curNeighbor.EdgeToNeighbor.Start, curNeighbor.EdgeToNeighbor.End);

					Rect aabb = curTriangle.CalculateAABB();

					int minX, minZ, maxX, maxZ;
					MapTransformation.GetIndexRect(aabb, out minX, out minZ, out maxX, out maxZ);

					// every cell within triangle
					for (int iZ = minZ; iZ < maxZ; ++iZ)
					{
						for (int iX = minX; iX < maxX; ++iX)
						{
							RegionTile curTile = RegionGrid[iX, iZ];

							Vector2 tileCenter = MapTransformation.GetTileCenter(iX, iZ);

							float bCur, bPrevNeigh, bNextNeigh;
							bool isInside = curTriangle.TryGetBarycentricCoordinates(tileCenter, out bCur, out bPrevNeigh, out bNextNeigh);
							
							if (!isInside)
							{
								continue;
							}

							// N1 = prevNeighbor
							// N  = curNeighbor
							// N2 = nextNeighbor
							// C  = curCell
							//
							//  N2 /
							//    n2    \
							//   / \  N  \
							//  / C \     \
							// /     n1___/
							// \    /    /
							//  \__/ N1 /
							// 
							// f(c)  = 100% c
							// f(n1) =  33% (C + N + N1)
							// f(n2) =  33% (C + N + N2)

							// f(x) = f(c) * b_c + f(n1) * b_n1 + f(n2) * b_n2
							// = b_c*c + b_n1*0.33*(C+N+N1) + b_n2*0.33*(C+N+N2)
							// = c*(b_c + b_n1*0.33 + b_n2*0.33) + N * 0.33 * (b_n1 + b_n2) + N1 * b_n1*0.33 + N2 * b_n2 * 0.33)
							// = 0.33 * (c*(3*b_c + b_n1 + b_n2) + N * (b_n1 + b_n2) + N1 * (b_n1) + N2 * (b_n2))
							// = c * (0.33 + 0.66*b_c) + N * 0.33 * (b_n1 + b_n2) + N1 * 0.33 * (b_n1) + N2 * 0.33 * (b_n2)

							RegionCell cell_N  = curNeighbor.WasClamped  ? curCell : RegionCells[curNeighbor.NeighborIndexIfValid];
							RegionCell cell_N1 = prevNeighbor.WasClamped ? curCell : RegionCells[prevNeighbor.NeighborIndexIfValid];
							RegionCell cell_N2 = nextNeighbor.WasClamped ? curCell : RegionCells[nextNeighbor.NeighborIndexIfValid];

							Debug.Assert(curTile.Amounts[(int) RegionType.Uninitialized] == 1.0f);
							curTile.Amounts[(int) RegionType.Uninitialized] = 0.0f;

							const float ONE_THIRD = (1.0f / 3.0f);

							// c * (0.33 + 0.66*b_c)
							curTile.Amounts[(int) curCell.RegionType]  += bCur * (2.0f * ONE_THIRD) + ONE_THIRD;

							// N * (0.33* (b_n1 + b_n2))
							curTile.Amounts[(int) cell_N.RegionType]   += ONE_THIRD * (bPrevNeigh + bNextNeigh);

							// N1 * (0.33 * b_n1)
							curTile.Amounts[(int) cell_N1.RegionType]  += ONE_THIRD * (bPrevNeigh);

							// N2 * (0.33 * b_n2)
							curTile.Amounts[(int) cell_N2.RegionType]  += ONE_THIRD * (bNextNeigh);
						}
					}

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