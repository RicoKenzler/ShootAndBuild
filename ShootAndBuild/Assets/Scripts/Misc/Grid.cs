using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class Grid : MonoBehaviour
    {
        [SerializeField] private float	m_Resolution	= 1.0f;
        [SerializeField] private int	m_Size			= 128;

		///////////////////////////////////////////////////////////////////////////

        private List<bool>	m_Grid		= new List<bool>();
        private int			m_Width		= 0;
        private int			m_Height	= 0;

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;

            m_Width  = (int) (m_Size / m_Resolution);
            m_Height = (int) (m_Size / m_Resolution);
            m_Grid.Capacity = m_Width * m_Height;

            halfTile = new Vector3(m_Resolution * 0.5f, 0.0f, m_Resolution * 0.5f);

            for (int i = 0; i < m_Width * m_Height; ++i)
            {
                m_Grid.Add(false);
            }
        }

		///////////////////////////////////////////////////////////////////////////

        void OnDrawGizmos()
        {
            float w = m_Width;
            float h = m_Height;

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
            Rect area = GetAffectedArea(go, position);

            for (int y = (int)area.yMin; y < area.yMax; ++y)
            {
                for (int x = (int)area.xMin; x < area.xMax; ++x)
                {
                    int index = x + y * m_Width;
                    m_Grid[index] = value;
                }
            }
        }

		///////////////////////////////////////////////////////////////////////////

        public bool IsFree(GameObject go, Vector3 position)
        {
            Rect area = GetAffectedArea(go, position);

            for (int y = (int)area.yMin; y < area.yMax; ++y)
            {
                for (int x = (int)area.xMin; x < area.xMax; ++x)
                {
                    int index = x + y * m_Width;
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

            return new Rect(min.x, min.z, max.x - min.x, max.z - min.z);
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

        public Vector3 halfTile
        {
            get; private set;
        }

		///////////////////////////////////////////////////////////////////////////

        public static Grid instance
        {
            get; private set;
        }

    }
}