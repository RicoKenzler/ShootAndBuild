using UnityEngine;

namespace SAB.Terrain
{
	[System.Serializable]
	public class RegionGrid
	{
		[SerializeField] private RegionTile[]				m_RegionTiles;
		[SerializeField] private RegionMapTransformation	m_RegionMapTransformation;
		[SerializeField] private int m_SizeX = 0;
		[SerializeField] private int m_SizeZ = 0;

		///////////////////////////////////////////////////////////////////////////

		public int						sizeX					{ get { return m_SizeX; } }
		public int						sizeZ					{ get { return m_SizeZ; } }
		public RegionMapTransformation	regionMapTransformation { get { return m_RegionMapTransformation; } }

		///////////////////////////////////////////////////////////////////////////

		public RegionGrid(int _sizeX, int _sizeZ, Vector2 mapSizeWS)
		{
			m_SizeX = _sizeX;
			m_SizeZ = _sizeZ;

			m_RegionTiles = new RegionTile[m_SizeX * m_SizeZ];

			for (int i = 0; i < m_SizeX * m_SizeZ; ++i)
			{
				m_RegionTiles[i] = new RegionTile();
				m_RegionTiles[i].Reset();
			}

			Debug.Assert(m_SizeX == m_SizeZ);

			m_RegionMapTransformation = new RegionMapTransformation(mapSizeWS, m_SizeX);
		}

		///////////////////////////////////////////////////////////////////////////

		private int GetIndex(int x, int z)
		{
			return (z * m_SizeX) + x;
		}

		///////////////////////////////////////////////////////////////////////////

		public RegionTile GetAt(int x, int z)
		{
			int index = GetIndex(x, z);

			return m_RegionTiles[index];
		}
	}
}