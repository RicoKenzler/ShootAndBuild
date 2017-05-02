using UnityEngine;

namespace SAB
{

    public class Throwable : MonoBehaviour
    {
        public GameObject projectilePrefab;

        public AudioData throwSound;

        public void Throw()
        {
            GameObject projectileContainer = GameObject.Find("Projectiles");

            GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
            instance.transform.position = transform.position;
            instance.GetComponent<Grenade>().owner = this;

            Rigidbody body = instance.GetComponent<Rigidbody>();
            body.velocity = transform.rotation * Vector3.forward * 10;

            AudioManager.instance.PlayAudio(throwSound, instance.transform.position);
        }
    }
}