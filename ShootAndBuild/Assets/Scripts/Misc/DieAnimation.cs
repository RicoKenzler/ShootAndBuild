using UnityEngine;

public class DieAnimation : MonoBehaviour
{
	private const float sinkDelay = 5.0f;
	private const float sinkDuration = 5.0f;
	private float timeSinceStart = 0.0f;
	private Vector3 startPos;
	private Vector3 objectSize;
	private Collider colliderRef;

	void Start()
	{
		foreach (MonoBehaviour c in GetComponents<MonoBehaviour>())
		{
			if (c != this)
			{
				c.enabled = false;
			}
		}

		startPos = transform.position;

		colliderRef = GetComponent<Collider>();
		objectSize = colliderRef.bounds.size;
		colliderRef.enabled = false;

		GetComponent<Rigidbody>().velocity = Vector3.zero;

		GetComponentInChildren<Animation>().Play("die");
	}

	void Update()
	{
		timeSinceStart += Time.deltaTime;

		if (timeSinceStart > sinkDelay)
		{
			float p = (timeSinceStart - sinkDelay) / sinkDuration;
			float offset = objectSize.y * p;
			Vector3 pos = startPos;
			pos.y -= offset;
			transform.position = pos;
		}
		if (timeSinceStart > sinkDelay + sinkDuration)
		{
			Destroy(gameObject);
		}
	}

	public void ShowBloodDecal(GameObject decal)
	{
		GameObject parent = GameObject.Find("DecalManager");
		GameObject instance = Instantiate(decal, parent.transform);
		Vector3 pos = transform.position;
		pos.y = 0;
		instance.transform.position = pos;
	}
}
