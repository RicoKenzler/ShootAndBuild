﻿using System.Collections;
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

	public void SpawnParticle(ParticleSystem particle, GameObject spawner, Vector3 position, Quaternion rotation, bool isInLocalSpace, float lifetime = 10.0f)
	{
		Transform parent = isInLocalSpace ? spawner.transform : this.transform;
		GameObject newObject = Instantiate(particle.gameObject, parent);
		newObject.transform.position = position;
		newObject.transform.rotation = rotation;
		newObject.name = particle.gameObject.name + "(" + spawner.name + ")";

		// unfortunately it's not trivial to get the particles duration :(
		Destroy(newObject, lifetime);
	}
}
