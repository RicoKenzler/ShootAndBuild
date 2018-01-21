using SAB.Terrain;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class Grid : MonoBehaviour
    {
		[HideInInspector]
		[SerializeField] private List<bool> m_StaticGrid	= new List<bool>();
        [SerializeField] private float		m_Resolution	= 1.0f;
        [SerializeField] private int		m_Size			= 128;

		///////////////////////////////////////////////////////////////////////////

        private List<bool>	m_Grid		= new List<bool>();

		///////////////////////////////////////////////////////////////////////////

        public static Grid	instance { get; private set; }
		public Vector3		halfTile { get; private set; }
		public int			size	 { get; private set; }

		///////////////////////////////////////////////////////////////////////////

		public Grid()
		{

		}

		void Awake()
        {
            instance = this;

			size = m_Size;
            halfTile = new Vector3(m_Resolution * 0.5f, 0.0f, m_Resolution * 0.5f);

			int c = width * height;

            m_Grid.Capacity = c;
			for (int i = 0; i < c; ++i)
            {
                m_Grid.Add(m_StaticGrid[i]);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        void OnDrawGizmos()
        {
            float w = width;
            float h = height;

            for (int i = 0; i < m_Grid.Count; ++i)
            {
                float x = (i % w) * m_Resolution;
                float y = ((int)(i / h)) * m_Resolution;
                Vector3 drawPos = new Vector3(x, 5.0f, y);
                drawPos += halfTile;

                Gizmos.color = m_Grid[i] == true ? Color.red : Color.yellow;
                Gizmos.DrawCube(drawPos, Vector3.one * m_Resolution * 0.8f);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public void Reserve(GameObject go, Vector3 position)
        {
            Set(go, true, position);
        }

		///////////////////////////////////////////////////////////////////////////

        public void Free(GameObject go, Vector3 position)
        {
            Set(go, false, position);
        }

		///////////////////////////////////////////////////////////////////////////

        private void Set(GameObject go, bool value, Vector3 position)
        {
			int w = width;
            Rect area = GetAffectedArea(go, position);

            for (int y = (int)area.yMin; y < area.yMax; ++y)
            {
                for (int x = (int)area.xMin; x < area.xMax; ++x)
                {
                    int index = x + y * w;
                    m_Grid[index] = value;
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public bool IsFree(GameObject go, Vector3 position)
        {
			int w = width;
            Rect area = GetAffectedArea(go, position);

            for (int y = (int)area.yMin; y < area.yMax; ++y)
            {
                for (int x = (int)area.xMin; x < area.xMax; ++x)
                {
                    int index = x + y * w;
                    if (m_Grid[index] == true)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

		///////////////////////////////////////////////////////////////////////////

        private Rect GetAffectedArea(GameObject go, Vector3 position)
        {
            Collider collider = go.GetComponent<Collider>();
            Vector3 extents = collider.bounds.extents;

            // urgh, extends is zero. This can happen for unbuild objects
            // use size instead. Don't know if this is a good idea
            if (extents == Vector3.zero)
            {
                BoxCollider box = go.GetComponent<BoxCollider>();
                if (box)
                {
                    extents = box.size / 2;
                }
                else
                {
                    CapsuleCollider capsule = go.GetComponent<CapsuleCollider>();
                    extents = capsule.radius * Vector3.one;
                }
            }

            Vector3 min = ToNextTile(position - extents);
            Vector3 max = ToNextTile(position + extents);

			min.x = Mathf.Max(min.x, 0);
			min.z = Mathf.Max(min.z, 0);
			max.x = Mathf.Min(max.x, size);
			max.z = Mathf.Min(max.z, size);

			Rect r = new Rect(min.x, min.z, max.x - min.x, max.z - min.z);
			if (r.width < 1)
			{
				r.width = 1;
			}
			if (r.height < 1)
			{
				r.height = 1;
			}

			return r;
        }

		///////////////////////////////////////////////////////////////////////////

        private Vector3 ToNextTile(Vector3 worldPos)
        {
            return new Vector3(Mathf.Round(worldPos.x / m_Resolution), worldPos.y, Mathf.Round(worldPos.z / m_Resolution));
        }

		///////////////////////////////////////////////////////////////////////////

        public Vector3 ToTileCenter(Vector3 input)
        {
            return new Vector3(Mathf.Floor(input.x) + halfTile.x, input.y, Mathf.Floor(input.z) + halfTile.z);
        }

		///////////////////////////////////////////////////////////////////////////

		public void CreateGridByTerrain(GeneratedTerrain terrain)
		{
			int h = height;
			int w = width;

			Vector2 terrainCellSize = terrain.regionGrid.regionMapTransformation.CellSize;
			
			m_StaticGrid.Clear();

			for (int y = 0; y < h; ++y)
			{
				for (int x = 0; x < w; ++x)
				{
					int tx = (int)(x / terrainCellSize.x);
					int ty = (int)(y / terrainCellSize.y);

					if (tx > terrain.regionGrid.sizeX || ty > terrain.regionGrid.sizeZ)
					{
						m_StaticGrid.Add(false);
						continue;
					}

					RegionTile tile = terrain.regionGrid.GetAt(tx, ty);
					bool blocked = tile.MainRegion == RegionType.Water;
					m_StaticGrid.Add(blocked);
				}
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private int width
		{
			get
			{
				return (int)(m_Size / m_Resolution);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		private int height
		{
			get
			{
				return (int)(m_Size / m_Resolution);
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}
}