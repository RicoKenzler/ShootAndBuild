using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using EdgeKey = System.Int64;
using PointIndex = System.Int32;
using TriIndex = System.Int32;

namespace SAB
{
	public class VoronoiPointGenerator
	{
		VoronoiCreationState State;

		public void CreateRandomPoints(VoronoiCreationState creationState, int seed)
		{
			State = creationState;

			Debug.Assert(State.InputVerticesIncludingSuperTriangle.Count == 0);

			// Distribute ~2/3 of the points p to a regular grid of size y,x
			// x   *   y = p/2
			// r*y *   y = p/2		(r = x/y)
			// y = sqrt(p/(2*r))
			// x = h*y;
			
			// 1) Get Random Point List.
			UnityEngine.Random.InitState(seed);

			float ratioXbyY = State.DIMENSIONS.x / State.DIMENSIONS.y;
			float pseudoRandomGridDimensionY_F = Mathf.Sqrt((State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE * 2 / 3));
			float pseudoRandomGridDimensionX_F = pseudoRandomGridDimensionY_F * ratioXbyY;
			int pseudoRandomGridDimensionX = (int) Mathf.Max(pseudoRandomGridDimensionX_F, 1);
			int pseudoRandomGridDimensionY = (int) Mathf.Max(pseudoRandomGridDimensionY_F, 1);
	
			int pseudoRandomGridPointCount = pseudoRandomGridDimensionX * pseudoRandomGridDimensionX;

			Vector2 PSEUDO_RANDOM_GRID_CELL_SIZE = new Vector2(State.DIMENSIONS.x / (float)pseudoRandomGridDimensionX, State.DIMENSIONS.y / (float)pseudoRandomGridDimensionY);

			for (int i = 0; i < State.POINT_COUNT_WITHOUT_SUPER_TRIANGLE; ++i)
			{
				float x;
				float y;

				if (i < pseudoRandomGridPointCount)
				{
					// randomly distribute within grid cell
					int gridCellX = i % pseudoRandomGridDimensionX;
					int gridCellY = i / pseudoRandomGridDimensionX;

					x = UnityEngine.Random.Range(State.MIN_COORDS.x + gridCellX * PSEUDO_RANDOM_GRID_CELL_SIZE.x, State.MIN_COORDS.x + (gridCellX + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.x);
					y = UnityEngine.Random.Range(State.MIN_COORDS.y + gridCellY * PSEUDO_RANDOM_GRID_CELL_SIZE.y, State.MIN_COORDS.y + (gridCellY + 1) * PSEUDO_RANDOM_GRID_CELL_SIZE.y);
				}
				else
				{
					// randomly distribute
					x = UnityEngine.Random.Range(State.MIN_COORDS.x, State.MAX_COORDS.x);
					y = UnityEngine.Random.Range(State.MIN_COORDS.y, State.MAX_COORDS.y);
				}

				x = Mathf.Clamp(x, State.MIN_COORDS.x + 0.02f * State.DIMENSIONS.x, State.MAX_COORDS.x - 0.02f * State.DIMENSIONS.x);
				y = Mathf.Clamp(y, State.MIN_COORDS.y + 0.02f * State.DIMENSIONS.y, State.MAX_COORDS.y - 0.02f * State.DIMENSIONS.y);

				bool validPoint = true;

				for (int j = 0; j < State.InputVerticesIncludingSuperTriangle.Count; ++j)
				{
					float dx = State.InputVerticesIncludingSuperTriangle[j].x - x;
					float dy = State.InputVerticesIncludingSuperTriangle[j].y - y;

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

				State.InputVerticesIncludingSuperTriangle.Add(new Vector2(x,y));
			}
		} //< end create random points

	} //< end class
}