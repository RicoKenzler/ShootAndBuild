﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey		= System.Int64;
using PointIndex	= System.Int32;
using TriIndex		= System.Int32;

namespace SAB.Terrain
{
	public class VoronoiConverter
	{
		private VoronoiCreationState m_State;

		///////////////////////////////////////////////////////////////////////////

		public bool DelauneyToVoronoi(VoronoiCreationState creationState)
		{
			m_State = creationState;

			Debug.Assert(m_State.InputVerticesIncludingSuperTriangle.Count == m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE + 3);
			Debug.Assert(m_State.DelauneyTrianglesIncludingSuperTriangle.Count > 0);
			Debug.Assert(m_State.VoronoiCells.Count == 0);

			int pointCountWithoutSuperTriangle = m_State.InputVerticesIncludingSuperTriangle.Count - 3;

			m_State.VoronoiCells.Capacity = pointCountWithoutSuperTriangle;

			for (PointIndex p = 0; p < pointCountWithoutSuperTriangle; ++p)
			{
				m_State.VoronoiCells.Add(new VoronoiCell());
			} 

			// Register All Edges & Centers
			Dictionary<EdgeKey, TwoTriangleIndices> edgesToTriangleIndices = new Dictionary<long, TwoTriangleIndices>();
			List<Vector2> triangleVoronoiCenters = new List<Vector2>();
			triangleVoronoiCenters.Capacity = m_State.DelauneyTrianglesIncludingSuperTriangle.Count;

			for (TriIndex t = 0; t < m_State.DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
			{
				TriangleI currentTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[t];

				Vector2 center = currentTriangle.CircumscribedCircle.Center;	
				triangleVoronoiCenters.Add(center);

				for (int side = 0; side < 3; ++side)
				{
					EdgeIndices edge = currentTriangle.GetEdgeOppositeTo(side);

					EdgeKey edgeKey = edge.ComputeKey();
					
					if (edgesToTriangleIndices.ContainsKey(edgeKey))
					{
						TwoTriangleIndices twoIndicesOld = edgesToTriangleIndices[edgeKey];
						Debug.Assert(twoIndicesOld.T2 == -1);
						twoIndicesOld.T2 = t;
						edgesToTriangleIndices[edgeKey] = twoIndicesOld;
					}
					else
					{
						TwoTriangleIndices twoTriangles;
						twoTriangles.T1 = t;
						twoTriangles.T2 = -1;
						edgesToTriangleIndices[edgeKey] = twoTriangles;
					}
				}	
			}

			// Add All Edges
			for (TriIndex t = 0; t < m_State.DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
			{
				// Tell every VoronoiCell (DelauneyVertex), which HullVertices it has
				TriangleI currentTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[t];

				Vector2 currentTriangleCenter = triangleVoronoiCenters[t];

				for (int side = 0; side < 3; ++side)
				{
					EdgeIndices curEdgeIndices = currentTriangle.GetEdgeOppositeTo(side);
					Vector2  curOppositeVertex = m_State.InputVerticesIncludingSuperTriangle[currentTriangle.GetIndex(side)];

					TwoTriangleIndices neighborTriangles = edgesToTriangleIndices[curEdgeIndices.ComputeKey()];

					TriIndex otherTriangleIndex = neighborTriangles.GetIndexUnequalTo(t);

					if (otherTriangleIndex < t)
					{
						// already processed this pair
						continue;
					}

					TriangleI otherTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[otherTriangleIndex];
					Vector2 otherTriangleCenter = triangleVoronoiCenters[otherTriangleIndex];

					Edge edgeForNeighbors = new Edge(currentTriangleCenter, otherTriangleCenter);
						
					if (curEdgeIndices.IndexP1 < m_State.VoronoiCells.Count)
					{
						VoronoiNeighbor neighbor1 = new VoronoiNeighbor(curEdgeIndices.IndexP2, edgeForNeighbors);
						m_State.VoronoiCells[curEdgeIndices.IndexP1].NeighborCellsCCW.Add(neighbor1);
					}
					if (curEdgeIndices.IndexP2 < m_State.VoronoiCells.Count)
					{
						VoronoiNeighbor neighbor2 = new VoronoiNeighbor(curEdgeIndices.IndexP1, edgeForNeighbors);
						m_State.VoronoiCells[curEdgeIndices.IndexP2].NeighborCellsCCW.Add(neighbor2);	
					}

				}
			}

			List<int> indicesToRemove = new List<int>();

			// Remove and Clamp edges
			for (PointIndex c = 0; c < m_State.VoronoiCells.Count; ++c)
			{
				if (m_State.VoronoiParams.SuppressClamping)
				{
					break;
				}

				indicesToRemove.Clear();
				VoronoiCell currentCell = m_State.VoronoiCells[c];
				for (int n = 0; n < currentCell.NeighborCellsCCW.Count; ++n)
				{
					VoronoiNeighbor neighborCopy = currentCell.NeighborCellsCCW[n];

					bool edgeStartOutside	= IsOutsideClampRect(neighborCopy.EdgeToNeighbor.Start);
					bool edgeEndOutside		= IsOutsideClampRect(neighborCopy.EdgeToNeighbor.End);

					if (edgeStartOutside && edgeEndOutside)
					{
						// both outside: does not matter
						indicesToRemove.Add(n);
						continue;
					}
					else if (!edgeStartOutside && !edgeEndOutside)
					{
						// regular inner edge
						continue;
					}
					else 
					{
						// One inside, one outside: Clamp

						if (edgeStartOutside)
						{
							// Clamp start
							neighborCopy.EdgeToNeighbor.Start = ComputeClampedEdgeEnd(neighborCopy.EdgeToNeighbor.End, neighborCopy.EdgeToNeighbor.Start);
						}
						else
						{
							// Clamp end
							neighborCopy.EdgeToNeighbor.End = ComputeClampedEdgeEnd(neighborCopy.EdgeToNeighbor.Start, neighborCopy.EdgeToNeighbor.End); 
						}

						neighborCopy.WasClamped = true;
						currentCell.NeighborCellsCCW[n] = neighborCopy;
					}	
				}

				for (int r = indicesToRemove.Count - 1; r >= 0; --r)
				{
					currentCell.NeighborCellsCCW.RemoveAt(indicesToRemove[r]);
				}

				if (indicesToRemove.Count > 0)
				{
					currentCell.CalculateCentroid();
				}
			}

			// Sort CCW and force edge directions
			for (PointIndex c = 0; c < m_State.VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = m_State.VoronoiCells[c];
				Debug.Assert(m_State.VoronoiCells.Count >= 3);
				
				currentCell.SortEdgesCCW(true);
			}

			// Add Edges at clamp rect
			for (PointIndex c = 0; c < m_State.VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = m_State.VoronoiCells[c];
				
				if (m_State.VoronoiParams.SuppressNewBorderEdges)
				{
					break;
				}

				bool success = currentCell.AddClampRectEdgesToFillOpenPolygon(new Vector2(0.0f, 0.0f), m_State.DIMENSIONS);
				
				if (!success)
				{
					return false;
				}
			}

			// Calculate Centroid
			for (PointIndex c = 0; c < m_State.VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = m_State.VoronoiCells[c];
				currentCell.CalculateCentroid();

				if (currentCell.NeighborCellsCCW.Count == 0)
				{
					Debug.LogWarning("Degenerated Cell found");
					return false;
				}
			}

			return true;
		}
			
		///////////////////////////////////////////////////////////////////////////

		public bool IsOutsideClampRect(Vector2 point)
		{
			return (point.x < 0.0f) || (point.y < 0.0f) || (point.x > m_State.DIMENSIONS.x) || (point.y > m_State.DIMENSIONS.y);
		}

		///////////////////////////////////////////////////////////////////////////

		Vector2 ClampToBorder(Vector2 startPoint, Vector2 dirNorm, Vector2 clampRectMin, Vector2 clampRectMax)
		{
			Vector2 maxDeltasPos = (clampRectMax - startPoint);
			Vector2 minDeltasNeg = (clampRectMin - startPoint);

			float maxFactorX = float.MaxValue;
			float maxFactorY = float.MaxValue;

			float EPSILON = 0.001f;

			if (dirNorm.x > EPSILON)
			{
				maxFactorX = maxDeltasPos.x / dirNorm.x;
			}
			else if (dirNorm.x < -EPSILON)
			{
				maxFactorX = minDeltasNeg.x / dirNorm.x;
			}
			
			if (dirNorm.y > EPSILON)
			{
				maxFactorY = maxDeltasPos.y / dirNorm.y;
			}
			else if (dirNorm.y < -EPSILON)
			{
				maxFactorY = minDeltasNeg.y / dirNorm.y;
			}

			Debug.Assert(maxFactorX != float.MaxValue || maxFactorY != float.MaxValue);

			float factor = Mathf.Min(maxFactorX, maxFactorY);

			Vector2 newPoint = startPoint + factor * dirNorm;
			
			newPoint.x = Mathf.Max(newPoint.x, clampRectMin.x);
			newPoint.x = Mathf.Min(newPoint.x, clampRectMax.x);
			newPoint.y = Mathf.Max(newPoint.y, clampRectMin.y);
			newPoint.y = Mathf.Min(newPoint.y, clampRectMax.y);

			// floatingPointPrecision could make -10 to -9.99999
			if (maxFactorX < maxFactorY)
			{
				if (dirNorm.x < 0.0f)
				{
					newPoint.x = 0.0f;
				}
				else
				{
					newPoint.x = m_State.DIMENSIONS.x;
				}
			}
			else
			{
				if (dirNorm.y < 0.0f)
				{
					newPoint.y = 0.0f;
				}
				else
				{
					newPoint.y = m_State.DIMENSIONS.y;
				}
			}

			return newPoint;
		}
	
		///////////////////////////////////////////////////////////////////////////

		public Vector2 ComputeClampedEdgeEnd(Vector2 insideEdgeStart, Vector2 outsideEdgeEnd)
		{
			Vector2 moveDirection = outsideEdgeEnd - insideEdgeStart;
			float moveDirectionLength = moveDirection.magnitude;
			
			if (moveDirectionLength == 0.0f)
			{
				Debug.Assert(false, "Degenerated Edge");
				return insideEdgeStart;
			}

			moveDirection.x /= moveDirectionLength;
			moveDirection.y /= moveDirectionLength;

			return ClampToBorder(insideEdgeStart, moveDirection, new Vector2(0.0f, 0.0f), m_State.DIMENSIONS);
		}


	} //< end class

}