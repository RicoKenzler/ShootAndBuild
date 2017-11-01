using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	public class MainBase : MonoBehaviour
	{
		void Start ()
		{
			if (!GameManager.instance.loseConditionContextObject)
			{
				GameManager.instance.loseConditionContextObject = gameObject;
			}
		}
	
		void Update ()
		{
		
		}
	}

}