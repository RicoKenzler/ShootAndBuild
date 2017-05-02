﻿using UnityEngine;
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
                editorTarget.enemyTemplates = new EnemyBehaviour[enemyNames.Length];
            }

            if (editorTarget.enemyTemplates.Length != enemyNames.Length)
            {
                GameObject[] oldEnemyTemplates = new GameObject[editorTarget.enemyTemplates.Length];
                Array.Copy(editorTarget.enemyTemplates, oldEnemyTemplates, editorTarget.enemyTemplates.Length);

                editorTarget.enemyTemplates = new EnemyBehaviour[enemyNames.Length];

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
                    editorTarget.enemyTemplates[i] = (EnemyBehaviour)EditorGUILayout.ObjectField(editorTarget.enemyTemplates[i], typeof(EnemyBehaviour), false);
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.Space();

            this.DrawDefaultInspector();

        }
    }
}