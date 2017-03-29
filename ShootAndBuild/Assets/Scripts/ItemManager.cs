using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType
{
	None		= 0,

	Gold		= 1,
	Granades	= 2,
}

[System.Serializable]
public struct ItemDrop
{
	public GameObject	itemPrefab;
	public float		dropProbability;
	public int			minDropAmount;
	public int			maxDropAmount;
}

public class ItemManager : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Awake()
    {
        instance = this;
    }

	public static ItemManager instance
    {
        get; private set;
    }
}
