using UnityEngine;

namespace SAB
{

    public class Blockable : MonoBehaviour
    {
        private static Vector3 invalidPosition = new Vector3(float.NegativeInfinity, float.NegativeInfinity, float.NegativeInfinity);
        private Vector3 lastPosition = invalidPosition;

        void Update()
        {
            if (lastPosition != invalidPosition)
            {
                Grid.instance.Free(gameObject, lastPosition);
            }

            lastPosition = transform.position;
            Grid.instance.Reserve(gameObject, transform.position);
        }

        // Frees the position if the Gameobjects dies, or gets disabled
        void OnDisable()
        {
            if (lastPosition != invalidPosition)
            {
                Grid.instance.Free(gameObject, lastPosition);
            }
        }
    }
}