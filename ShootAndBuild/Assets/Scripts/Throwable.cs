using UnityEngine;

namespace SAB
{

    public class Throwable : MonoBehaviour
    {
        public GameObject projectilePrefab;

        public AudioData throwSound;

		public float throwStrength = 10;

        public void Throw()
        {
            GameObject projectileContainer = GameObject.Find("Projectiles");

            GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
            instance.transform.position = transform.position + transform.forward * 0.5f;
            instance.GetComponent<Grenade>().owner = this;
			instance.GetComponent<Grenade>().ownerFaction = GetComponent<Attackable>().faction;

            Rigidbody body = instance.GetComponent<Rigidbody>();
            body.velocity = transform.forward * throwStrength;

            AudioManager.instance.PlayAudio(throwSound, instance.transform.position);
        }
    }
}