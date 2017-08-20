using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using CellIndex = System.Int32;

namespace SAB
{
	public class RegionMapGenerator 
	{
		List<RegionCell> RegionMap		= new List<RegionCell>();
		RegionParameters RegionParams	= new RegionParameters();

		Vector2 CoordsMin;
		Vector2 CoordsMax;
		Vector2 Dimensions;

		public float GetNormalizedDistance(float distance)
		{
			return distance / (0.5f * (Dimensions.x + Dimensions.y));
		}

		public float NormalizedDistanceToWS(float distance)
		{
			return distance * (0.5f * (Dimensions.x + Dimensions.y));
		}

		public Vector2 NormalizedCoordinateToWS(Vector2 normalizedCoordinate)
		{
			Vector2 posWS = normalizedCoordinate;
			posWS.x *= Dimensions.x;
			posWS.y *= Dimensions.y;
			posWS += CoordsMin;

			return posWS;
		}

		public void GenerateRegions(int regionSeed, List<VoronoiCell> voronoiCells, RegionParameters regionParams, Vector2 mapCenter, Vector2 mapSize)
		{
			// 0) Init
			RegionParams = regionParams;
			RegionMap.Clear();
			RegionMap.Capacity = voronoiCells.Count;

			Random.InitState(regionSeed);

			CoordsMin = mapCenter - mapSize * 0.5f;
			CoordsMax = mapCenter + mapSize * 0.5f;
			Dimensions = CoordsMax - CoordsMin;

			for (CellIndex c = 0; c < voronoiCells.Count; ++c)
			{
				RegionMap.Add(new RegionCell(voronoiCells[c], RegionType.Uninitialized));
			}

			InitWaterCells();
			PropagateWaterDistances();
			InitBeachAreas();
		}

		// -----------------------------------------

		public void InitWaterCells()
		{
			// Make outer cells Water
			for (CellIndex c = 0; c < RegionMap.Count; ++c)
			{
				RegionCell cell = RegionMap[c];
				
				bool isOuterCell = false;

				for (int n = 0; n < cell.VoronoiCell.NeighborCells.Count; ++n)
				{
					if (cell.VoronoiCell.NeighborCells[n].WasClamped)
					{
						isOuterCell = true;
						break;
					}
				}

				if (isOuterCell)
				{
					cell.RegionType = RegionType.Water;
				}
			}

			// Create circles that cut out some parts of the land
			for (int circle = 0; circle < RegionParams.WaterCircles; circle++)
			{
				bool horizontalBorder = Random.Range(0,2) == 0 ? true : false;

				float randomPos1DOnBorder	= Random.Range(0.0f, 1.0f);
				float randomBorder			= Random.Range(0,2) == 0 ? 0.0f : 1.0f;

				Vector2 randomPosOnBorder = new Vector2(randomPos1DOnBorder, randomBorder);

				Vector2 circleCenterNorm = horizontalBorder ? randomPosOnBorder : new Vector2(randomPosOnBorder.y, randomPosOnBorder.x);
				float circleRadiusNorm = Random.Range(RegionParams.WaterCircleSize * 0.33f, RegionParams.WaterCircleSize);

				Vector2 circleCenterWS = NormalizedCoordinateToWS(circleCenterNorm);
				float circleRadiusWS = NormalizedDistanceToWS(circleRadiusNorm);

				// Make outer cells Water
				for (CellIndex c = 0; c < RegionMap.Count; ++c)
				{
					RegionCell cell = RegionMap[c];

					float distSq = (cell.VoronoiCell.Centroid - circleCenterWS).SqrMagnitude();

					if (distSq > (circleRadiusWS * circleRadiusWS))
					{
						continue;
					}

					cell.RegionType = RegionType.Water;
				}
			}
		}

		// -----------------------------------------

		void PropagateWaterDistances()
		{
			Queue<CellIndex> SeedCells = new Queue<CellIndex>();;
			List<bool> CellIsSeedCell = new List<bool>();

			CellIsSeedCell.Capacity = RegionMap.Count;

			for (CellIndex c = 0; c < RegionMap.Count; ++c)
			{
				RegionCell cell = RegionMap[c];
				
				if (cell.RegionType == RegionType.Water)
				{
					SeedCells.Enqueue(c);
					CellIsSeedCell.Add(true);
					cell.NormalizedDistanceToWater = 0.0f;
				}
				else
				{
					CellIsSeedCell.Add(false);
					cell.NormalizedDistanceToWater = float.MaxValue;
				}
			}

			while (SeedCells.Count != 0)
			{
				CellIndex c = SeedCells.Dequeue();
				CellIsSeedCell[c] = false;

				RegionCell cell = RegionMap[c];

				for (int n = 0; n < cell.VoronoiCell.NeighborCells.Count; ++n)
				{
					CellIndex neighborIndex = cell.VoronoiCell.NeighborCells[n].NeighborIndexIfValid;

					if (neighborIndex == -1)
					{
						continue;
					}

					RegionCell neighborCell = RegionMap[neighborIndex];

					float distanceToNeighbor = Vector2.Distance(cell.VoronoiCell.Centroid, neighborCell.VoronoiCell.Centroid);

					float newDistance = cell.NormalizedDistanceToWater + GetNormalizedDistance(distanceToNeighbor);

					if (newDistance >= neighborCell.NormalizedDistanceToWater)
					{
						continue;
					}

					neighborCell.NormalizedDistanceToWater = newDistance;
						
					if (CellIsSeedCell[neighborIndex])
					{
						continue;
					}

					CellIsSeedCell[neighborIndex] = true;
					SeedCells.Enqueue(neighborIndex);
				}		
			}
		}
		
		// -----------------------------------------

		public void InitBeachAreas()
		{
			// Set Beaches in proximity to water
			for (CellIndex c = 0; c < RegionMap.Count; ++c)
			{
				RegionCell currentCell = RegionMap[c];

				float normalizedDistance = currentCell.NormalizedDistanceToWater;
				if (normalizedDistance > 0.0f && normalizedDistance < RegionParams.BeachSize)
				{
					currentCell.RegionType = RegionType.Beach;
				}

				if (currentCell.NormalizedDistanceToWater == 0.0f)
				{
					for (int n = 0; n < currentCell.VoronoiCell.NeighborCells.Count; ++n)
					{
						CellIndex neighborIndex = currentCell.VoronoiCell.NeighborCells[n].NeighborIndexIfValid;
						if (neighborIndex != -1 && RegionMap[neighborIndex].NormalizedDistanceToWater != 0.0f)
						{
							RegionMap[neighborIndex].RegionType = RegionType.Beach;
						}
					}
				}
			}
		}

		// -----------------------------------------

		public void DebugDraw()
		{
			float debugDrawHeight = 1.0f;

			if (RegionParams.ShowRegions)
			{
				for (CellIndex c = 0; c < RegionMap.Count; ++c)
				{
					VoronoiCell currentCell = RegionMap[c].VoronoiCell;
					RegionType  regionType	= RegionMap[c].RegionType;

					Color cellColor = RegionMapTypes.GetDebugColor(regionType);

					Vector3 centroid3D = new Vector3(currentCell.Centroid.x, debugDrawHeight, currentCell.Centroid.y);

					for (int p = 0; p < currentCell.NeighborCells.Count; ++p)
					{
						VoronoiNeighbor neighbor = currentCell.NeighborCells[p];

						Vector2 start	= neighbor.EdgeToNeighbor.Start;
						Vector2 end		= neighbor.EdgeToNeighbor.End;

						Vector3 start3D = new Vector3(start.x,	debugDrawHeight, start.y);
						Vector3 end3D   = new Vector3(end.x,	debugDrawHeight, end.y);

						Gizmos.color = Color.black;
						Gizmos.DrawLine(start3D, end3D);

						DebugHelper.BufferTriangle(start3D, centroid3D, end3D, cellColor);
					}
				} //< for all cells

				DebugHelper.DrawBufferedTriangles();
			}

			if (RegionParams.ShowIndices)
			{
				for (CellIndex c = 0; c < RegionMap.Count; ++c)
				{
					VoronoiCell currentCell = RegionMap[c].VoronoiCell;
					
					DebugHelper.DrawText(currentCell.Centroid, debugDrawHeight, Color.black, c.ToString());
					
				} //< for all cells
			}

			if (RegionParams.ShowWaterDistance)
			{
				for (CellIndex c = 0; c < RegionMap.Count; ++c)
				{
					RegionCell currentCell = RegionMap[c];
					
					DebugHelper.DrawText(currentCell.VoronoiCell.Centroid, debugDrawHeight, Color.black, ((int)(currentCell.NormalizedDistanceToWater * 100)).ToString());
				}
			}
		}
	}
}