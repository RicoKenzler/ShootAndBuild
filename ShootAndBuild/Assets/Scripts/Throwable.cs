using UnityEngine;

public class Throwable : MonoBehaviour
{
	public GameObject projectilePrefab;

	public AudioClip[] throwSounds;

	public void Throw()
	{
		GameObject projectileContainer = GameObject.Find("Projectiles");

		GameObject instance = Instantiate(projectilePrefab, projectileContainer.transform);
		instance.transform.position = transform.position;

		Rigidbody body = instance.GetComponent<Rigidbody>();
		body.velocity = transform.rotation * Vector3.forward * 10;

		AudioManager.instance.PlayRandomOneShot(throwSounds, new OneShotParams(instance.transform.position));
	}
}
