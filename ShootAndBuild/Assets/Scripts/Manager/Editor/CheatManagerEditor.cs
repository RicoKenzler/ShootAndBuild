using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
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
				foreach (EnemyBehaviourBase enemy in EnemyManager.instance.allEnemies)
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

				List<StorableItemData> allItemDatas = FindAssetsByType<StorableItemData>();
				
				foreach (StorableItemData itemData in allItemDatas)
				{
					Inventory.ChangeItemCount_AutoSelectInventories(new ItemAndCount(itemData, cheatCount), false);
				}
			}

			if (GUILayout.Button("Add Enemies"))
			{
				Spawn.EnemySpawner[] allSpawners = FindObjectsOfType<Spawn.EnemySpawner>();
			
				if (allSpawners.Length > 0)
				{
					allSpawners[0].ForceImmediateSpawn();
				}
			}

			if (GUILayout.Button("Next Stage"))
			{
				cheatManager.completeCurrentStage = true;
			}

			if (GUILayout.Button("Next Wave"))
			{
				cheatManager.completeCurrentWave = true;
			}

			GUI.enabled = true;		

			if (!freezeEnemyOld && cheatManager.freezeEnemies)
			{
				cheatManager.pauseWaves = true;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		public static List<T> FindAssetsByType<T>() where T : UnityEngine.Object
		{
			List<T> results = new List<T>();
			string[] guids = AssetDatabase.FindAssets(string.Format("t:{0}", typeof(T)));

			for( int i = 0; i < guids.Length; i++ )
			{
				string assetPath = AssetDatabase.GUIDToAssetPath( guids[i] );
				T asset = AssetDatabase.LoadAssetAtPath<T>( assetPath );
				if( asset != null )
				{
					results.Add(asset);
				}
			}
			return results;
		}

		///////////////////////////////////////////////////////////////////////////
	}

}
