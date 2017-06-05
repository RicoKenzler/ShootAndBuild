using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using SAB.Spawn;

namespace SAB
{

    [CustomEditor(typeof(SpawnManager))]
    public class SpawnManagerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            SpawnManager editorTarget = (SpawnManager)this.target;



            EditorGUILayout.LabelField("Enemy Templates", EditorStyles.boldLabel);

            //enemy templates
            string[] enemyNames = Enum.GetNames(typeof(EnemyType));

            if (editorTarget.enemyTemplates == null)
            {
                editorTarget.enemyTemplates = new EnemyBehaviourBase[enemyNames.Length];
            }

            if (editorTarget.enemyTemplates.Length != enemyNames.Length)
            {
                EnemyBehaviourBase[] oldEnemyTemplates = new EnemyBehaviourBase[editorTarget.enemyTemplates.Length];
                Array.Copy(editorTarget.enemyTemplates, oldEnemyTemplates, editorTarget.enemyTemplates.Length);

                editorTarget.enemyTemplates = new EnemyBehaviourBase[enemyNames.Length];

                Array.Copy(oldEnemyTemplates, editorTarget.enemyTemplates, Math.Min(editorTarget.enemyTemplates.Length, oldEnemyTemplates.Length));
            }

            for (int i = 0; i < enemyNames.Length; ++i)
            {

                if (i == 0)
                {
                    continue;
                }

                EditorGUILayout.BeginHorizontal();
                {

                    EditorGUILayout.LabelField(enemyNames[i]);
                    editorTarget.enemyTemplates[i] = (EnemyBehaviourBase)EditorGUILayout.ObjectField(editorTarget.enemyTemplates[i], typeof(EnemyBehaviourBase), false);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            this.DrawDefaultInspector();

        }
    }
}