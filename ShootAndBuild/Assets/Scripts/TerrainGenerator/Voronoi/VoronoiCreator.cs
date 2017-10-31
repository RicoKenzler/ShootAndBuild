using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey		= System.Int64;
using PointIndex	= System.Int32;
using TriIndex		= System.Int32;

namespace SAB.Terrain
{
	public class VoronoiCreator
	{
		private VoronoiCreationState m_State			= new VoronoiCreationState();

		private VoronoiPointGenerator m_PointGenerator	= new VoronoiPointGenerator();
		private DelauneyTriangulator m_Triangulator		= new DelauneyTriangulator();
		private VoronoiConverter m_Converter			= new VoronoiConverter();

		private bool	m_WasRunAtLeastOnce			= false;
		private int		m_InvalidTries				= 0;
		private float	m_RelaxationAmountLeft		= 0.0f;

		public List<VoronoiCell> GenerateVoronoi(int seed, VoronoiParameters voronoiParams, Vector2 gridSizeWS, bool afterInvalidRun = false)
		{
			// Return Cached Voronoi
			if (voronoiParams.NoRecomputation && m_State.VoronoiCells.Count == voronoiParams.VoronoiPointCount)
			{
				return m_State.VoronoiCells;
			}

			// TODO: Relaxation
			// TODO: Alle Early outs (inkl. Exceptions) abfangen und neugenerierung anstossen
			// TODO: Am Ende (nach relaxation) Sanity check

			// 0) Only allow n invalid runs
			if (afterInvalidRun)
			{
				m_InvalidTries++;

				if (m_InvalidTries > 3)
				{
					Debug.Log("Too many invalid tries in a row. Aborting");
					return null;
				}
			}
			else
			{
				m_InvalidTries = 0;
			}

			// 0) Init
			m_State = new VoronoiCreationState();
			m_State.VoronoiParams = voronoiParams;
			m_State.DIMENSIONS = gridSizeWS;
			m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE = m_State.VoronoiParams.VoronoiPointCount;

			m_WasRunAtLeastOnce = true;
			m_RelaxationAmountLeft = m_State.VoronoiParams.RelaxationAmount;

			// 1) Create Random Points
			m_PointGenerator.CreateRandomPoints(m_State, seed);

			do
			{
				// 2) Delauney
				bool delauneySuccess = m_Triangulator.GetDelauneyTriangluation(m_State);

				if (!delauneySuccess)
				{
					Debug.Log("Error happened during DelauneyTriangulation. Retrying...");
					return GenerateVoronoi(seed + 1, voronoiParams, gridSizeWS, true);
				}

				// 3) Voronoi
				bool voronoiSuccess = m_Converter.DelauneyToVoronoi(m_State);

				if (!voronoiSuccess)
				{
					Debug.Log("Error happened during DelauneyToVoronoi. Retrying...");
					return GenerateVoronoi(seed + 1, voronoiParams, gridSizeWS, true);
				}

				if (m_RelaxationAmountLeft <= 0.0f)
				{
					return m_State.VoronoiCells;
				}
				else
				{
					DoLloydRelaxation(m_State, Mathf.Min(m_RelaxationAmountLeft, 1.0f));

					m_RelaxationAmountLeft -= 1.0f;

					// Remove all temporary structs and super triangle (will be re-evaluated in next try)
					m_State.InputVerticesIncludingSuperTriangle.RemoveRange(voronoiParams.VoronoiPointCount, m_State.InputVerticesIncludingSuperTriangle.Count - voronoiParams.VoronoiPointCount);
					m_State.DelauneyTrianglesIncludingSuperTriangle	= new List<TriangleI>();
					m_State.VoronoiCells								= new List<VoronoiCell>();
				}
			}
			while (true);
		}

		///////////////////////////////////////////////////////////////////////////

		void DoLloydRelaxation(VoronoiCreationState state, float relaxationAmount)
		{
			Debug.Assert(relaxationAmount > 0.0f && relaxationAmount <= 1.0f);

			for (PointIndex c = 0; c < state.VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = state.VoronoiCells[c];

				state.InputVerticesIncludingSuperTriangle[c] = Vector2.Lerp(state.InputVerticesIncludingSuperTriangle[c], currentCell.Centroid, relaxationAmount);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public void DebugDraw(VoronoiParameters voronoiParams)
		{
			if (!m_WasRunAtLeastOnce)
			{
				return; 
			} 

			float debugDrawHeight = 1.0f;

			float triangleOffsetLength = 0.05f * ((m_State.DIMENSIONS.x + m_State.DIMENSIONS.y) / m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE);
			triangleOffsetLength = Mathf.Lerp(triangleOffsetLength, 0.1f, 0.9f);

			if (voronoiParams.ShowDelauney)
			{
				// Draw all Triangles:
				for (TriIndex t = 0; t < m_State.DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
				{
					TriangleI currentTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[t];

					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && !currentTriangle.HasAsVertex(voronoiParams.DebugDrawOnlyVertexIndex))
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1 && t != voronoiParams.DebugDrawOnlyTriangleIndex)
					{
						continue;
					}

					Vector3 p1 = new Vector3(m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].x, debugDrawHeight, m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].y);
					Vector3 p2 = new Vector3(m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].x, debugDrawHeight, m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].y);
					Vector3 p3 = new Vector3(m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].x, debugDrawHeight, m_State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].y);

					Vector3 centroid = (p1 + p2 + p3) / 3.0f;

					Vector3 epsilonOffsetP1 = (centroid - p1); epsilonOffsetP1.Normalize();
					Vector3 epsilonOffsetP2 = (centroid - p2); epsilonOffsetP2.Normalize();
					Vector3 epsilonOffsetP3 = (centroid - p3); epsilonOffsetP3.Normalize();
					p1 += epsilonOffsetP1 * triangleOffsetLength;
					p2 += epsilonOffsetP2 * triangleOffsetLength;
					p3 += epsilonOffsetP3 * triangleOffsetLength;

					Color col = new Color(t * 0.123f % 1.0f, t * 0.311f % 1.0f, t * 0.76f % 1.0f);
					float minCol = Mathf.Min(col.r, col.g, col.b);

					if (minCol < 0.2f)
					{
						col.r = Mathf.Min(col.r * 2.0f, 1.0f);
						col.g = Mathf.Min(col.r * 2.0f, 1.0f);
						col.b = Mathf.Min(col.r * 2.0f, 1.0f);
					}

					col = Color.Lerp(Color.green, col, 0.6f);

					bool a1; bool a2; bool a3;
					int sharedPointCount = currentTriangle.SharedPointCount(m_State.SuperTriangle, out a1, out a2, out a3);
					if (sharedPointCount == 1)
					{
						col = new Color(0.0f, 0.0f, 0.0f);
					}
					else if (sharedPointCount >= 2)
                    {
						col = new Color(0.2f, 0.2f, 0.2f);
					}

					Gizmos.color = col;
					Gizmos.DrawLine(p1, p2);
					Gizmos.DrawLine(p1, p3);
					Gizmos.DrawLine(p2, p3);

					if (voronoiParams.ShowIndices)
					{
						DebugHelper.DrawText(centroid, col, t.ToString());
					}	
				}
			}

			if (voronoiParams.ShowVoronoi)
			{
				for (PointIndex c = 0; c < m_State.VoronoiCells.Count; ++c)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && c != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					VoronoiCell currentCell = m_State.VoronoiCells[c];
					Color cellBaseColor = new Color(c * 0.123f % 1.0f, c * 0.311f % 1.0f, c * 0.76f % 1.0f);

					for (int p = 0; p < currentCell.NeighborCellsCCW.Count; ++p)
					{
						VoronoiNeighbor neighbor = currentCell.NeighborCellsCCW[p];

						if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
						{
							EdgeIndices currentEdgeIndices = new EdgeIndices(c, neighbor.NeighborIndexIfValid);
							TriangleI focusTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
							if (!focusTriangle.SharesEdge(currentEdgeIndices))
							{
								continue;
							}
						}


						Vector2 start	= neighbor.EdgeToNeighbor.Start;
						Vector2 end		= neighbor.EdgeToNeighbor.End;

						Vector2 epsilonOffsetP1 = (currentCell.Centroid - start);		epsilonOffsetP1.Normalize();
						Vector2 epsilonOffsetP2 = (currentCell.Centroid - end);	epsilonOffsetP2.Normalize();

						start	+= epsilonOffsetP1 * triangleOffsetLength;
						end		+= epsilonOffsetP2 * triangleOffsetLength;

						Color col = neighbor.WasClamped ? Color.Lerp(Color.blue, cellBaseColor, 0.4f) : Color.Lerp(Color.red, cellBaseColor, 0.4f);
						Color colWeak = Color.Lerp(col, Color.black, 0.5f);
						
						Gizmos.color = col;
						Gizmos.DrawLine(new Vector3(start.x, debugDrawHeight, start.y), new Vector3(end.x, debugDrawHeight, end.y));

						if (voronoiParams.DebugDrawOnlyTriangleIndex == -1 && voronoiParams.ShowVorArea)
						{
							Gizmos.color = colWeak;
							Gizmos.DrawLine(new Vector3(start.x, debugDrawHeight, start.y), new Vector3(currentCell.Centroid.x, debugDrawHeight, currentCell.Centroid.y));
							Gizmos.DrawLine(new Vector3(end.x, debugDrawHeight, end.y), new Vector3(currentCell.Centroid.x, debugDrawHeight, currentCell.Centroid.y));
						}

						if (voronoiParams.ShowVorOrigentation)
						{
							Vector2 startPointOffsetted = Vector2.Lerp(start, end, 0.1f);
							DebugHelper.DrawText(startPointOffsetted, debugDrawHeight, col, p.ToString());
						}
					}

					if (voronoiParams.ShowIndices)
					{
						DebugHelper.DrawText(currentCell.Centroid, debugDrawHeight, cellBaseColor, c.ToString());
					}	
				}
			}

			if (voronoiParams.ShowBorder)
			{
				Color col = new Color(0,0, 0.2f);

				Gizmos.color = col;

				Gizmos.DrawLine(new Vector3(0.0f,				debugDrawHeight, 0.0f),					new Vector3(0.0f,				debugDrawHeight, m_State.DIMENSIONS.y));
				Gizmos.DrawLine(new Vector3(0.0f,				debugDrawHeight, m_State.DIMENSIONS.y),	new Vector3(m_State.DIMENSIONS.x, debugDrawHeight, m_State.DIMENSIONS.y));
				Gizmos.DrawLine(new Vector3(m_State.DIMENSIONS.x, debugDrawHeight, m_State.DIMENSIONS.y),	new Vector3(m_State.DIMENSIONS.x, debugDrawHeight, 0.0f));
				Gizmos.DrawLine(new Vector3(m_State.DIMENSIONS.x, debugDrawHeight, 0.0f),					new Vector3(0.0f,				debugDrawHeight, 0.0f));
			}

			if (voronoiParams.ShowPoints)
			{
				Color col = new Color(1.0f, 1.0f, 0.0f);
				Gizmos.color = new Color(col.r, col.g, col.b, 0.5f);

				for (int p = 0; p < m_State.InputVerticesIncludingSuperTriangle.Count; ++p)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && p != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
					{
						TriangleI focusTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
						if (!focusTriangle.HasAsVertex(p))
						{
							continue;
						}
					}


					Vector2 pos = m_State.InputVerticesIncludingSuperTriangle[p];

					Gizmos.DrawSphere(new Vector3(pos.x, debugDrawHeight, pos.y), triangleOffsetLength * 2.0f);

					if (voronoiParams.ShowIndices)
					{
						DebugHelper.DrawText(pos, debugDrawHeight, col, p.ToString());
					}	
				}
			}
		}

	} //< end class
}
