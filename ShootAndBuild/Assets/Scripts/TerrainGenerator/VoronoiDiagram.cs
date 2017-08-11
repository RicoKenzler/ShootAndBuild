using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey	= System.Int64;
using PointIndex	= System.Int32;
using TriIndex  = System.Int32;

namespace SAB
{
	public struct Circle
	{
		public Vector2 Center;
		public float   Radius;

		public bool IsPointInside(Vector2 point)
		{
			return Vector2.SqrMagnitude(point - Center) < (Radius * Radius);
		}
	}

	public struct EdgeIndices
	{
		public PointIndex IndexP1;
		public PointIndex IndexP2;

		public EdgeIndices(PointIndex indexP1, PointIndex indexP2)
		{
			IndexP1 = indexP1;
			IndexP2 = indexP2;
		}

		public EdgeKey ComputeKey()
		{
			PointIndex smallerIndex	= IndexP1 < IndexP2 ? IndexP1 : IndexP2;
			PointIndex biggerIndex	= IndexP1 > IndexP2 ? IndexP1 : IndexP2;

			return biggerIndex * 1000*1000*1000 + smallerIndex;
		}

		public Vector2 GetCenter(List<Vector2> pointList)
		{
			return (pointList[IndexP1] + pointList[IndexP2]) * 0.5f;
		}
	}

	public struct TwoTriangleIndices
	{
		public TriIndex T1;
		public TriIndex T2;

		public TriIndex GetIndexUnequalTo(TriIndex T)
		{
			return (T1 == T) ? T2 : T1;
		}
	}

	public struct Triangle
	{
		public PointIndex IndexP0;
		public PointIndex IndexP1;
		public PointIndex IndexP2;

		public PointIndex GetIndex(int i)
		{
			switch (i)
			{
				case 0: return IndexP0;
				case 1: return IndexP1;
			}

			return IndexP2;
		}

		public bool HasAsVertex(PointIndex vertex)
		{
			return (IndexP0 == vertex || IndexP1 == vertex || IndexP2 == vertex);
		}

		public PointIndex GetIndexOppositeTo(EdgeIndices edgeIndices)
		{
			bool shared01 =	((edgeIndices.IndexP1 == IndexP0 && edgeIndices.IndexP2 == IndexP1) || (edgeIndices.IndexP1 == IndexP1 && edgeIndices.IndexP2 == IndexP0));
			bool shared12 = ((edgeIndices.IndexP1 == IndexP1 && edgeIndices.IndexP2 == IndexP2) || (edgeIndices.IndexP1 == IndexP2 && edgeIndices.IndexP2 == IndexP1));
			bool shared02 = ((edgeIndices.IndexP1 == IndexP0 && edgeIndices.IndexP2 == IndexP2) || (edgeIndices.IndexP1 == IndexP2 && edgeIndices.IndexP2 == IndexP0));
			
			if (shared01)
			{
				return IndexP2;
			}
			else if (shared12)
			{
				return IndexP0;
			}
			else if (shared02)
			{
				return IndexP1;
			}

			Debug.Assert(false);
			return -1;
		}

		public EdgeIndices GetEdgeOppositeTo(int i)
		{
			return new EdgeIndices(GetIndex((i + 1) % 3), GetIndex((i + 2) % 3));
		}

		public Circle CircumscribedCircle;

		public Vector2 GetCentroid(List<Vector2> pointList)
		{
			return (pointList[IndexP0] + pointList[IndexP1] + pointList[IndexP2]) / 3.0f;
		}

		public bool SharesEdge(EdgeIndices edge)
		{
			return (((edge.IndexP1 == IndexP0) || (edge.IndexP1 == IndexP1) || (edge.IndexP1 == IndexP2)) && 
					((edge.IndexP2 == IndexP0) || (edge.IndexP2 == IndexP1) || (edge.IndexP2 == IndexP2)));
		}

		public bool SharesPointWith(Triangle other)
		{
			bool ownP0Shared = ((IndexP0 == other.IndexP0) || (IndexP0 == other.IndexP1) || (IndexP0 == other.IndexP2));
			bool ownP1Shared = ((IndexP1 == other.IndexP0) || (IndexP1 == other.IndexP1) || (IndexP1 == other.IndexP2));
			bool ownP2Shared = ((IndexP2 == other.IndexP0) || (IndexP2 == other.IndexP1) || (IndexP2 == other.IndexP2));

			return ownP0Shared || ownP1Shared || ownP2Shared;
		}

		public int SharedPointCount(Triangle other, out bool ownP0Shared, out bool ownP1Shared, out bool ownP2Shared)
		{
			ownP0Shared = ((IndexP0 == other.IndexP0) || (IndexP0 == other.IndexP1) || (IndexP0 == other.IndexP2));
			ownP1Shared = ((IndexP1 == other.IndexP0) || (IndexP1 == other.IndexP1) || (IndexP1 == other.IndexP2));
			ownP2Shared = ((IndexP2 == other.IndexP0) || (IndexP2 == other.IndexP1) || (IndexP2 == other.IndexP2));
		
			return (ownP0Shared ? 1 : 0) + (ownP1Shared ? 1 : 0) + (ownP2Shared ? 1 : 0);
		}

		public bool TryInit(PointIndex indexP0, PointIndex indexP1, PointIndex indexP2, List<Vector2> pointList)
		{
			IndexP0 = indexP0;
			IndexP1 = indexP1;
			IndexP2 = indexP2;

			bool success = CalculateCircumscribedCircle(pointList);
			return success;
		}

		bool CalculateCircumscribedCircle(List<Vector2> pointList)
		{
			Vector2 p0 = pointList[IndexP0];
			Vector2 p1 = pointList[IndexP1];
			Vector2 p2 = pointList[IndexP2];

			return VoronoiDiagram.FindCircumscribedCircle(p0,p1,p2, out CircumscribedCircle);
		}
	}

	public struct Edge
	{
		public Vector2 Start;
		public Vector2 End;

		public Edge(Vector2 start, Vector2 end)
		{
			Start = start;
			End = end;
		}

		public Vector2 GetCenter()
		{
			return (Start + End) * 0.5f;
		}

		public void MakeDirectionUnique()
		{
			// Make start smaller than end
			Vector2 intermediate;

			if (Start.x < End.x)
			{
				intermediate = Start;
				Start = End;
				End = intermediate;
				return;
			}
			else if (Start.x == End.x)
			{
				if (Start.y < End.y)
				{
					intermediate = Start;
					Start = End;
					End = intermediate;
					return;
				}
			}
		}
	}

	public struct VoronoiNeighbor
	{
		public PointIndex	NeighborIndex;
		public Edge			EdgeToNeighbor;
		public bool			WasClamped;

		public VoronoiNeighbor(PointIndex neighborIndex, Edge edge)
		{
			EdgeToNeighbor				= edge;
			NeighborIndex				= neighborIndex;
			WasClamped					= false;
		}
	}

	public class VoronoiCell
	{
		public List<VoronoiNeighbor> NeighborCells = new List<VoronoiNeighbor>();
		public Vector2 Centroid = new Vector2(0.0f, 0.0f);

		public void CalculateCentroid()
		{
			Centroid = new Vector2(0.0f, 0.0f);

			foreach (VoronoiNeighbor neighbor in NeighborCells)
			{
				Centroid += neighbor.EdgeToNeighbor.GetCenter();
			}

			Centroid /= (float) NeighborCells.Count;
		}

		public void SortEdgesCCW(bool forceEdgesDirection)
		{
			CalculateCentroid();

			NeighborCells.Sort(delegate(VoronoiNeighbor left, VoronoiNeighbor right)
			{
				Vector2 centroidToLeft = left.EdgeToNeighbor.GetCenter() - Centroid;
				float angleLeft = Vector2.SignedAngle(centroidToLeft, Vector2.right);
					
				Vector2 centroidToRight = right.EdgeToNeighbor.GetCenter() - Centroid;
				float angleRight = Vector2.SignedAngle(centroidToRight, Vector2.right);

				if (angleLeft > angleRight)
				{
					return -1;
				}
				else if (angleLeft < angleRight)
				{
					return 1;
				}

				return 0;
			});

			if (forceEdgesDirection)
			{
				for (int n = 0; n < NeighborCells.Count; n+=1)
				{
					int nextIndex = (n+1) % NeighborCells.Count;

					VoronoiNeighbor neighborCellCopy		= NeighborCells[n];
					VoronoiNeighbor nextNeighborCellCopy	= NeighborCells[nextIndex];

					Edge currentEdge = neighborCellCopy.EdgeToNeighbor;
					Edge nextEdge = NeighborCells[nextIndex].EdgeToNeighbor;

					bool startConnects		= false;
					bool nextEndConnects	= false;

					if (currentEdge.Start == nextEdge.Start)
					{
						startConnects		= true;
					}
					else if (currentEdge.Start == nextEdge.End)
					{
						startConnects		= true;
						nextEndConnects		= true;
					}
					else if (currentEdge.End == nextEdge.Start)
					{

					}
					else if (currentEdge.End == nextEdge.End)
					{
						nextEndConnects		= true;
					}
					else
					{
						Debug.Assert(neighborCellCopy.WasClamped || nextNeighborCellCopy.WasClamped, "Invalid Polygon");
					}

					Vector2 intermediate;

					if (startConnects)
					{
						intermediate		= currentEdge.Start;
						currentEdge.Start	= currentEdge.End;
						currentEdge.End		= intermediate;

						neighborCellCopy.EdgeToNeighbor = currentEdge;
						NeighborCells[n] = neighborCellCopy;
					}

					if (nextEndConnects)
					{
						intermediate		= nextEdge.Start;
						nextEdge.Start		= nextEdge.End;
						nextEdge.End		= intermediate;

						nextNeighborCellCopy.EdgeToNeighbor = nextEdge;
						NeighborCells[nextIndex] = nextNeighborCellCopy;
					}
				}
			}
		}

		public void AddClampRectEdgesToFillOpenPolygon(Vector2 MIN_COORDS, Vector2 MAX_COORDS)
		{
			int oldCount = NeighborCells.Count;

			for (int n = 0; n < NeighborCells.Count; n++)
			{
				int nextIndex = (n+1) % NeighborCells.Count;

				VoronoiNeighbor neighborCellCopy		= NeighborCells[n];
				VoronoiNeighbor nextNeighborCellCopy	= NeighborCells[nextIndex];

				if (neighborCellCopy.WasClamped && nextNeighborCellCopy.WasClamped)
				{
					Edge currentEdge	= neighborCellCopy.EdgeToNeighbor;
					Edge nextEdge		= NeighborCells[nextIndex].EdgeToNeighbor;

					if (currentEdge.End.x == nextEdge.Start.x || currentEdge.End.y == nextEdge.Start.y)
					{
						VoronoiNeighbor newVoronoiNeighbor;
						newVoronoiNeighbor.NeighborIndex = -1;
						newVoronoiNeighbor.WasClamped    = true;

						// add simple edge
						newVoronoiNeighbor.EdgeToNeighbor = new Edge(currentEdge.End, nextEdge.Start);
						NeighborCells.Add(newVoronoiNeighbor);
					}
					else 
					{
						float verticalBorder;
						float horizontalBorder;

						if (currentEdge.End.x == MIN_COORDS.x || nextEdge.Start.x == MIN_COORDS.x)
						{
							horizontalBorder = MIN_COORDS.x;
						}
						else if (currentEdge.End.x == MAX_COORDS.x || nextEdge.Start.x == MAX_COORDS.x)
						{
							horizontalBorder = MAX_COORDS.x;
						}
						else
						{
							Debug.Assert(false, "Unexpected case on BorderRect Corner");
							break;
						}

						if (currentEdge.End.y == MIN_COORDS.y || nextEdge.Start.y == MIN_COORDS.y)
						{
							verticalBorder = MIN_COORDS.y;
						}
						else if (currentEdge.End.y == MAX_COORDS.y || nextEdge.Start.y == MAX_COORDS.y)
						{
							verticalBorder = MAX_COORDS.y;
						}
						else
						{
							Debug.Assert(false, "Unexpected case on BorderRect Corner");
							break;
						}

						Vector2 corner = new Vector2(horizontalBorder, verticalBorder);
						Edge newEdge1 = new Edge(currentEdge.End, corner);
						Edge newEdge2 = new Edge(corner, nextEdge.Start);

						VoronoiNeighbor newVoronoiNeighbor1;
						newVoronoiNeighbor1.NeighborIndex = -1;
						newVoronoiNeighbor1.WasClamped    = true;
						newVoronoiNeighbor1.EdgeToNeighbor = newEdge1;

						VoronoiNeighbor newVoronoiNeighbor2;
						newVoronoiNeighbor2.NeighborIndex = -1;
						newVoronoiNeighbor2.WasClamped    = true;
						newVoronoiNeighbor2.EdgeToNeighbor = newEdge2;

						NeighborCells.Add(newVoronoiNeighbor1);
						NeighborCells.Add(newVoronoiNeighbor2);
					}

					break;
				} //< if
			} // < for all cells

			if (oldCount != NeighborCells.Count)
			{
				// Before adding the border edges, the polygon could have consisted of only two edges, meaning there is no real direction before
				SortEdgesCCW(true);
			}

		} // < end addClampEdge method

	}

	public class VoronoiDiagram 
	{
		private List<Vector2> InputVerticesIncludingSuperTriangle;		
		private List<Triangle> DelauneyTrianglesIncludingSuperTriangle;
		private List<VoronoiCell> VoronoiCells;
		private int		POINT_COUNT_WITHOUT_SUPER_TRIANGLE;
		private Vector2 MIN_COORDS;
		private Vector2 MAX_COORDS;
		private Vector2 DIMENSIONS;
		private Triangle SuperTriangle;
		private VoronoiParameters VoronoiParams;
		private bool WasRunAtLeastOnce = false;

		// Bowyer-Watson Algorithm for Delauny-Triangulation
		public bool GenerateDelauney(int seedPoints, VoronoiParameters voronoiParams, Vector2 gridCenterWS, Vector2 gridSizeWS)
		{
			// TODO: Am anfang alle input point überlappungen verhindern
			// TODO: Relaxation
			// TODO: Alle Early outs (inkl. Exceptions) abfangen und neugenerierung anstossen
			// TODO: Am Ende (nach relaxation) Sanity check

			WasRunAtLeastOnce = true;

			InputVerticesIncludingSuperTriangle		= new List<Vector2>();
			DelauneyTrianglesIncludingSuperTriangle = new List<Triangle>();
			VoronoiCells							= new List<VoronoiCell>();

			VoronoiParams = voronoiParams;

			// 1) Get Random Point List.
			Random.InitState(seedPoints);

			MIN_COORDS = gridCenterWS - gridSizeWS;
			MAX_COORDS = gridCenterWS + gridSizeWS;
			DIMENSIONS = MAX_COORDS - MIN_COORDS;

			// Distribute ~2/3 of the points p to a regular grid of size y,x
			// x   *   y = p/2
			// r*y *   y = p/2		(r = x/y)
			// y = sqrt(p/(2*r))
			// x = h*y;
			POINT_COUNT_WITHOUT_SUPER_TRIANGLE = voronoiParams.VoronoiPointCount;



			bool useHardcodedPoints = false;

			if (useHardcodedPoints)
			{
				InputVerticesIncludingSuperTriangle.Add( new Vector2(-2.2f, 7.9f)		)	;
				InputVerticesIncludingSuperTriangle.Add( new Vector2(-7.5f, -4.2f)		)	;
				InputVerticesIncludingSuperTriangle.Add( new Vector2(0.8f, 3.3f)		)	;
				POINT_COUNT_WITHOUT_SUPER_TRIANGLE = InputVerticesIncludingSuperTriangle.Count;
			}


			float ratioXbyY = DIMENSIONS.x / DIMENSIONS.y;
			float pseudoRandomGridDimensionY_F = Mathf.Sqrt((POINT_COUNT_WITHOUT_SUPER_TRIANGLE * 2 / 3));
			float pseudoRandomGridDimensionX_F = pseudoRandomGridDimensionY_F * ratioXbyY;
			int pseudoRandomGridDimensionX = (int) Mathf.Max(pseudoRandomGridDimensionX_F, 1);
			int pseudoRandomGridDimensionY = (int) Mathf.Max(pseudoRandomGridDimensionY_F, 1);
	
			int pseudoRandomGridPointCount = pseudoRandomGridDimensionX * pseudoRandomGridDimensionX;

			Vector2 PSEUDO_RANDOM_GRID_CELL_SIZE = new Vector2(DIMENSIONS.x / (float)pseudoRandomGridDimensionX, DIMENSIONS.y / (float)pseudoRandomGridDimensionY);

			for (int i = 0; i < POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++i)
			{
				if (useHardcodedPoints)
				{
					break;
				}

				float x;
				float y;

				if (i < pseudoRandomGridPointCount)
				{
					// randomly distribute within grid cell
					int gridCellX = i % pseudoRandomGridDimensionX;
					int gridCellY = i / pseudoRandomGridDimensionX;

					x = Random.Range(MIN_COORDS.x + gridCellX * PSEUDO_RANDOM_GRID_CELL_SIZE.x, MIN_COORDS.x + (gridCellX + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.x);
					y = Random.Range(MIN_COORDS.y + gridCellY * PSEUDO_RANDOM_GRID_CELL_SIZE.y, MIN_COORDS.y + (gridCellY + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.y);
				}
				else
				{
					// randomly distribute
					x = Random.Range(MIN_COORDS.x, MAX_COORDS.x);
					y = Random.Range(MIN_COORDS.y, MAX_COORDS.y);
				}

				x = Mathf.Clamp(x, MIN_COORDS.x + 0.02f * DIMENSIONS.x, MAX_COORDS.x - 0.02f * DIMENSIONS.x);
				y = Mathf.Clamp(y, MIN_COORDS.y + 0.02f * DIMENSIONS.y, MAX_COORDS.y - 0.02f * DIMENSIONS.y);
				InputVerticesIncludingSuperTriangle.Add(new Vector2(x,y));
			}

			// Randomly shuffle points to test algorithm if its really delauny triangulation (independent of input point order)
			Random.InitState(voronoiParams.ShuffleSeed);

			for (PointIndex i = 0; i < POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++i)
			{
				PointIndex swapWithIndex = Random.Range(0, POINT_COUNT_WITHOUT_SUPER_TRIANGLE);
				Vector2 oldPoint = InputVerticesIncludingSuperTriangle[i];
				InputVerticesIncludingSuperTriangle[i] = InputVerticesIncludingSuperTriangle[swapWithIndex];
				InputVerticesIncludingSuperTriangle[swapWithIndex] = oldPoint;
			}

			// 2) Get Super Triangle
			float maxDimension = Mathf.Max(DIMENSIONS.x, DIMENSIONS.y);
			Vector2 superTriangleP1 = new Vector2(MIN_COORDS.x - maxDimension, MIN_COORDS.y - maxDimension);
			Vector2 superTriangleP2 = superTriangleP1 + new Vector2(0, maxDimension * 5);
			Vector2 superTriangleP3 = superTriangleP1 + new Vector2(maxDimension * 5, 0);
				
			PointIndex SUPER_TRIANGLE_INDEX_P1 = POINT_COUNT_WITHOUT_SUPER_TRIANGLE;
			PointIndex SUPER_TRIANGLE_INDEX_P2 = POINT_COUNT_WITHOUT_SUPER_TRIANGLE + 1;
			PointIndex SUPER_TRIANGLE_INDEX_P3 = POINT_COUNT_WITHOUT_SUPER_TRIANGLE + 2;
			InputVerticesIncludingSuperTriangle.Add(superTriangleP1);
			InputVerticesIncludingSuperTriangle.Add(superTriangleP2);
			InputVerticesIncludingSuperTriangle.Add(superTriangleP3);

			// 3) Start algorithm. (Our input list looks like this: [i1, i2, i3, i4, ... s1, s2, s3]		
			bool superTriangleValid = SuperTriangle.TryInit(SUPER_TRIANGLE_INDEX_P1, SUPER_TRIANGLE_INDEX_P2, SUPER_TRIANGLE_INDEX_P3, InputVerticesIncludingSuperTriangle);

			if (!superTriangleValid)
			{
				Debug.Log("Corrupt Supertriangle. Aborting Delauney Computation.");
				return false;
			}

			DelauneyTrianglesIncludingSuperTriangle.Add(SuperTriangle);
			
			for (int p = 0; p < POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++p) 
			{
				Vector2 currentPoint = InputVerticesIncludingSuperTriangle[p];

				List<TriIndex> deleteTrianglesSorted = new List<TriIndex>();

				// Find all Traingles, in which circle we lie
				for (TriIndex t = 0; t < DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
				{
					Triangle currentTriangle = DelauneyTrianglesIncludingSuperTriangle[t];

					if (currentTriangle.CircumscribedCircle.IsPointInside(currentPoint))
					{
						deleteTrianglesSorted.Add(t);	
					}
				}

				List<EdgeIndices> edgesForNewTriangles = new List<EdgeIndices>();

				// Find all Edges that are not shared between badies
				for (int dT = 0; dT < deleteTrianglesSorted.Count; ++dT)
				{
					Triangle currentTriangle = DelauneyTrianglesIncludingSuperTriangle[deleteTrianglesSorted[dT]];

					EdgeIndices edge1 = new EdgeIndices(currentTriangle.IndexP0, currentTriangle.IndexP1);
					EdgeIndices edge2 = new EdgeIndices(currentTriangle.IndexP1, currentTriangle.IndexP2);
					EdgeIndices edge3 = new EdgeIndices(currentTriangle.IndexP0, currentTriangle.IndexP2);

					bool edge1Shared = false;
					bool edge2Shared = false;
					bool edge3Shared = false;

					for (int dT2 = 0; dT2 < deleteTrianglesSorted.Count; ++dT2)
					{
						if (dT == dT2)
						{
							continue;
						}

						Triangle otherTriangle = DelauneyTrianglesIncludingSuperTriangle[deleteTrianglesSorted[dT2]];

						if (!edge1Shared && otherTriangle.SharesEdge(edge1))
						{
							edge1Shared = true;
						}
						if (!edge2Shared && otherTriangle.SharesEdge(edge2))
						{
							edge2Shared = true;
						}
						if (!edge3Shared && otherTriangle.SharesEdge(edge3))
						{
							edge3Shared = true;
						}
					}

					if (!edge1Shared)
					{
						edgesForNewTriangles.Add(edge1);
					}
					if (!edge2Shared)
					{
						edgesForNewTriangles.Add(edge2);
					}
					if (!edge3Shared)
					{
						edgesForNewTriangles.Add(edge3);
					}
				}

				// Delete all bad triangles
				for (int dt = deleteTrianglesSorted.Count - 1; dt >= 0; --dt)
				{
					int indexToDelete = deleteTrianglesSorted[dt];
					DelauneyTrianglesIncludingSuperTriangle.RemoveAt(indexToDelete);
				}

				// Add new triangles for given edges
				for (int e = 0; e < edgesForNewTriangles.Count; ++e)
				{
					EdgeIndices currentEdge = edgesForNewTriangles[e];
					Triangle newTriangle = new Triangle();
					bool initSuccessfull = newTriangle.TryInit(currentEdge.IndexP1, currentEdge.IndexP2, p, InputVerticesIncludingSuperTriangle);

					if (!initSuccessfull)
					{
						Debug.Log("Could not compute Circumscribed circle. Aborting Delauney Computation.");
						return false;
					}

					DelauneyTrianglesIncludingSuperTriangle.Add(newTriangle);
				}
			}

			/*

			// Remove all triangles that share a vertex with super triangle	
			for (int t = outTriangles.Count - 1; t >= 0; --t)
			{
				if (outTriangles[t].SharesPointWith(superTriangle))
				{
					outTriangles.RemoveAt(t);
				}
			}

			// Remove super-triangle vertices
			inputPointList.RemoveAt(SUPER_TRIANGLE_INDEX_P3);
			inputPointList.RemoveAt(SUPER_TRIANGLE_INDEX_P2);
			inputPointList.RemoveAt(SUPER_TRIANGLE_INDEX_P1);*/

			bool voronoiSuccess = DelauneyToVoronoi();

			return voronoiSuccess;
		}
	
		// -------------------------------------------------------------------

		public bool IsOutsideClampRect(Vector2 point)
		{
			return (point.x < MIN_COORDS.x) || (point.y < MIN_COORDS.y) || (point.x > MAX_COORDS.x) || (point.y > MAX_COORDS.y);
		}

		// -------------------------------------------------------------------

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
			
		/*	newPoint.x = Mathf.Max(newPoint.x, clampRectMin.x);
			newPoint.x = Mathf.Min(newPoint.x, clampRectMax.x);
			newPoint.y = Mathf.Max(newPoint.y, clampRectMin.y);
			newPoint.y = Mathf.Min(newPoint.y, clampRectMax.y);*/

			return newPoint;
		}

		// -------------------------------------------------------------------

		Vector2 MoveEdgeCenterToOuterBorder(EdgeIndices sharedEdge, Vector2 pointInMoveDirection, Vector2 clampRectMin, Vector2 clampRectMax, List<Vector2> delauneyVerticesIncludingSuperTriangle)
		{
			Vector2 edgeP1 = delauneyVerticesIncludingSuperTriangle[sharedEdge.IndexP1];
			Vector2 edgeP2 = delauneyVerticesIncludingSuperTriangle[sharedEdge.IndexP2];
			Vector2 edgeCenter = ((edgeP1 + edgeP2) * 0.5f);

			Vector2 awayFromEdge = pointInMoveDirection - edgeCenter;
			Vector2 edgeDirection = edgeP2 - edgeP1;
			Vector2 moveDirection = new Vector2(edgeDirection.y, -edgeDirection.x);
			if (Vector2.Dot(moveDirection, awayFromEdge) < 0.0f)
			{
				moveDirection = -moveDirection;
			}

			float moveDirectionLength = moveDirection.magnitude;
			
			if (moveDirectionLength == 0.0f)
			{
				Debug.Assert(false, "Degenerated Triangle");
				return pointInMoveDirection;
			}

			moveDirection.x /= moveDirectionLength;
			moveDirection.y /= moveDirectionLength;

			return ClampToBorder(edgeCenter, moveDirection, clampRectMin, clampRectMax);
		}

		// -------------------------------------------------

		public Edge ComputeElongatedVoronoiNeighbor(TriIndex innerTriIndex, TriIndex outerTriIndex, List<Vector2> triCenters, Vector2 MIN_COORDS, Vector2 MAX_COORDS)
		{
			Vector2 innerCenter = triCenters[innerTriIndex];
			Vector2 outerCenter = triCenters[outerTriIndex];

			Vector2 edgeDirection = outerCenter - innerCenter;

			float edgeDirectionLength = edgeDirection.magnitude;
			
			if (edgeDirectionLength == 0.0f)
			{
				Debug.Assert(false, "Degenerated Edge");
				return new Edge(new Vector2(0,0), new Vector2(0,0));
			}

			edgeDirection.x /= edgeDirectionLength;
			edgeDirection.y /= edgeDirectionLength;

			Vector2 newEdgeEnd = ClampToBorder(innerCenter, edgeDirection, MIN_COORDS, MAX_COORDS);

			return new Edge(innerCenter, newEdgeEnd);
		}

		// -------------------------------------------------

		public Edge ComputeElongatedVoronoiNeighbor(Vector2 newEdgeStartPos, EdgeIndices perpendicularEdge, Vector2 innerPoint, List<Vector2> delauneyVerticesIncludingSuperTriangle, Vector2 MIN_COORDS, Vector2 MAX_COORDS)
		{
			Vector2 edgeP1 = delauneyVerticesIncludingSuperTriangle[perpendicularEdge.IndexP1];
			Vector2 edgeP2 = delauneyVerticesIncludingSuperTriangle[perpendicularEdge.IndexP2];
			Vector2 edgeCenter = ((edgeP1 + edgeP2) * 0.5f);

			Vector2 awayFromEdge = edgeCenter - innerPoint;

			Vector2 edgeDirection = edgeP2 - edgeP1;
			Vector2 moveDirection = new Vector2(edgeDirection.y, -edgeDirection.x);

			if (Vector2.Dot(moveDirection, awayFromEdge) < 0.0f)
			{
				moveDirection = -moveDirection;
			}

			float moveDirectionLength = moveDirection.magnitude;
			
			if (moveDirectionLength == 0.0f)
			{
				Debug.Assert(false, "Degenerated Triangle");
				return new Edge(edgeCenter, edgeCenter);
			}

			moveDirection.x /= moveDirectionLength;
			moveDirection.y /= moveDirectionLength;

			Vector2 newEdgeEnd = ClampToBorder(newEdgeStartPos, moveDirection, MIN_COORDS, MAX_COORDS);
			return new Edge(newEdgeStartPos, newEdgeEnd);
		}

		// -------------------------------------------------

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

			return ClampToBorder(insideEdgeStart, moveDirection, MIN_COORDS, MAX_COORDS);
		}

		// -------------------------------------------------

		public bool DelauneyToVoronoi()
		{
			int pointCountWithoutSuperTriangle = InputVerticesIncludingSuperTriangle.Count - 3;

			VoronoiCells.Capacity = pointCountWithoutSuperTriangle;

			for (PointIndex p = 0; p < pointCountWithoutSuperTriangle; ++p)
			{
				VoronoiCells.Add(new VoronoiCell());
			} 

			// Register All Edges & Centers
			Dictionary<EdgeKey, TwoTriangleIndices> edgesToTriangleIndices = new Dictionary<long, TwoTriangleIndices>();
			List<Vector2> triangleVoronoiCenters = new List<Vector2>();
			triangleVoronoiCenters.Capacity = DelauneyTrianglesIncludingSuperTriangle.Count;

			for (TriIndex t = 0; t < DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
			{
				Triangle currentTriangle = DelauneyTrianglesIncludingSuperTriangle[t];

				// Note: We do not use circle center (as for a usual "Voronoi" Diagram, but the centroid, thus getting the barycentric dual mesh)
				// See http://www.redblobgames.com/x/1721-voronoi-alternative/
				Vector2 center = VoronoiParams.RealVoronoi ? currentTriangle.CircumscribedCircle.Center : currentTriangle.GetCentroid(InputVerticesIncludingSuperTriangle);	
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
			for (TriIndex t = 0; t < DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
			{
				// Tell every VoronoiCell (DelauneyVertex), which HullVertices it has
				Triangle currentTriangle = DelauneyTrianglesIncludingSuperTriangle[t];

				bool currentTriangleContainsSuperVertex = currentTriangle.SharesPointWith(SuperTriangle);
				Vector2 currentTriangleCenter = triangleVoronoiCenters[t];

				for (int side = 0; side < 3; ++side)
				{
					EdgeIndices curEdgeIndices = currentTriangle.GetEdgeOppositeTo(side);
					Vector2  curOppositeVertex = InputVerticesIncludingSuperTriangle[currentTriangle.GetIndex(side)];

					TwoTriangleIndices neighborTriangles = edgesToTriangleIndices[curEdgeIndices.ComputeKey()];

					TriIndex otherTriangleIndex = neighborTriangles.GetIndexUnequalTo(t);

					if (otherTriangleIndex < t)
					{
						// already processed this pair
						continue;
					}

					Triangle otherTriangle = DelauneyTrianglesIncludingSuperTriangle[otherTriangleIndex];

					bool otherTriangleContainsSuperVertex = otherTriangle.SharesPointWith(SuperTriangle);
					Vector2 otherTriangleCenter = triangleVoronoiCenters[otherTriangleIndex];

					Edge edgeForNeighbors = new Edge(currentTriangleCenter, otherTriangleCenter);
						
					if (curEdgeIndices.IndexP1 < VoronoiCells.Count)
					{
						VoronoiNeighbor neighbor1 = new VoronoiNeighbor(curEdgeIndices.IndexP2, edgeForNeighbors);
						VoronoiCells[curEdgeIndices.IndexP1].NeighborCells.Add(neighbor1);
					}
					if (curEdgeIndices.IndexP2 < VoronoiCells.Count)
					{
						VoronoiNeighbor neighbor2 = new VoronoiNeighbor(curEdgeIndices.IndexP1, edgeForNeighbors);
						VoronoiCells[curEdgeIndices.IndexP2].NeighborCells.Add(neighbor2);	
					}

				}
			}

			List<int> indicesToRemove = new List<int>();

			// Remove and Clamp edges
			for (PointIndex c = 0; c < VoronoiCells.Count; ++c)
			{
				if (VoronoiParams.SuppressClamping)
				{
					break;
				}

				indicesToRemove.Clear();
				VoronoiCell currentCell = VoronoiCells[c];
				for (int n = 0; n < currentCell.NeighborCells.Count; ++n)
				{
					VoronoiNeighbor neighborCopy = currentCell.NeighborCells[n];

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
						currentCell.NeighborCells[n] = neighborCopy;
					}	
				}

				for (int r = indicesToRemove.Count - 1; r >= 0; --r)
				{
					currentCell.NeighborCells.RemoveAt(indicesToRemove[r]);
				}

				if (indicesToRemove.Count > 0)
				{
					currentCell.CalculateCentroid();
				}
			}

			// Sort CCW and force edge directions
			for (PointIndex c = 0; c < VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = VoronoiCells[c];
				Debug.Assert(VoronoiCells.Count >= 3);
				
				currentCell.SortEdgesCCW(true);
			}

			// Add Edges at clamp rect
			for (PointIndex c = 0; c < VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = VoronoiCells[c];
				
				if (VoronoiParams.SuppressNewBorderEdges)
				{
					break;
				}

				currentCell.AddClampRectEdgesToFillOpenPolygon(MIN_COORDS, MAX_COORDS);
			}

			// Calculate Centroid
			for (PointIndex c = 0; c < VoronoiCells.Count; ++c)
			{
				VoronoiCell currentCell = VoronoiCells[c];
				currentCell.CalculateCentroid();
			}

			return true;
		}

		// -------------------------------------------------------------------

		static public bool FindCircumscribedCircle(Vector2 r, Vector2 s, Vector2 t, out Circle circle)
		{
			// point of equi-distance: (r and s)
			// (rx-x)^2 + (ry-y)^2 = (sx-x)^2 + (sy-y)^2
			// rx^2 -2rx*x + ry^2 - 2ry*y = sx^2 - 2*sx*x + sy^2 - 2*sy*y
			// x(-2rx + 2*sx) = - rx^2 - ry^2 + 2ry*y + sx^2 + sy^2 - 2*sy*y
			// x = (sx^2 + sy^2 + 2ry*y - 2sy*y - rx^2 - ry^2) / ( 2sx - 2*rx )

			// point of equi-distance: (s and t)
			// x = (sx^2 + sy^2 + 2ty*y - 2sy*y - tx^2 - ty^2) / ( 2sx - 2*tx )

			// point of complete equi-distance:
			// x = x
			// y = rx^2*sx - ry^2*sx - sx^2*tx - sy^2*tx + rx^2*tx + ry^2*tx + tx^2*sx + ty^2*sx + sx^2*sx + sy^2*rx - tx^2*rx - ty^2*rx / 2*(ry*sx - ry*tx + sy*tx - ty*sx + ty*rx - sy*rx)

			float divisor1 = (2*(r.x*(s.y-t.y)-r.y*(s.x-t.x)+s.x*t.y-t.x*s.y));
			
			if (divisor1 == 0)
			{
				circle.Center = (r + s + t) / 3.0f;
				circle.Radius = Vector2.Distance(r, circle.Center);
				return false;
			}
				
			float x = ((r.x*r.x + r.y*r.y)*(s.y-t.y)+(s.x*s.x+s.y*s.y)*(t.y-r.y)+(t.x*t.x+t.y*t.y)*(r.y-s.y)) / divisor1;
			float y = ((r.x*r.x + r.y*r.y)*(t.x-s.x)+(s.x*s.x+s.y*s.y)*(r.x-t.x)+(t.x*t.x+t.y*t.y)*(s.x-r.x)) / divisor1;

			circle.Center = new Vector2(x,y);
			circle.Radius = Vector2.Distance(r, circle.Center);
			return true;
		}

		public void DebugDraw(VoronoiParameters voronoiParams)
		{
			if (!WasRunAtLeastOnce)
			{
				return; 
			} 

			float debugDrawHeight = 1.0f;

			float triangleOffsetLength = 0.05f * ((DIMENSIONS.x + DIMENSIONS.y) / POINT_COUNT_WITHOUT_SUPER_TRIANGLE);
			triangleOffsetLength = Mathf.Lerp(triangleOffsetLength, 0.1f, 0.9f);

			if (voronoiParams.ShowDelauney)
			{
				// Draw all Triangles:
				for (TriIndex t = 0; t < DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
				{
					Triangle currentTriangle = DelauneyTrianglesIncludingSuperTriangle[t];

					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && !currentTriangle.HasAsVertex(voronoiParams.DebugDrawOnlyVertexIndex))
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1 && t != voronoiParams.DebugDrawOnlyTriangleIndex)
					{
						continue;
					}

					Vector3 p1 = new Vector3(InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].x, debugDrawHeight, InputVerticesIncludingSuperTriangle[currentTriangle.IndexP0].y);
					Vector3 p2 = new Vector3(InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].x, debugDrawHeight, InputVerticesIncludingSuperTriangle[currentTriangle.IndexP1].y);
					Vector3 p3 = new Vector3(InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].x, debugDrawHeight, InputVerticesIncludingSuperTriangle[currentTriangle.IndexP2].y);

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
					int sharedPointCount = currentTriangle.SharedPointCount(SuperTriangle, out a1, out a2, out a3);
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
				for (PointIndex c = 0; c < VoronoiCells.Count; ++c)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && c != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					VoronoiCell currentCell = VoronoiCells[c];
					Color cellBaseColor = new Color(c * 0.123f % 1.0f, c * 0.311f % 1.0f, c * 0.76f % 1.0f);

					for (int p = 0; p < currentCell.NeighborCells.Count; ++p)
					{
						VoronoiNeighbor neighbor = currentCell.NeighborCells[p];

						if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
						{
							EdgeIndices currentEdgeIndices = new EdgeIndices(c, neighbor.NeighborIndex);
							Triangle focusTriangle = DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
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

				Gizmos.DrawLine(new Vector3(MIN_COORDS.x, debugDrawHeight, MIN_COORDS.y), new Vector3(MIN_COORDS.x, debugDrawHeight, MAX_COORDS.y));
				Gizmos.DrawLine(new Vector3(MIN_COORDS.x, debugDrawHeight, MAX_COORDS.y), new Vector3(MAX_COORDS.x, debugDrawHeight, MAX_COORDS.y));
				Gizmos.DrawLine(new Vector3(MAX_COORDS.x, debugDrawHeight, MAX_COORDS.y), new Vector3(MAX_COORDS.x, debugDrawHeight, MIN_COORDS.y));
				Gizmos.DrawLine(new Vector3(MAX_COORDS.x, debugDrawHeight, MIN_COORDS.y), new Vector3(MIN_COORDS.x, debugDrawHeight, MIN_COORDS.y));
			}

			if (voronoiParams.ShowPoints)
			{
				Color col = new Color(1.0f, 1.0f, 0.0f);
				Gizmos.color = new Color(col.r, col.g, col.b, 0.5f);

				for (int p = 0; p < InputVerticesIncludingSuperTriangle.Count; ++p)
				{
					if (voronoiParams.DebugDrawOnlyVertexIndex != -1 && p != voronoiParams.DebugDrawOnlyVertexIndex)
					{
						continue;
					}

					if (voronoiParams.DebugDrawOnlyTriangleIndex != -1)
					{
						Triangle focusTriangle = DelauneyTrianglesIncludingSuperTriangle[voronoiParams.DebugDrawOnlyTriangleIndex];
						if (!focusTriangle.HasAsVertex(p))
						{
							continue;
						}
					}


					Vector2 pos = InputVerticesIncludingSuperTriangle[p];

					Gizmos.DrawSphere(new Vector3(pos.x, debugDrawHeight, pos.y), triangleOffsetLength * 2.0f);

					if (voronoiParams.ShowIndices)
					{
						DrawText(pos, debugDrawHeight, col, p.ToString());
					}	
				}
			}
		}

		static void DrawText(Vector2 pos2, float height, Color col, string text)
		{
			DrawText(new Vector3(pos2.x, height, pos2.y), col, text);
		}

		static void DrawText(Vector3 pos3, Color col, string text)
		{
			GUIStyle style = new GUIStyle();
			style.normal.textColor = col;
			UnityEditor.Handles.Label (pos3, text, style);
		}
	}

}