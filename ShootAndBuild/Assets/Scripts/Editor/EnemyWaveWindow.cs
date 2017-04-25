using UnityEngine;
using UnityEditor;

public class EnemyWaveWindow : EditorWindow
{

    string myString = "Hello World";
    bool groupEnabled;
    bool myBool = true;
    float myFloat = 1.23f;

    private static SpawnManager managerInstance = null;

    //----------------------------------------------------------------------
    
        // Add menu named "My Window" to the Window menu
    [MenuItem("ShootAndBuild-Tools/Enemy Wave Editor")]
    static void Init()
    {
        // Get existing open window or if none, make a new one:
        EnemyWaveWindow window = (EnemyWaveWindow)EditorWindow.GetWindow(typeof(EnemyWaveWindow));
        window.Show();
    }

    //----------------------------------------------------------------------

    void OnEnable()
    {
        //Debug.Log("enabele");
        managerInstance = FindObjectOfType<SpawnManager>();

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
        if (managerInstance == null)
        {
            return;
        }


        for (int s = 0; s < managerInstance.spawners.Length; ++s)
        {
            EditorGUILayout.BeginHorizontal();
            {
                EditorGUILayout.BeginVertical();
                {
                    EditorGUILayout.LabelField( managerInstance.spawners[s].ID);
                }
                EditorGUILayout.EndVertical();

            }
            EditorGUILayout.EndHorizontal();
        }


        GUILayout.Label("Base Settings", EditorStyles.boldLabel);
        myString = EditorGUILayout.TextField("Text Field", myString);

        groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
        myBool = EditorGUILayout.Toggle("Toggle", myBool);
        myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
        EditorGUILayout.EndToggleGroup();
    }
}