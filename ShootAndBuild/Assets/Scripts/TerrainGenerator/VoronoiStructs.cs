using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey		= System.Int64;
using PointIndex	= System.Int32;
using TriIndex		= System.Int32;

namespace SAB
{
	public class VoronoiCreationState
	{
		public List<Vector2> InputVerticesIncludingSuperTriangle		= new List<Vector2>();		
		public List<Triangle> DelauneyTrianglesIncludingSuperTriangle	= new List<Triangle>();
		public List<VoronoiCell> VoronoiCells							= new List<VoronoiCell>();
		
		public int		POINT_COUNT_WITHOUT_SUPER_TRIANGLE				= -1;
		public Vector2 MIN_COORDS										= new Vector2(0,0);
		public Vector2 MAX_COORDS										= new Vector2(0,0);
		public Vector2 DIMENSIONS										= new Vector2(0,0);
		public Triangle SuperTriangle									= new Triangle();
		public VoronoiParameters VoronoiParams							= new VoronoiParameters();
	}

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

		public Circle CircumscribedCircle;

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

		public EdgeIndices GetEdgeOppositeTo(int i)
		{
			return new EdgeIndices(GetIndex((i + 1) % 3), GetIndex((i + 2) % 3));
		}


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

			return FindCircumscribedCircle(p0,p1,p2, out CircumscribedCircle);
		}

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

		public bool AddClampRectEdgesToFillOpenPolygon(Vector2 MIN_COORDS, Vector2 MAX_COORDS)
		{
			int oldCount = NeighborCells.Count;
			bool errorHappened = false;

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
							errorHappened = true;
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
							errorHappened = true;
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

			return !errorHappened;
		} // < end addClampEdge method

	}
}