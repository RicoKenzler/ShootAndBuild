using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

	public enum EnemyType
	{
		None = 0,
		Bat = 1,
		Rabbit = 2,
		Slime = 3,
	}

	//----------------------------------------------------------------------

	//TODO maybe expand this to something much bigger with update logic, conditions, etc..
	[Serializable]
	public struct EnemyWave
	{

		public EnemyWave(int _spwanerCount)
		{
			//index will be spwaner id
			spawnPropability = new SpawnPropabilityBlock[_spwanerCount];
			for (int i = 0; i < spawnPropability.Length; ++i)
			{
				spawnPropability[i].enemies = new List<EnemyType>();
				spawnPropability[i].spawnRate = new List<float>();
			}
			duration = 30f;

		}

		public float duration;
		public SpawnPropabilityBlock[] spawnPropability;    
	}

	//----------------------------------------------------------------------

	[Serializable]
	public struct SpawnPropabilityBlock
	{
		public List<EnemyType> enemies;
		public List<float> spawnRate;

	}

	//----------------------------------------------------------------------

	//[HideInInspector]
	public List<EnemyWave> waves; 

	//----------------------------------------------------------------------

	public EnemySpawner[] spawners;

	//TODO write editor and match with enemy type enum
	public GameObject[] enemyTemplates;

	//----------------------------------------------------------------------

	void Awake()
	{
		if (spawners == null || spawners.Length == 0)
		{
			Debug.LogError("No Spawners assigned!");
		}
	}

	//----------------------------------------------------------------------

	// Use this for initialization
	void Start ()
	{
		
	}

	//----------------------------------------------------------------------

	// Update is called once per frame
	void Update ()
	{
		
	}
}
