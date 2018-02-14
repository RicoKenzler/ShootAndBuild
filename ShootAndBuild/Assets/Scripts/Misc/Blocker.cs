using UnityEngine;

namespace SAB
{
    public class Blocker : MonoBehaviour
    {
		private static Vector3 INVALID_POSITION = Vector3.negativeInfinity;
        private Vector3 m_LastPosition = INVALID_POSITION;

		///////////////////////////////////////////////////////////////////////////

        void Update()
        {
            if (!m_LastPosition.Equals(INVALID_POSITION))
            {
                BlockerGrid.instance.Free(gameObject, m_LastPosition);
            }

            m_LastPosition = transform.position;
            BlockerGrid.instance.Reserve(gameObject, m_LastPosition);
        }

		///////////////////////////////////////////////////////////////////////////

        // Frees the position if the Gameobjects dies, or gets disabled
        void OnDisable()
        {
            if (!m_LastPosition.Equals(INVALID_POSITION))
            {
                BlockerGrid.instance.Free(gameObject, m_LastPosition);
            }
        }
    }
}