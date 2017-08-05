using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public struct Circle
	{
		public Vector2 Center;
		public float   Radius;

		bool IsPointInside(Vector2 point)
		{
			return Vector2.SqrMagnitude(point - Center) < (Radius * Radius);
		}
	}

	public struct Triangle
	{
		public int IndexP1;
		public int IndexP2;
		public int IndexP3;

		public Circle CircumscribedCircle;

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
		public void GenerateDelauney(int seedPoints, int seedPointShuffle)
		{
			// 1) Get Random Point List.
			List<Vector2> inputPointList = new List<Vector2>();
			Random.InitState(seedPoints);

			Vector2 MIN_COORDS = new Vector2(-50, -50);
			Vector2 MAX_COORDS = new Vector2( 50,  50);
			Vector2 DIMENSIONS = MAX_COORDS - MIN_COORDS;

			const int POINT_COUNT = 100;

			for (int i = 0; i < POINT_COUNT; ++i)
			{
				float x = Random.Range(MIN_COORDS.x, MAX_COORDS.x);
				float y = Random.Range(MIN_COORDS.y, MAX_COORDS.y);
				inputPointList.Add(new Vector2(x,y));
			}

			Random.InitState(seedPointShuffle);

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
				
			const int SUPER_TRIANGLE_INDEX_P1 = POINT_COUNT;
			const int SUPER_TRIANGLE_INDEX_P2 = POINT_COUNT + 1;
			const int SUPER_TRIANGLE_INDEX_P3 = POINT_COUNT + 2;
			inputPointList.Add(superTriangleP1);
			inputPointList.Add(superTriangleP2);
			inputPointList.Add(superTriangleP3);

			// 3) Start algorithm. (Our input list looks like this: [i1, i2, i3, i4, ... s1, s2, s3]
			
			for (int i = 0; i < POINT_COUNT; ++i)
			{

			}

			Vector2 center = new Vector2(-10, 10);
			Vector2 d1 = new Vector2(12,19); d1.Normalize();
			Vector2 d2 = new Vector2(12,11); d2.Normalize();
			Vector2 d3 = new Vector2(1,-23); d3.Normalize();

			Circle centerNew;
			bool success = FindCircumscribedCircle(center +d1*9, center + d2*9, center+d3*9, out centerNew);
			
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
			float divisor2 = (2 * (s.x - t.x));
			
			if ((divisor1 == 0 || divisor2 == 0))
			{
				circle.Center = (r + s + t) / 3.0f;
				circle.Radius = Vector2.Distance(r, circle.Center);
				return false;
			}
				
			float y = ((r.x*r.x + r.y*r.y)*(t.x-s.x)+(s.x*s.x+s.y*s.y)*(r.x-t.x)+(t.x*t.x+t.y*t.y)*(s.x-r.x)) / divisor1;
			float x = (s.x*s.x + s.y*s.y + 2*t.y*y - 2*s.y*y - t.x*t.x - t.y*t.y) / divisor2;

			circle.Center = new Vector2(x,y);
			circle.Radius = Vector2.Distance(r, circle.Center);
			return true;
		}
	}
}