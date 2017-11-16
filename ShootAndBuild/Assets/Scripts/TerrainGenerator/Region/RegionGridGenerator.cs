using System.Collections.Generic;
using UnityEngine;

using CellIndex = System.Int32;

namespace SAB.Terrain
{
	public class RegionTile
	{
		public List<float> RegionAmounts	= new List<float>((int)RegionType.Count + 1);
		public float Height					= 0.0f;
		public RegionType MainRegion		= RegionType.Count;
		public CellIndex Cell				= -1;
		
		public void Reset()
		{
			for (int t = 0; t < (int) RegionType.Count + 1; ++t)
			{
				RegionAmounts.Add(0.0f);
            }

			MainRegion = RegionType.Count;

			RegionAmounts[(int) RegionType.Count] = 1.0f;
		}

		public Color GetDebugColor()
		{
			Color col = new Color(0,0,0, 1);

			for (int t = 0; t < (int) RegionType.Count; ++t)
			{
				float amount = RegionAmounts[t];
				Color typeCol = RegionMapTypes.GetDebugColor((RegionType)t);

				col.r += amount*typeCol.r;
				col.g += amount*typeCol.g;
				col.b += amount*typeCol.b;
			}		

			return col;
		}
	}

	///////////////////////////////////////////////////////////////////////////

	public class RegionGridGenerator
	{
		
		///////////////////////////////////////////////////////////////////////////

		private Vector2				m_HeightRangeY;
		private List<RegionCell>	m_RegionCells = null;
		private RegionGrid			m_RegionGrid;

		private RegionParameters		m_RegionParams;
		private RegionMapTransformation m_MapTransformation;

		///////////////////////////////////////////////////////////////////////////

		public RegionGrid		regionGrid		{ get { return m_RegionGrid; } }
		public Vector2			heightRangeY	{ get { return m_HeightRangeY; } }

		///////////////////////////////////////////////////////////////////////////

		public void GenerateRegionGrid(List<RegionCell> regionCells, RegionMapTransformation mapTransformation, RegionParameters regionParameters)
		{
			// 0) Init
			m_RegionCells		= regionCells;
			m_RegionParams		= regionParameters;
			m_MapTransformation	= mapTransformation;

			m_RegionGrid = new RegionGrid(mapTransformation.Resolution, mapTransformation.Resolution, mapTransformation.MapSizeWS);

			// 1) Assign Cells
			AssignAmountsToTiles();

			m_HeightRangeY = new Vector2(float.MaxValue, float.MinValue);

			// 2) Check consistency && find min/max Height
			for (int iy = 0; iy < m_RegionGrid.sizeZ; iy++)
			{
				for (int ix = 0; ix < m_RegionGrid.sizeX; ix++)
				{
					RegionTile tile = m_RegionGrid.GetAt(ix, iy);

					float amountSum = 0.0f;
					for (int t = 0; t < (int) RegionType.Count; ++t)
					{
						amountSum += tile.RegionAmounts[t];
					}

					if (amountSum < 0.99f || amountSum > 1.01f)
					{
						Debug.Assert(false, "Inconsistent Region Grid");
						break;
					}

					m_HeightRangeY.x = Mathf.Min(tile.Height, m_HeightRangeY.x);
					m_HeightRangeY.y = Mathf.Max(tile.Height, m_HeightRangeY.y);

					Debug.Assert(tile.RegionAmounts[(int) RegionType.Count] == 0.0f);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void AssignAmountsToTiles()
		{
			// for all cells...
			for (CellIndex c = 0; c < m_RegionCells.Count; ++c)
            {
				RegionCell curCell = m_RegionCells[c];
				
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
					m_MapTransformation.GetIndexRect(aabb, out minX, out minZ, out maxX, out maxZ);

					// every cell within triangle
					for (int iZ = minZ; iZ < maxZ; ++iZ)
					{
						for (int iX = minX; iX < maxX; ++iX)
						{
							RegionTile curTile = m_RegionGrid.GetAt(iX, iZ);

							Vector2 tileCenter = m_MapTransformation.GetTileCenter(iX, iZ);

							float bCur, bPrevNeigh, bNextNeigh;
							bool isInside = curTriangle.TryGetBarycentricCoordinates(tileCenter, out bCur, out bPrevNeigh, out bNextNeigh);
							
							if (!isInside)
							{
								continue;
							}

							curTile.MainRegion = curCell.RegionType;

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

							RegionCell cell_N  = curNeighbor.WasClamped  ? curCell : m_RegionCells[curNeighbor.NeighborIndexIfValid];
							RegionCell cell_N1 = prevNeighbor.WasClamped ? curCell : m_RegionCells[prevNeighbor.NeighborIndexIfValid];
							RegionCell cell_N2 = nextNeighbor.WasClamped ? curCell : m_RegionCells[nextNeighbor.NeighborIndexIfValid];

							Debug.Assert(curTile.RegionAmounts[(int) RegionType.Count] == 1.0f);
							curTile.RegionAmounts[(int) RegionType.Count] = 0.0f;

							const float ONE_THIRD = (1.0f / 3.0f);

							// c * (0.33 + 0.66*b_c)
							float amountCur = bCur * (2.0f * ONE_THIRD) + ONE_THIRD;
							curTile.RegionAmounts[(int) curCell.RegionType]  += amountCur;

							// N * (0.33* (b_n1 + b_n2))
							float amountN = ONE_THIRD * (bPrevNeigh + bNextNeigh);
							curTile.RegionAmounts[(int) cell_N.RegionType]   += amountN;

							// N1 * (0.33 * b_n1)
							float amountN1 = ONE_THIRD * (bPrevNeigh);
							curTile.RegionAmounts[(int) cell_N1.RegionType]  += amountN1;

							// N2 * (0.33 * b_n2)
							float amountN2 = ONE_THIRD * (bNextNeigh);
							curTile.RegionAmounts[(int) cell_N2.RegionType]  += amountN2;

							// Assign Cell
							curTile.Cell = c;

							// Assign Height (dependent on water distance)
							curTile.Height	=	amountCur	* curCell.ComputeHeightDueToWaterDistance(m_RegionParams) + 
												amountN		* cell_N.ComputeHeightDueToWaterDistance(m_RegionParams)  +
												amountN1	* cell_N1.ComputeHeightDueToWaterDistance(m_RegionParams) +
												amountN2	* cell_N2.ComputeHeightDueToWaterDistance(m_RegionParams);
							
						}
					}

				}

				
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void DebugDraw()
		{
			if (m_RegionGrid == null)
			{
				return;
			}

			const int MAX_ALLOWED_INDICES = 40;
			int maxIndexX = Mathf.Min(m_RegionGrid.sizeX, MAX_ALLOWED_INDICES);
			int maxIndexZ = Mathf.Min(m_RegionGrid.sizeZ, MAX_ALLOWED_INDICES);

			if (m_RegionParams.ShowRegionGrid)
			{
				float debugDrawHeight = 1.0f;

				for (int iZ = 0; iZ < maxIndexZ; iZ++)
				{
					for (int iX = 0; iX < maxIndexX; iX++)
					{
						Vector2 cellMin = m_MapTransformation.GetTileMin(iX, iZ);
						Vector2 cellMax = m_MapTransformation.GetTileMin(iX + 1, iZ + 1);
					
						Vector2 cellMinOffsetted = Vector2.Lerp(cellMin, cellMax, 0.05f);
						Vector2 cellMaxOffsetted = Vector2.Lerp(cellMin, cellMax, 0.95f);

						RegionTile tile = m_RegionGrid.GetAt(iX, iZ);

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

			if (m_RegionParams.ShowRegionGrid && m_RegionParams.ShowIndices)
			{
				float debugDrawHeight = 1.0f;

				for (int iZ = 0; iZ < maxIndexZ; iZ++)
				{
					for (int iX = 0; iX < maxIndexX; iX++)
					{
						Vector2 cellMin = m_MapTransformation.GetTileMin(iX, iZ);
						Vector2 cellMax = m_MapTransformation.GetTileMin(iX + 1, iZ + 1);
					
						Vector2 cellCenter = (cellMin + cellMax) * 0.5f;
						DebugHelper.DrawText(cellCenter, debugDrawHeight, Color.white, iX + "|" + iZ);
					}
				}
			}
		} //< end debug draw
	
	} //< end region grid generator


}