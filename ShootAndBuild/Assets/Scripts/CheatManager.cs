using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CheatManager : MonoBehaviour 
{
	[Header("Invincible")]
	public bool invinciblePlayers		= false;
	public bool invincibleEnemies		= false;
	public bool invincibleBuildings		= false;

	[Header("Freeze")]
	public bool freezeEnemies			= false;
	public bool freezeTowers			= false;
	public bool stopEnemySpawns			= false;

	[Header("Rules")]
	public bool disableWin				= false;
	public bool disableLose				= false;
	public bool noResourceCosts			= false;

	[Header("Disable Systems")]
	public bool disableAudio			= false;

	void Awake()
	{
		instance = this;
	}

	void Start() 
	{
		
	}
	
	void Update() 
	{

	}

	public static CheatManager instance
	{
		get; private set;
	}
}


[CustomEditor(typeof(CheatManager))]
public class CheatManagerEditor : Editor
{
	public override void OnInspectorGUI()
    {
		CheatManager cheatManager = (CheatManager)target;

		bool freezeEnemyOld = cheatManager.freezeEnemies;

		DrawDefaultInspector();

		// We only want to modify Scene data during play mode
		GUI.enabled = Application.isPlaying;

		List<Attackable> attackablesToKill = new List<Attackable>();

		GUILayout.Label("Kill", EditorStyles.boldLabel);
		if (GUILayout.Button("Kill all Enemies"))
		{
			foreach (EnemyBehaviour enemy in EnemyManager.instance.allEnemies)
			{
				attackablesToKill.Add(enemy.GetComponent<Attackable>());
			}
		}

		if (GUILayout.Button("Kill all Players"))
		{
			foreach (InputController player  in PlayerManager.instance.allAlivePlayers)
			{
				attackablesToKill.Add(player.GetComponent<Attackable>());
			}
		}

		if (GUILayout.Button("Kill all Buildings"))
		{
			foreach (Building building in BuildingManager.instance.allBuildings)
			{
				attackablesToKill.Add(building.GetComponent<Attackable>());
			}
		}

		foreach (Attackable attackable in attackablesToKill)
		{
			attackable.DealLethalDamage(cheatManager.gameObject, cheatManager.gameObject);
		}

		GUILayout.Label("Add", EditorStyles.boldLabel);
		if (GUILayout.Button("Add Resources"))
		{
			int cheatCount = 500;

			foreach (ItemType itemType in System.Enum.GetValues(typeof(ItemType)))
			{
				ItemData itemData = ItemManager.instance.GetItemInfos(itemType);
				if (itemData.useOnCollect)
				{
					continue;
				}

				if (itemData.isShared)
				{
					Inventory.sharedInventoryInstance.AddItem(itemType, cheatCount);
				}
				else
				{
					foreach (InputController player in PlayerManager.instance.allAlivePlayers)
					{
						player.GetComponent<Inventory>().AddItem(itemType, cheatCount);
					}
				}
				
			}
		}

		if (GUILayout.Button("Add Enemies"))
		{
			EnemySpawner[] allSpawners = FindObjectsOfType<EnemySpawner>();
			
			foreach (EnemySpawner spawner in allSpawners)
			{
				spawner.ForceImmediateSpawn();
			}
		}

		GUI.enabled = true;		

		if (!freezeEnemyOld && cheatManager.freezeEnemies)
		{
			cheatManager.stopEnemySpawns = true;
		}
    }
}