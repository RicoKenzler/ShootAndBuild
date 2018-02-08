using UnityEngine;

namespace SAB
{
    public class Blockable : MonoBehaviour
    {
        private static Vector3 INVALID_POSITION = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        private Vector3 m_LastPosition = INVALID_POSITION;

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            if (m_LastPosition != INVALID_POSITION)
            {
                Grid.instance.Free(gameObject, m_LastPosition);
            }

            m_LastPosition = transform.position;
            Grid.instance.Reserve(gameObject, m_LastPosition);
        }

		///////////////////////////////////////////////////////////////////////////

        // Frees the position if the Gameobjects dies, or gets disabled
        void OnDisable()
        {
            if (m_LastPosition != INVALID_POSITION)
            {
                Grid.instance.Free(gameObject, m_LastPosition);
            }
        }
    }
}