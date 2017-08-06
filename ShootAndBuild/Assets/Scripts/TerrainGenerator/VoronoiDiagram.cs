using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

	public struct Edge
	{
		public int IndexP1;
		public int IndexP2;

		public Edge(int indexP1, int indexP2)
		{
			IndexP1 = indexP1;
			IndexP2 = indexP2;
		}
	}

	public struct Triangle
	{
		public int IndexP1;
		public int IndexP2;
		public int IndexP3;

		public Circle CircumscribedCircle;

		public bool SharesEdge(Edge edge)
		{
			return (((edge.IndexP1 == IndexP1) || (edge.IndexP1 == IndexP2) || (edge.IndexP1 == IndexP3)) && 
					((edge.IndexP2 == IndexP1) || (edge.IndexP2 == IndexP2) || (edge.IndexP2 == IndexP3)));
		}

		public bool SharesPointWith(Triangle other)
		{
			return (((IndexP1 == other.IndexP1) || (IndexP2 == other.IndexP1) || (IndexP3 == other.IndexP1)) ||
					((IndexP1 == other.IndexP2) || (IndexP2 == other.IndexP2) || (IndexP3 == other.IndexP2)) ||
					((IndexP1 == other.IndexP3) || (IndexP2 == other.IndexP3) || (IndexP3 == other.IndexP3)));
		}

		public bool TryInit(int indexP1, int indexP2, int indexP3, List<Vector2> pointList)
		{
			IndexP1 = indexP1;
			IndexP2 = indexP2;
			IndexP3 = indexP3;

			bool success = CalculateCircumscribedCircle(pointList);
			return success;
		}

		bool CalculateCircumscribedCircle(List<Vector2> pointList)
		{
			Vector2 p1 = pointList[IndexP1];
			Vector2 p2 = pointList[IndexP2];
			Vector2 p3 = pointList[IndexP3];

			return VoronoiDiagram.FindCircumscribedCircle(p1,p2,p3, out CircumscribedCircle);
		}
	}

	public class VoronoiDiagram 
	{
		// Bowyer-Watson Algorithm for Delauny-Triangulation
		public bool GenerateDelauney(int seedPoints, VoronoiParameters voronoiParams, Vector2 gridCenterWS, Vector2 gridSizeWS)
		{
			// TODO: Am Ende (nach relaxation) Sanity check (z.b. dass keine points aufeinander liegen)

			Vector2 c = new Vector2(13,-18);
			Vector2 d1 = new Vector2(2, 2); d1.Normalize();
			Vector2 d2 = new Vector2(2, 3); d2.Normalize();
			Vector2 d3 = new Vector2(2, 4); d3.Normalize();

			List <Vector2> testList = new List<Vector2>();
			testList.Add(c + d1);
			testList.Add(c + d2);
			testList.Add(c + d3);
			
			Triangle t1 = new Triangle();
			t1.TryInit(0,1,2, testList);


			// 1) Get Random Point List.
			List<Vector2> inputPointList = new List<Vector2>();
			Random.InitState(seedPoints);

			Vector2 MIN_COORDS = gridCenterWS - gridSizeWS;
			Vector2 MAX_COORDS = gridCenterWS + gridSizeWS;
			Vector2 DIMENSIONS = MAX_COORDS - MIN_COORDS;

			// Distribute ~2/3 of the points p to a regular grid of size y,x
			// x   *   y = p/2
			// r*y *   y = p/2		(r = x/y)
			// y = sqrt(p/(2*r))
			// x = h*y;
			int POINT_COUNT = voronoiParams.VoronoiPointCount;
			float ratioXbyY = DIMENSIONS.x / DIMENSIONS.y;
			float pseudoRandomGridDimensionY_F = Mathf.Sqrt((POINT_COUNT * 2 / 3));
			float pseudoRandomGridDimensionX_F = pseudoRandomGridDimensionY_F * ratioXbyY;
			int pseudoRandomGridDimensionX = (int) Mathf.Max(pseudoRandomGridDimensionX_F, 1);
			int pseudoRandomGridDimensionY = (int) Mathf.Max(pseudoRandomGridDimensionY_F, 1);
	
			int pseudoRandomGridPointCount = pseudoRandomGridDimensionX * pseudoRandomGridDimensionX;

			Vector2 PSEUDO_RANDOM_GRID_CELL_SIZE = new Vector2(DIMENSIONS.x / (float)pseudoRandomGridDimensionX, DIMENSIONS.y / (float)pseudoRandomGridDimensionY);

			for (int i = 0; i < POINT_COUNT; ++i)
			{
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
				inputPointList.Add(new Vector2(x,y));
			}

			// Randomly shuffle points to test algorithm if its really delauny triangulation (independent of input point order)
			Random.InitState(voronoiParams.ShuffleSeed);

			for (int i = 0; i < POINT_COUNT; ++i)
			{
				int swapWithIndex = Random.Range(0, POINT_COUNT);
				Vector2 oldPoint = inputPointList[i];
				inputPointList[i] = inputPointList[swapWithIndex];
				inputPointList[swapWithIndex] = oldPoint;
			}

			// 2) Get Super Triangle
			float maxDimension = Mathf.Max(DIMENSIONS.x, DIMENSIONS.y);
			Vector2 superTriangleP1 = new Vector2(MIN_COORDS.x - maxDimension, MIN_COORDS.y - maxDimension);
			Vector2 superTriangleP2 = superTriangleP1 + new Vector2(0, maxDimension * 5);
			Vector2 superTriangleP3 = superTriangleP1 + new Vector2(maxDimension * 5, 0);
				
			int SUPER_TRIANGLE_INDEX_P1 = POINT_COUNT;
			int SUPER_TRIANGLE_INDEX_P2 = POINT_COUNT + 1;
			int SUPER_TRIANGLE_INDEX_P3 = POINT_COUNT + 2;
			inputPointList.Add(superTriangleP1);
			inputPointList.Add(superTriangleP2);
			inputPointList.Add(superTriangleP3);

			// 3) Start algorithm. (Our input list looks like this: [i1, i2, i3, i4, ... s1, s2, s3]
			List<Triangle> outTriangles = new List<Triangle>();
			Triangle superTriangle = new Triangle();
			bool superTriangleValid = superTriangle.TryInit(SUPER_TRIANGLE_INDEX_P1, SUPER_TRIANGLE_INDEX_P2, SUPER_TRIANGLE_INDEX_P3, inputPointList);

			if (!superTriangleValid)
			{
				Debug.Log("Corrupt Supertriangle. Aborting Delauney Computation.");
				return false;
			}

			outTriangles.Add(superTriangle);

			int pointsToAddCount = POINT_COUNT;
			if (voronoiParams.DebugDrawDelauney && voronoiParams.DebugSteps > 0)
			{
				pointsToAddCount = Mathf.Max(pointsToAddCount, voronoiParams.DebugSteps);
			}

			for (int p = 0; p < POINT_COUNT; ++p)
			{
				Vector2 currentPoint = inputPointList[p];

				List<int> deleteTrianglesSorted = new List<int>();

				// Find all Traingles, in which circle we lie
				for (int t = 0; t < outTriangles.Count; ++t)
				{
					Triangle currentTriangle = outTriangles[t];

					if (currentTriangle.CircumscribedCircle.IsPointInside(currentPoint))
					{
						deleteTrianglesSorted.Add(t);	
					}
				}

				List<Edge> edgesForNewTriangles = new List<Edge>();

				// Find all Edges that are not shared between badies
				for (int dT = 0; dT < deleteTrianglesSorted.Count; ++dT)
				{
					Triangle currentTriangle = outTriangles[deleteTrianglesSorted[dT]];

					Edge edge1 = new Edge(currentTriangle.IndexP1, currentTriangle.IndexP2);
					Edge edge2 = new Edge(currentTriangle.IndexP2, currentTriangle.IndexP3);
					Edge edge3 = new Edge(currentTriangle.IndexP1, currentTriangle.IndexP3);

					bool edge1Shared = false;
					bool edge2Shared = false;
					bool edge3Shared = false;

					for (int dT2 = 0; dT2 < deleteTrianglesSorted.Count; ++dT2)
					{
						if (dT == dT2)
						{
							continue;
						}

						Triangle otherTriangle = outTriangles[deleteTrianglesSorted[dT2]];

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
					outTriangles.RemoveAt(indexToDelete);
				}

				// Add new triangles for given edges
				for (int e = 0; e < edgesForNewTriangles.Count; ++e)
				{
					Edge currentEdge = edgesForNewTriangles[e];
					Triangle newTriangle = new Triangle();
					bool initSuccessfull = newTriangle.TryInit(currentEdge.IndexP1, currentEdge.IndexP2, p, inputPointList);

					if (!initSuccessfull)
					{
						Debug.Log("Could not compute Circumscribed circle. Aborting Delauney Computation.");
						return false;
					}

					outTriangles.Add(newTriangle);
				}
			}

			// Remove all triangles that share a vertex with super triangle	
			for (int t = outTriangles.Count - 1; t >= 0; --t)
			{
				if (outTriangles[t].SharesPointWith(superTriangle))
				{
					outTriangles.RemoveAt(t);
				}
			}

			if (voronoiParams.DebugDrawDelauney)
			{
				// Draw all Triangles:
				for (int t = 0; t < outTriangles.Count; ++t)
				{
					Triangle currentTriangle = outTriangles[t];

					float height = (float) t / (float) outTriangles.Count;
					height = 1.0f;
					Vector3 p1 = new Vector3(inputPointList[currentTriangle.IndexP1].x, height, inputPointList[currentTriangle.IndexP1].y);
					Vector3 p2 = new Vector3(inputPointList[currentTriangle.IndexP2].x, height, inputPointList[currentTriangle.IndexP2].y);
					Vector3 p3 = new Vector3(inputPointList[currentTriangle.IndexP3].x, height, inputPointList[currentTriangle.IndexP3].y);

					Color col = new Color(t * 0.123f % 1.0f, t * 0.311f % 1.0f, t * 0.76f % 1.0f);
					Debug.DrawLine(p1, p2, col, 3.0f);
					Debug.DrawLine(p1, p3, col, 3.0f);
					Debug.DrawLine(p2, p3, col, 3.0f);
				}
			}

			return true;
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
}