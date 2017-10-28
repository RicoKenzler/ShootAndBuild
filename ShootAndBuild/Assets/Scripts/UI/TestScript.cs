using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	public bool Trigger = false;
	public Animator Animator;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (Trigger)
		{
			DoTrigger();
			Trigger = false;
		}
	}

	void DoTrigger()
	{
		if (Animator == null)
		{
			return;
		}
	}
}
