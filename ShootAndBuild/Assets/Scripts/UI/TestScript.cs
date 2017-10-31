using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	[SerializeField] private bool m_Trigger = false;
	[SerializeField] private Animator m_Animator;

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update ()
	{
		if (m_Trigger)
		{
			DoTrigger();
			m_Trigger = false;
		}
	}

	void DoTrigger()
	{
		if (m_Animator == null)
		{
			return;
		}
	}
}
