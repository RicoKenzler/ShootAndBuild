using SAB.Spawn;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAB
{
    public class EnemyWaveWindow : EditorWindow
    {
        private static SpawnManager s_ManagerInstance = null;
        private static EnemyWaveWindow s_WindowInstance = null;

        private static readonly int MARGIN_WIDTH = 6;
        private static readonly int LINE_HEIGHT = 22;
        private static readonly int WAVE_WIDTH = 60 + 40 + 20 +MARGIN_WIDTH +MARGIN_WIDTH; 

        ///////////////////////////////////////////////////////////////////////////

        // Add menu named "My Window" to the Window menu
        [MenuItem("ShootAndBuild-Tools/Enemy Wave Editor")]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            s_WindowInstance = (EnemyWaveWindow)EditorWindow.GetWindow(typeof(EnemyWaveWindow));
            s_WindowInstance.Show();
        }

        ///////////////////////////////////////////////////////////////////////////

        void OnEnable()
        {
            //Debug.Log("enabele");
            s_ManagerInstance = FindObjectOfType<SpawnManager>();
            s_WindowInstance = this;

        }

        ///////////////////////////////////////////////////////////////////////////

        void OnFocus()
        {
            //Debug.Log("focus");
            s_ManagerInstance = FindObjectOfType<SpawnManager>();
        }

        ///////////////////////////////////////////////////////////////////////////

        void OnGUI()
        {
            int xOffset = 0;
            int yOffset = MARGIN_WIDTH;

            if (s_ManagerInstance == null)
            {
                GUIStyle style = new GUIStyle();
                style.normal.textColor = Color.red;
                EditorGUILayout.LabelField("No Spawnmanager found. You should check this.", style);
                return;
            }

            ////////////////////////////////////
            // Header Bar
            ////////////////////////////////////

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical();
            {
                EditorGUILayout.LabelField("Spawners", EditorStyles.boldLabel);
                EditorGUILayout.Space();
            }
            EditorGUILayout.EndVertical();


            ////////////////////////////////////
            // Timeline Bar
            ////////////////////////////////////

            //scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(window.position.width - 6), GUILayout.Height(window.position.height - 40 - 55));
            //DrawGUI();//here we draw our GUI code
            //GUI.EndScrollView();

            //TODO list waves
            Rect wavesRect = new Rect(xOffset, yOffset, s_WindowInstance.position.width -MARGIN_WIDTH, LINE_HEIGHT );
            GUILayout.BeginArea(wavesRect);
            DrawWaves();
            GUILayout.EndArea();

            yOffset += LINE_HEIGHT + MARGIN_WIDTH;

            for (int s = 0; s < s_ManagerInstance.spawners.Length; ++s)
            {


                Rect timeLineRect = new Rect(xOffset, yOffset, s_WindowInstance.position.width - MARGIN_WIDTH,  LINE_HEIGHT * 6); //TODO find a way to place height
                GUILayout.BeginArea(timeLineRect);

                int ySize;
                DrawSpawnerTimeline(s, out ySize);
                yOffset += ySize;
                GUILayout.EndArea();

           

            }
            xOffset += 180 + s_ManagerInstance.waves.Count * (WAVE_WIDTH + MARGIN_WIDTH);



            ////////////////////////////////////
            // Add Wave button
            ////////////////////////////////////
            Rect buttonRect = new Rect(xOffset, LINE_HEIGHT + MARGIN_WIDTH, WAVE_WIDTH, yOffset);
            GUILayout.BeginArea(buttonRect);

            if (GUILayout.Button("Add Wave", GUILayout.Width(WAVE_WIDTH), GUILayout.Height(yOffset)))
            {
                s_ManagerInstance.waves.Add(new EnemyWave(s_ManagerInstance.spawners.Length));

            }
            GUILayout.EndArea();
		}

        ///////////////////////////////////////////////////////////////////////////

        private void DrawSpawnerTimeline(int _spawnerIndex, out int _ySize)
        {
            _ySize = 0;

            EditorGUILayout.LabelField(" "+ s_ManagerInstance.spawners[_spawnerIndex].ID); //hackedy hack
            _ySize += LINE_HEIGHT;

            //TODO have extra boxes for each spwawn block
            int xOffset = 180; //TODO limit length of spawer labels
            int maxYSize = 0;

            int ySize = 0;
            //waves row
            for (int w = 0; w < s_ManagerInstance.waves.Count; ++w)
            {
                //TODO x offset
                Rect buttonRect = new Rect(xOffset, 0, WAVE_WIDTH, 80); //TODO height flexible
                GUILayout.BeginArea(buttonRect);

                EditorGUILayout.BeginVertical();
                {
                    SpawnPropabilityBlock currentSpb = s_ManagerInstance.waves[w].spawnPropability[_spawnerIndex];
                    if (currentSpb.enemies == null || currentSpb.spawnRate == null)
                    {
                        currentSpb.enemies = new List<EnemyType>();
                        currentSpb.spawnRate = new List<float>();
                    }

                    ySize = 0;
                    for (int e = 0; e < currentSpb.enemies.Count; ++e)
                    {
                        //TODO have fixed rect for this
                        EditorGUILayout.BeginHorizontal(GUILayout.Width(80));
                        {
                            currentSpb.enemies[e]   = (EnemyType)EditorGUILayout.EnumPopup("", currentSpb.enemies[e], GUILayout.Width(60));
                            currentSpb.spawnRate[e] = EditorGUILayout.FloatField("", currentSpb.spawnRate[e], GUILayout.Width(40));

                            //remove entry
                            if (GUILayout.Button("X", GUILayout.Width(20)))
                            {
                                currentSpb.enemies.RemoveAt(e);
                                currentSpb.spawnRate.RemoveAt(e);
                            }

                        }
                        EditorGUILayout.EndHorizontal();
                        ySize += LINE_HEIGHT;
                    } //for spawn block

                    //TODO
                    if (GUILayout.Button("Add"))
                    {
                        currentSpb.enemies.Add(EnemyType.Bat);
                        currentSpb.spawnRate.Add(1);
                    }

                    s_ManagerInstance.waves[w].spawnPropability[_spawnerIndex] = currentSpb;
                }
                EditorGUILayout.EndVertical();

                GUILayout.EndArea();
                xOffset += WAVE_WIDTH + MARGIN_WIDTH;

                if (ySize > maxYSize)
                {
                    maxYSize = ySize;
                }

            } //for waves
            _ySize += maxYSize;
            _ySize += MARGIN_WIDTH;

            Rect sepeartorRect = new Rect(MARGIN_WIDTH, _ySize, xOffset - MARGIN_WIDTH - MARGIN_WIDTH, 1); //TODO height flexible
            GUILayout.BeginArea(sepeartorRect);
            SABEditorGUI.Seperator();
            GUILayout.EndArea();

            _ySize += MARGIN_WIDTH;
        }

        ///////////////////////////////////////////////////////////////////////////

        private void DrawWaves()
        {

            //TODO have extra boxes for each spwawn block
            int xOffset = 180; //TODO limit length of spawer labels
            //waves row

            for (int w = 0; w < s_ManagerInstance.waves.Count; ++w)
            {

                Rect buttonRect = new Rect(xOffset, 0, WAVE_WIDTH, LINE_HEIGHT); //TODO height flexible
                GUILayout.BeginArea(buttonRect);

                EditorGUILayout.BeginHorizontal(GUILayout.Width(WAVE_WIDTH));
                {
                    EditorGUILayout.LabelField("Wave " + (w + 1), GUILayout.Width(55) );
                    s_ManagerInstance.waves[w].duration = EditorGUILayout.FloatField("", s_ManagerInstance.waves[w].duration, GUILayout.Width(30));
                    EditorGUILayout.LabelField("s", GUILayout.Width(10));

                    GUI.backgroundColor = new Color(1, 0.5f, 0.5f);
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        s_ManagerInstance.waves.RemoveAt(w);

                    }
                    GUI.backgroundColor = Color.white;

                }
                EditorGUILayout.EndHorizontal();

                GUILayout.EndArea();
                xOffset += WAVE_WIDTH + MARGIN_WIDTH;

            } //for waves



        }

    }
}