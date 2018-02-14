using SAB.Spawn;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAB
{
	public class EnemyWaveEditor : EditorWindow
	{
		private static SpawnManagerPrototype m_Manager = null;
		private static EnemyWaveEditor m_Window = null;
		private Vector2 m_ScrollPosition;

		///////////////////////////////////////////////////////////////////////////

		[MenuItem("ShootAndBuild-Tools/Enemy Wave Editor Prototype")]
		static void Init()
		{
			// Get existing open window or if none, make a new one:
			m_Window = GetWindow<EnemyWaveEditor>();
			m_Window.Show();
		}

		///////////////////////////////////////////////////////////////////////////

		void OnEnable()
		{
			SpawnManagerPrototype[] managers = Resources.FindObjectsOfTypeAll<SpawnManagerPrototype>();
			if (managers.Count() > 0)
			{
				m_Manager = managers[0];
			}

			m_Window = this;
		}

		///////////////////////////////////////////////////////////////////////////

		void OnFocus()
		{
			SpawnManagerPrototype[] managers = Resources.FindObjectsOfTypeAll<SpawnManagerPrototype>();
			if (managers.Count() > 0)
			{
				m_Manager = managers[0];
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void OnGUI()
		{
			m_ScrollPosition = EditorGUILayout.BeginScrollView(m_ScrollPosition);

			EditorGUILayout.BeginVertical();

			for (int i = 0; i < m_Manager.waves.Count; ++i)
			{
				EditorGUILayout.BeginHorizontal();

				EditorGUILayout.BeginVertical(GUILayout.Width(100));

				GUILayout.Label("Wave " + (i + 1));
				GUILayout.Button("Up");
				GUILayout.Button("Down");
				if (GUILayout.Button("X"))
				{
					m_Manager.waves.RemoveAt(i);
					break;
				}

				EditorGUILayout.EndVertical();

				for (int j = 0; j < m_Manager.waves[i].stages.Count; ++j)
				{
					GUIStyle style = new GUIStyle(EditorStyles.helpBox);
					style.fixedWidth = 150;
					style.border = new RectOffset(2, 2, 2, 2);
					style.margin = new RectOffset(4, 4, 4, 4);

					GUILayout.BeginVertical(style);
					GUILayout.BeginHorizontal();
					DrawComboBox(m_Manager.waves[i].stages[j], m_Manager.waves[i]);
					if (GUILayout.Button("X"))
					{
						m_Manager.waves[i].stages.RemoveAt(j);
						break;
					}
					GUILayout.EndHorizontal();
					DrawStage(m_Manager.waves[i].stages[j]);
					GUILayout.EndVertical();
				}

				if (GUILayout.Button("+"))
				{
					MonsterSpawnStage stage = new MonsterSpawnStage();
					m_Manager.waves[i].stages.Add(stage);
				}
				
				EditorGUILayout.EndHorizontal();
			}

			GUILayout.Space(6);

			if (GUILayout.Button("Add New Wave"))
			{
				m_Manager.waves.Add(new SpawnWave());
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndScrollView();


			if (GUI.changed)
			{
				EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
				EditorUtility.SetDirty(m_Manager);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawComboBox(SpawnWaveStage stage, SpawnWave wave)
		{
			List<Type> stages = typeof(SpawnWaveStage)
			.Assembly.GetTypes()
			.Where(t => t.IsSubclassOf(typeof(SpawnWaveStage)))
			.ToList();
			
			List<string> names = stages.Select(t => t.Name).ToList();

			string selectedName = stage.GetType().Name;
			int oldIndex = names.IndexOf(selectedName);

			int newIndex = EditorGUILayout.Popup(oldIndex, names.ToArray());
			if (oldIndex != newIndex)
			{
				int stageIndex = wave.stages.IndexOf(stage);
				wave.stages.Remove(stage);
				wave.stages.Insert(stageIndex, (SpawnWaveStage)Activator.CreateInstance(stages[newIndex]));
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawEnemies(List<SpawnMob> monsters)
		{
			List<GameObject> enemies = Resources.FindObjectsOfTypeAll<EnemyBehaviourBase>().Select(t => t.gameObject).ToList();
			string[] enemyNames = enemies.Select(t => t.name).ToArray();

			EditorGUILayout.BeginVertical();

			for (int i = 0; i < monsters.Count; ++i)
			{
				EditorGUILayout.BeginHorizontal();

				int oldIndex = enemies.FindIndex(t => t == monsters[i].enemy);
				int newIndex = EditorGUILayout.Popup(oldIndex, enemyNames);

				if (oldIndex != newIndex)
				{
					monsters.RemoveAt(i);
					monsters.Insert(i, new SpawnMob() { enemy = enemies[newIndex] });
				}

				string output = GUILayout.TextField(monsters[i].count.ToString(), GUILayout.Width(30));
				int result = 0;
				if (int.TryParse(output, out result))
				{
					monsters[i].count = Mathf.Clamp(result, 0, 999);
				}

				if (GUILayout.Button("x"))
				{
					monsters.RemoveAt(i);
					--i;
				}

				EditorGUILayout.EndHorizontal();
			}

			if (GUILayout.Button("+"))
			{
				monsters.Add(new SpawnMob() { enemy = enemies[0] });
			}

			EditorGUILayout.EndVertical();
		}
		
		///////////////////////////////////////////////////////////////////////////

		void DrawStage(MonsterSpawnStage stage)
		{
			DrawEnemies(stage.monsters);

			GUILayout.Label("Duration");
			stage.duration = EditorGUILayout.IntField((int)stage.duration);
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawStage(CompletionStage stage)
		{
			GUILayout.Label("Completion");
			GUILayout.BeginHorizontal();

			stage.completion = EditorGUILayout.IntField(stage.completion, GUILayout.Width(100));
			GUILayout.Label("%");

			GUILayout.EndHorizontal();
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawStage(PauseStage stage)
		{
			GUILayout.Label("Pause");
			string output = GUILayout.TextField(stage.duration.ToString());
			int newValue = 0;
			if (int.TryParse(output, out newValue))
			{
				stage.duration = newValue;
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawStage(RewardStage stage)
		{
			GUILayout.Label("Reward");
			GUILayout.Label("Gold");

			int oldGoldCount = 0;

			foreach(ItemAndCount reward in stage.rewards)
			{
				if (reward.itemData == GameManager.instance.goldItemData)
				{
					oldGoldCount = reward.count;
				}
			}

			string output = GUILayout.TextField(oldGoldCount.ToString());
			int newValue = 0;

			if (int.TryParse(output, out newValue))
			{
				stage.rewards = new ItemAndCount[1];
				stage.rewards[0] = new ItemAndCount(GameManager.instance.goldItemData, newValue);
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void DrawStage(SpawnWaveStage stage)
		{
			if (stage is MonsterSpawnStage)
			{
				DrawStage(stage as MonsterSpawnStage);
			}
			else if (stage is CompletionStage)
			{
				DrawStage(stage as CompletionStage);
			}
			else if (stage is PauseStage)
			{
				DrawStage(stage as PauseStage);
			}
			else if (stage is RewardStage)
			{
				DrawStage(stage as RewardStage);
			}
		}

		///////////////////////////////////////////////////////////////////////////
	}
}