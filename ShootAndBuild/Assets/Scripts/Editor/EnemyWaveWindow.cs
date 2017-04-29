using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public class EnemyWaveWindow : EditorWindow
{

    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    private static SpawnManager managerInstance = null;
    private static EnemyWaveWindow windowInstance = null;

    private static readonly int MARGIN_WIDTH = 6;
    private static readonly int LINE_HEIGHT = 22;

    //----------------------------------------------------------------------

    // Add menu named "My Window" to the Window menu
    [MenuItem("ShootAndBuild-Tools/Enemy Wave Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        windowInstance = (EnemyWaveWindow)EditorWindow.GetWindow(typeof(EnemyWaveWindow));
        windowInstance.Show();
    }

    //----------------------------------------------------------------------

    void OnEnable()
    {
        //Debug.Log("enabele");
        managerInstance = FindObjectOfType<SpawnManager>();
        windowInstance = this;

    }

    //----------------------------------------------------------------------

    void OnFocus()
    {
        //Debug.Log("focus");
        managerInstance = FindObjectOfType<SpawnManager>();
    }

    //----------------------------------------------------------------------

    void OnGUI()
    {
        int yOffset = 0;
        int xOffset = 0;

        if (managerInstance == null)
        {
            GUIStyle style = new GUIStyle();
            style.normal.textColor = Color.red;
            EditorGUILayout.LabelField("No Spawnmanager found. You should check this.", style);
            return;
        }

        ////////////////////////////////////
        // Header Bar
        ////////////////////////////////////

        EditorGUILayout.BeginVertical();
        {
            EditorGUILayout.LabelField("Spawners");
            EditorGUILayout.Space();
        }
        EditorGUILayout.EndVertical();

        yOffset += LINE_HEIGHT + MARGIN_WIDTH;

        ////////////////////////////////////
        // Timeline Bar
        ////////////////////////////////////

        //scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Width(window.position.width - 6), GUILayout.Height(window.position.height - 40 - 55));
        //DrawGUI();//here we draw our GUI code
        //GUI.EndScrollView();

        //TODO list waves
        DrawWaves();

        for (int s = 0; s < managerInstance.spawners.Length; ++s)
        {


            Rect timeLineRect = new Rect(xOffset, yOffset, windowInstance.position.width - 60, windowInstance.position.height - yOffset );
            GUILayout.BeginArea(timeLineRect);

            int ySize;
            DrawSpawnerTimeline(s, out ySize);
            yOffset += ySize;   
            GUILayout.EndArea();
        }
        xOffset += 180  + managerInstance.waves.Count * (110 + MARGIN_WIDTH);



        ////////////////////////////////////
        // Add Wave button
        ////////////////////////////////////
        Rect buttonRect = new Rect(xOffset, LINE_HEIGHT + MARGIN_WIDTH, 80, 80);
        GUILayout.BeginArea(buttonRect);

        if (GUILayout.Button("Add Wave", GUILayout.Width(80),GUILayout.Height(80)))
        {
            managerInstance.waves.Add(new SpawnManager.EnemyWave(managerInstance.spawners.Length));

        }
        GUILayout.EndArea();

    }

    //----------------------------------------------------------------------

    private void  DrawSpawnerTimeline(int _spawnerIndex, out int _ySize)
    {
        _ySize = 0;

        EditorGUILayout.LabelField(managerInstance.spawners[_spawnerIndex].ID);
        _ySize += LINE_HEIGHT;

        //TODO have extra boxes for each spwawn block
        int xOffset = 180; //TODO limit length of spawer labels
        int maxYSize = 0;
        int waveWidth = 110; //TOD make constant?
        int ySize = 0;
        //waves row
        for (int w = 0; w < managerInstance.waves.Count; ++w )
        {
            //TODO x offset
            Rect buttonRect = new Rect(xOffset, 0, waveWidth, 80); //TODO height flexible
            GUILayout.BeginArea(buttonRect);

            EditorGUILayout.BeginVertical();
            {
                SpawnManager.SpawnPropabilityBlock currentSpb = managerInstance.waves[w].spawnPropability[_spawnerIndex];
                if (currentSpb.enemies == null || currentSpb.spawnRate == null)
                {
                    currentSpb.enemies = new List<SpawnManager.EnemyType>();
                    currentSpb.spawnRate = new List<float>();
                }

                ySize = 0;
                for (int e = 0; e < currentSpb.enemies.Count; ++e)
                {
                    //TODO have fixed rect for this
                    EditorGUILayout.BeginHorizontal(GUILayout.Width(80));
                    {
                        currentSpb.enemies[e] = (SpawnManager.EnemyType)EditorGUILayout.EnumPopup("", currentSpb.enemies[e], GUILayout.Width(60) );
                        currentSpb.spawnRate[e] = EditorGUILayout.FloatField("", currentSpb.spawnRate[e], GUILayout.Width(20) );
                    }

                    //remove entry
                    if (GUILayout.Button("X", GUILayout.Width(20)))
                    {
                        currentSpb.enemies.RemoveAt(e);
                        currentSpb.spawnRate.RemoveAt(e);
                    }

                    EditorGUILayout.EndHorizontal();
                    ySize += LINE_HEIGHT;
                } //for spawn block

                //TODO
                if (GUILayout.Button("Add"))
                {
                    currentSpb.enemies.Add(SpawnManager.EnemyType.Bat);
                    currentSpb.spawnRate.Add(1);
                }

                managerInstance.waves[w].spawnPropability[_spawnerIndex] = currentSpb;
            }
            EditorGUILayout.EndVertical();

            GUILayout.EndArea();
            xOffset += waveWidth + MARGIN_WIDTH;

            if (ySize > maxYSize)
            {
                maxYSize = ySize;
            }

        } //for waves
        _ySize += maxYSize;
        _ySize += MARGIN_WIDTH;

        EditorGUILayout.Space();


    }


    private void DrawWaves()
    {


        //TODO have extra boxes for each spwawn block
        int xOffset = 180; //TODO limit length of spawer labels
        int maxYSize = 0;
        int waveWidth = 110; //TOD make constant?
        int ySize = 0;
        //waves row
        EditorGUILayout.BeginHorizontal();
        for (int w = 0; w < managerInstance.waves.Count; ++w)
        {
            //TODO have fixed rect for this
            EditorGUILayout.BeginHorizontal(GUILayout.Width(waveWidth));
            {
                EditorGUILayout.LabelField("Wave " + (w +1));

                //remove entry
                if (GUILayout.Button("X", GUILayout.Width(20)))
                {
                    managerInstance.waves.RemoveAt(w);

                }
            }
            EditorGUILayout.EndHorizontal();

        } //for waves
        EditorGUILayout.EndHorizontal();


    }

}