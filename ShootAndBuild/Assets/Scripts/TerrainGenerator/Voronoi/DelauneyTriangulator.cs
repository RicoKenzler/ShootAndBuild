﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey		= System.Int64;
using PointIndex	= System.Int32;
using TriIndex		= System.Int32;

namespace SAB.Terrain
{
	public class DelauneyTriangulator
	{
		private VoronoiCreationState m_State;

		// Bowyer-Watson Algorithm for Delauny-Triangulation
		public bool GetDelauneyTriangluation(VoronoiCreationState creationState)
		{
			m_State = creationState;

			Debug.Assert(m_State.InputVerticesIncludingSuperTriangle.Count == m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE);
			Debug.Assert(m_State.DelauneyTrianglesIncludingSuperTriangle.Count == 0);

			// 2) Get Super Triangle
			float maxDimension = Mathf.Max(m_State.DIMENSIONS.x, m_State.DIMENSIONS.y);
			Vector2 superTriangleP1 = new Vector2(0.0f - maxDimension, 0.0f - maxDimension);
			Vector2 superTriangleP2 = superTriangleP1 + new Vector2(0, maxDimension * 5);
			Vector2 superTriangleP3 = superTriangleP1 + new Vector2(maxDimension * 5, 0);
				
			PointIndex SUPER_TRIANGLE_INDEX_P1 = m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE;
			PointIndex SUPER_TRIANGLE_INDEX_P2 = m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE + 1;
			PointIndex SUPER_TRIANGLE_INDEX_P3 = m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE + 2;
			m_State.InputVerticesIncludingSuperTriangle.Add(superTriangleP1);
			m_State.InputVerticesIncludingSuperTriangle.Add(superTriangleP2);
			m_State.InputVerticesIncludingSuperTriangle.Add(superTriangleP3);

			// 3) Start algorithm. (Our input list looks like this: [i1, i2, i3, i4, ... s1, s2, s3]		
			bool superTriangleValid = m_State.SuperTriangle.TryInit(SUPER_TRIANGLE_INDEX_P1, SUPER_TRIANGLE_INDEX_P2, SUPER_TRIANGLE_INDEX_P3, m_State.InputVerticesIncludingSuperTriangle);

			if (!superTriangleValid)
			{
				Debug.Log("Corrupt Supertriangle. Aborting Delauney Computation.");
				return false;
			}

			m_State.DelauneyTrianglesIncludingSuperTriangle.Add(m_State.SuperTriangle);
			
			for (int p = 0; p < m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++p) 
			{
				Vector2 currentPoint = m_State.InputVerticesIncludingSuperTriangle[p];

				List<TriIndex> deleteTrianglesSorted = new List<TriIndex>();

				// Find all Traingles, in which circle we lie
				for (TriIndex t = 0; t < m_State.DelauneyTrianglesIncludingSuperTriangle.Count; ++t)
				{
					TriangleI currentTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[t];

					if (currentTriangle.CircumscribedCircle.IsPointInside(currentPoint))
					{
						deleteTrianglesSorted.Add(t);	
					}
				}

				List<EdgeIndices> edgesForNewTriangles = new List<EdgeIndices>();

				// Find all Edges that are not shared between badies
				for (int dT = 0; dT < deleteTrianglesSorted.Count; ++dT)
				{
					TriangleI currentTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[deleteTrianglesSorted[dT]];

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

						TriangleI otherTriangle = m_State.DelauneyTrianglesIncludingSuperTriangle[deleteTrianglesSorted[dT2]];

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
					m_State.DelauneyTrianglesIncludingSuperTriangle.RemoveAt(indexToDelete);
				}

				// Add new triangles for given edges
				for (int e = 0; e < edgesForNewTriangles.Count; ++e)
				{
					EdgeIndices currentEdge = edgesForNewTriangles[e];
					TriangleI newTriangle = new TriangleI();
					bool initSuccessfull = newTriangle.TryInit(currentEdge.IndexP1, currentEdge.IndexP2, p, m_State.InputVerticesIncludingSuperTriangle);

					if (!initSuccessfull)
					{
						Debug.Log("Could not compute Circumscribed circle. Aborting Delauney Computation.");
						return false;
					}

					m_State.DelauneyTrianglesIncludingSuperTriangle.Add(newTriangle);
				}
			} //< end for each point

			return true;
		} //< end GetDelauneyTriangulation

	} //< end class
}