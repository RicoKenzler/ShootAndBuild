using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey		= System.Int64;
using PointIndex	= System.Int32;
using TriIndex		= System.Int32;

namespace SAB
{
	public class VoronoiCreator
	{
		VoronoiCreationState State		= new VoronoiCreationState();

		VoronoiPointGenerator PointGenerator	= new VoronoiPointGenerator();
		DelauneyTriangulator Triangulator		= new DelauneyTriangulator();
		VoronoiConverter Converter				= new VoronoiConverter();

		private bool WasRunAtLeastOnce			= false;

		// Bowyer-Watson Algorithm for Delauny-Triangulation
		public bool GenerateVoronoi(int seed, VoronoiParameters voronoiParams, Vector2 gridCenterWS, Vector2 gridSizeWS)
		{
			// TODO: Am anfang alle input point überlappungen verhindern
			// TODO: Relaxation
			// TODO: Alle Early outs (inkl. Exceptions) abfangen und neugenerierung anstossen
			// TODO: Am Ende (nach relaxation) Sanity check

			WasRunAtLeastOnce = true;

			State = new VoronoiCreationState();
			State.VoronoiParams = voronoiParams;
			State.MIN_COORDS = gridCenterWS - gridSizeWS;
			State.MAX_COORDS = gridCenterWS + gridSizeWS;
			State.DIMENSIONS = State.MAX_COORDS - State.MIN_COORDS;
			State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE = State.VoronoiParams.VoronoiPointCount;

			PointGenerator.CreateRandomPoints(State, seed);

			bool delauneySuccess = Triangulator.GetDelauneyTriangluation(State);

			if (!delauneySuccess)
			{
				return false;
			}

			bool voronoiSuccess = Converter.DelauneyToVoronoi(State);

			if (!voronoiSuccess)
			{
				return false;
			}

			return voronoiSuccess;
		}

		// -------------------------------------------------------------------

		public void DebugDraw(VoronoiParameters voronoiParams)
		{
			if (!WasRunAtLeastOnce)
			{
				return; 
			} 

			float debugDrawHeight = 1.0f;

			float triangleOffsetLength = 0.05f * ((State.DIMENSIONS.x + State.DIMENSIONS.y) / State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE);
			triangleOffsetLength = Mathf.Lerp(triangleOffsetLength, 0.1f, 0.9f);

			if (voronoiParams.ShowDelauney)
			{
				// Draw all Triangles:
				for (TriIndex t = 0; t < State.DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
				{
					Triangle currentTriangle = State.DelauneyTrianglesIncludingSuperTriangle[t];

					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && !currentTriangle.HasAsVertex(voronoiParams.DebugDrawOnlyVertexIndex))
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1 && t != voronoiParams.DebugDrawOnlyTriangleIndex)
					{
						continue;
					}

					Vector3 p1 = new Vector3(State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].x, debugDrawHeight, State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].y);
					Vector3 p2 = new Vector3(State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].x, debugDrawHeight, State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].y);
					Vector3 p3 = new Vector3(State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].x, debugDrawHeight, State.InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].y);

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
					int sharedPointCount = currentTriangle.SharedPointCount(State.SuperTriangle, out a1, out a2, out a3);
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
						DrawText(centroid, col, t.ToString());
					}	
				}
			}

			if (voronoiParams.ShowVoronoi)
			{
				for (PointIndex c = 0; c < State.VoronoiCells.Count; ++c)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && c != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					VoronoiCell currentCell = State.VoronoiCells[c];
					Color cellBaseColor = new Color(c * 0.123f % 1.0f, c * 0.311f % 1.0f, c * 0.76f % 1.0f);

					for (int p = 0; p < currentCell.NeighborCells.Count; ++p)
					{
						VoronoiNeighbor neighbor = currentCell.NeighborCells[p];

						if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
						{
							EdgeIndices currentEdgeIndices = new EdgeIndices(c, neighbor.NeighborIndex);
							Triangle focusTriangle = State.DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
							if (!focusTriangle.SharesEdge(currentEdgeIndices))
							{
								continue;
							}
						}


						Vector2 start		= neighbor.EdgeToNeighbor.Start;
						Vector2 end	= neighbor.EdgeToNeighbor.End;

						Vector2 epsilonOffsetP1 = (currentCell.Centroid - start);		epsilonOffsetP1.Normalize();
						Vector2 epsilonOffsetP2 = (currentCell.Centroid - end);	epsilonOffsetP2.Normalize();

						start		+= epsilonOffsetP1 * triangleOffsetLength;
						end	+= epsilonOffsetP2 * triangleOffsetLength;

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
							DrawText(startPointOffsetted, debugDrawHeight, col, p.ToString());
						}
					}

					if (voronoiParams.ShowIndices)
					{
						DrawText(currentCell.Centroid, debugDrawHeight, cellBaseColor, c.ToString());
					}	
				}
			}

			if (voronoiParams.ShowBorder)
			{
				Color col = new Color(0,0, 0.2f);

				Gizmos.color = col;

				Gizmos.DrawLine(new Vector3(State.MIN_COORDS.x, debugDrawHeight, State.MIN_COORDS.y), new Vector3(State.MIN_COORDS.x, debugDrawHeight, State.MAX_COORDS.y));
				Gizmos.DrawLine(new Vector3(State.MIN_COORDS.x, debugDrawHeight, State.MAX_COORDS.y), new Vector3(State.MAX_COORDS.x, debugDrawHeight, State.MAX_COORDS.y));
				Gizmos.DrawLine(new Vector3(State.MAX_COORDS.x, debugDrawHeight, State.MAX_COORDS.y), new Vector3(State.MAX_COORDS.x, debugDrawHeight, State.MIN_COORDS.y));
				Gizmos.DrawLine(new Vector3(State.MAX_COORDS.x, debugDrawHeight, State.MIN_COORDS.y), new Vector3(State.MIN_COORDS.x, debugDrawHeight, State.MIN_COORDS.y));
			}

			if (voronoiParams.ShowPoints)
			{
				Color col = new Color(1.0f, 1.0f, 0.0f);
				Gizmos.color = new Color(col.r, col.g, col.b, 0.5f);

				for (int p = 0; p < State.InputVerticesIncludingSuperTriangle.Count; ++p)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && p != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
					{
						Triangle focusTriangle = State.DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
						if (!focusTriangle.HasAsVertex(p))
						{
							continue;
						}
					}


					Vector2 pos = State.InputVerticesIncludingSuperTriangle[p];

					Gizmos.DrawSphere(new Vector3(pos.x, debugDrawHeight, pos.y), triangleOffsetLength * 2.0f);

					if (voronoiParams.ShowIndices)
					{
						DrawText(pos, debugDrawHeight, col, p.ToString());
					}	
				}
			}
		}

		// -------------------------------------------------------------------

		static void DrawText(Vector2 pos2, float height, Color col, string text)
		{
			DrawText(new Vector3(pos2.x, height, pos2.y), col, text);
		}

		// -------------------------------------------------------------------

		static void DrawText(Vector3 pos3, Color col, string text)
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = col;
			UnityEditor.Handles.Label (pos3, text, style);
		}

	} //< end class
}
