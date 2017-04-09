using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleManager : MonoBehaviour
{
	void Awake()
    {
        instance = this;
    }

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{

	}

	public static ParticleManager instance
    {
        get; private set;
    }

	public void SpawnParticle(ParticleSystem particleAsset, GameObject spawner, Vector3 position, Quaternion rotation, bool isInLocalSpace, float lifetime, bool scaleByParentSize, bool offsetToOutside)
	{
		Transform parent = isInLocalSpace ? spawner.transform : this.transform;

		GameObject newObject = Instantiate(particleAsset.gameObject, parent);
		newObject.transform.position = position;
		newObject.transform.rotation = rotation;
		newObject.name = particleAsset.gameObject.name + "(" + spawner.name + ")";

		Collider collider = spawner.GetComponent<Collider>();

		Vector3 bboxExtents = collider.bounds.extents;
		
		float scale = bboxExtents.x + bboxExtents.y + bboxExtents.z;
		scale /= 3.0f;

		if (scaleByParentSize)
		{
			Vector3 assetScale = particleAsset.transform.localScale;
			newObject.transform.localScale = assetScale * scale;
		}

		if (offsetToOutside)
		{
			newObject.transform.position = newObject.transform.position + scale * newObject.transform.forward;
		}

		// unfortunately it's not trivial to get the particles duration :(
		Destroy(newObject, lifetime);
	}
}
