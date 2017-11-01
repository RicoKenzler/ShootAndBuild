using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HelperMethods
{
	public static void DestroyOrDestroyImmediate(Object objToDestroy)
	{
		if (Application.isPlaying)
		{
			Object.Destroy(objToDestroy);
		}
		else
		{
			Object.DestroyImmediate(objToDestroy);
		}
	}
}
