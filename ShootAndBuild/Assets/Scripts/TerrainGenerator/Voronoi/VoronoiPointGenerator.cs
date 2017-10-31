using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey = System.Int64;
using PointIndex = System.Int32;
using TriIndex = System.Int32;

namespace SAB.Terrain
{
	public class VoronoiPointGenerator
	{
		private VoronoiCreationState m_State;

		public void CreateRandomPoints(VoronoiCreationState creationState, int seed)
		{
			m_State = creationState;

			Debug.Assert(m_State.InputVerticesIncludingSuperTriangle.Count == 0);

			// Distribute ~2/3 of the points p to a regular grid of size y,x
			// x   *   y = p/2
			// r*y *   y = p/2		(r = x/y)
			// y = sqrt(p/(2*r))
			// x = h*y;
			
			// 1) Get Random Point List.
			UnityEngine.Random.InitState(seed);

			float ratioXbyY = m_State.DIMENSIONS.x / m_State.DIMENSIONS.y;
			float pseudoRandomGridDimensionY_F = Mathf.Sqrt((m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE * 2 / 3));
			float pseudoRandomGridDimensionX_F = pseudoRandomGridDimensionY_F * ratioXbyY;
			int pseudoRandomGridDimensionX = (int) Mathf.Max(pseudoRandomGridDimensionX_F, 1);
			int pseudoRandomGridDimensionY = (int) Mathf.Max(pseudoRandomGridDimensionY_F, 1);
	
			int pseudoRandomGridPointCount = pseudoRandomGridDimensionX * pseudoRandomGridDimensionX;

			Vector2 PSEUDO_RANDOM_GRID_CELL_SIZE = new Vector2(m_State.DIMENSIONS.x / (float)pseudoRandomGridDimensionX, m_State.DIMENSIONS.y / (float)pseudoRandomGridDimensionY);

			for (int i = 0; i < m_State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++i)
			{
				float x;
				float y;

				if (i < pseudoRandomGridPointCount)
				{
					// randomly distribute within grid cell
					int gridCellX = i % pseudoRandomGridDimensionX;
					int gridCellY = i / pseudoRandomGridDimensionX;

					x = UnityEngine.Random.Range(0.0f + gridCellX * PSEUDO_RANDOM_GRID_CELL_SIZE.x, (gridCellX + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.x);
					y = UnityEngine.Random.Range(0.0f + gridCellY * PSEUDO_RANDOM_GRID_CELL_SIZE.y, (gridCellY + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.y);
				}
				else
				{
					// randomly distribute
					x = UnityEngine.Random.Range(0.0f, m_State.DIMENSIONS.x);
					y = UnityEngine.Random.Range(0.0f, m_State.DIMENSIONS.y);
				}

				x = Mathf.Clamp(x, 0.0f + 0.02f * m_State.DIMENSIONS.x, m_State.DIMENSIONS.x - 0.02f * m_State.DIMENSIONS.x);
				y = Mathf.Clamp(y, 0.0f + 0.02f * m_State.DIMENSIONS.y, m_State.DIMENSIONS.y - 0.02f * m_State.DIMENSIONS.y);

				bool validPoint = true;

				for (int j = 0; j < m_State.InputVerticesIncludingSuperTriangle.Count; ++j)
				{
					float dx = m_State.InputVerticesIncludingSuperTriangle[j].x - x;
					float dy = m_State.InputVerticesIncludingSuperTriangle[j].y - y;

					if (Mathf.Abs(dx) < 0.001f && Mathf.Abs(dy) < 0.001f)
					{
						validPoint = false;;
						break;
					}
				}

				if (!validPoint)
				{
					--i;
					continue;
				}

				m_State.InputVerticesIncludingSuperTriangle.Add(new Vector2(x,y));
			}
		} //< end create random points

	} //< end class
}