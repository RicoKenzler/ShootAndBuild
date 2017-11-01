using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////
	// Helper struct: Cell within the sound grid
	///////////////////////////////////////////////////////////////////////////
	[System.Serializable]
	public struct AmbientSoundCell
	{
		public AmbientSoundType	SoundType;

		///////////////////////////////////////////////////////////////////////////

		public AmbientSoundCell(AmbientSoundType soundType)
		{
			SoundType = soundType;
		}

		///////////////////////////////////////////////////////////////////////////

		public static Color GetDebugColor(AmbientSoundType soundType)
		{
			switch (soundType)
			{
				case AmbientSoundType.Water:
					return Color.blue;
				case AmbientSoundType.Grass:
					return Color.green;
				case AmbientSoundType.Ruins:
					return Color.gray;
			}

			return Color.white;
		}

		///////////////////////////////////////////////////////////////////////////

		public Color GetDebugColor()
		{
			return GetDebugColor(SoundType);
		}
	}

	///////////////////////////////////////////////////////////////////////////

	public class AmbientSoundGrid : MonoBehaviour 
	{
		[SerializeField] AmbientSoundCell[]	m_AmbientGrid;
		[SerializeField] int				m_GridDimension = 0;

		///////////////////////////////////////////////////////////////////////////

		public const string AMBIENT_SOUND_GRID_OBJECT_NAME = "AmbientSoundGrid";
		public const int AMBIENT_GRID_CELL_SIZE = 16;

		///////////////////////////////////////////////////////////////////////////

		public AmbientSoundCell[]	ambientGrid		{ get { return m_AmbientGrid; } }
		public int					gridDimension	{ get { return m_GridDimension; } }

		///////////////////////////////////////////////////////////////////////////	

		public static AmbientSoundGrid CreateEmptyObject(Terrain.GeneratedTerrain parentTerrain)
		{
			if (!parentTerrain)
			{
				Debug.Assert(false, "No Terrain set");
				return null;
			}

			GameObject oldObject = parentTerrain.transform.FindImmediateChildOfName(AMBIENT_SOUND_GRID_OBJECT_NAME);

			if (oldObject)
			{
				HelperMethods.DestroyOrDestroyImmediate(oldObject);
			}

			GameObject newObject = new GameObject(AMBIENT_SOUND_GRID_OBJECT_NAME, typeof(AmbientSoundGrid));
			newObject.transform.parent = parentTerrain.transform;
			AmbientSoundGrid newSoundGrid = newObject.GetComponent<AmbientSoundGrid>();

			parentTerrain.ambientSoundGrid = newSoundGrid;

			return newSoundGrid;
		}

		///////////////////////////////////////////////////////////////////////////	

		public void GenerateAmbientGrid(Terrain.RegionGrid regionGrid, List<Terrain.RegionCell> regionCells, Vector2 terrainSizeWS)
		{
			if (regionGrid == null)
			{
				m_AmbientGrid = null;
				return;
			}

			m_GridDimension = (int) Mathf.Ceil(Mathf.Max(terrainSizeWS.x, terrainSizeWS.y) / (float) AMBIENT_GRID_CELL_SIZE);
			m_GridDimension = Mathf.Max(m_GridDimension, 1);
			
			m_AmbientGrid = new AmbientSoundCell[m_GridDimension * m_GridDimension];

			int regionDimensionX = regionGrid.sizeX;
			int regionDimensionZ = regionGrid.sizeZ;
			Vector2 regionTileSize = new Vector2(terrainSizeWS.x / regionDimensionX, terrainSizeWS.y / regionDimensionZ);

			for (int ambientX = 0 ; ambientX < m_GridDimension; ++ambientX)
			{
				float xWS		= (ambientX + 0.5f) * AMBIENT_GRID_CELL_SIZE;
				int regionX		= (int) (xWS / regionTileSize.x);
				regionX			= Mathf.Min(regionX, regionDimensionX - 1);

				for (int ambientZ = 0 ; ambientZ < m_GridDimension; ++ambientZ)
				{
					float zWS		= (ambientZ + 0.5f) * AMBIENT_GRID_CELL_SIZE;
					int regionZ		= (int) (zWS / regionTileSize.y);
					regionZ			= Mathf.Min(regionZ, regionDimensionZ - 1);

					Terrain.RegionTile cellsCenterTile = regionGrid.GetAt(regionX, regionZ);
					Terrain.RegionType regionType = regionCells[cellsCenterTile.Cell].RegionType;

					AmbientSoundCell curCell = new AmbientSoundCell(AmbientSoundType.Grass);

					switch (regionType)
					{
						case Terrain.RegionType.Bricks:
							curCell.SoundType = AmbientSoundType.Ruins;
							break;

						case Terrain.RegionType.Beach:
						case Terrain.RegionType.Water:
							curCell.SoundType = AmbientSoundType.Water;
							break;
					}

					m_AmbientGrid[ambientX * m_GridDimension + ambientZ] = curCell;
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////	

		public AmbientSoundCell GetAmbientCellSafe(Vector2 posWS)
		{
			int iX = (int) (posWS.x / (float) AmbientSoundGrid.AMBIENT_GRID_CELL_SIZE);
			int iZ = (int) (posWS.y / (float) AmbientSoundGrid.AMBIENT_GRID_CELL_SIZE);

			iX = Mathf.Clamp(iX, 0, m_GridDimension - 1);
			iZ = Mathf.Clamp(iZ, 0, m_GridDimension - 1);

			return m_AmbientGrid[iX * m_GridDimension + iZ];
		}

		///////////////////////////////////////////////////////////////////////////
	
		public void OnDrawGizmosSelected()
		{
			DebugDraw();
		}

		///////////////////////////////////////////////////////////////////////////

		public void DebugDraw()
		{
			float debugDrawHeight = 1.0f;

			for (int iX = 0; iX < m_GridDimension; iX++)
			{
				for (int iZ = 0; iZ < m_GridDimension; iZ++)
				{
					Vector2 cellMin = new Vector2(iX * AMBIENT_GRID_CELL_SIZE, iZ * AMBIENT_GRID_CELL_SIZE);
					Vector2 cellMax = cellMin + new Vector2(AMBIENT_GRID_CELL_SIZE, AMBIENT_GRID_CELL_SIZE);
					
					Vector2 cellMinOffsetted = Vector2.Lerp(cellMin, cellMax, 0.05f);
					Vector2 cellMaxOffsetted = Vector2.Lerp(cellMin, cellMax, 0.95f);

					AmbientSoundCell curCell = m_AmbientGrid[iX * m_GridDimension + iZ];

					Color col = curCell.GetDebugColor();		
					col.a = 0.5f;	

					DebugHelper.BufferQuad(new Vector3(cellMinOffsetted.x, debugDrawHeight, cellMinOffsetted.y), new Vector3(cellMaxOffsetted.x, debugDrawHeight, cellMaxOffsetted.y), col);
				}
			}

			DebugHelper.DrawBufferedTriangles();
		}

		///////////////////////////////////////////////////////////////////////////
	}
}
