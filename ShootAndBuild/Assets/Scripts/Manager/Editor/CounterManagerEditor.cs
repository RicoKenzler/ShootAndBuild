using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

using TCounterContext = System.String;

namespace SAB
{
	 [CustomEditor(typeof(CounterManager))]
    public class CounterManagerEditor : Editor
    {
        private void OnEnable()
        {
            EditorApplication.update += UpdateWhenVisible;
        }

        private void OnDisable()
        {
            EditorApplication.update -= UpdateWhenVisible;
        }

        private void UpdateWhenVisible()
        {
            Repaint();
        }

        bool[] playerFoldout = new bool[CounterManager.COUNTER_PER_PLAYER_COUNT];

        public override void OnInspectorGUI()
        {
            CounterManager counterManager = (CounterManager)target;

            DrawDefaultInspector();

            if (!Application.isPlaying)
            {
                return;
            }

            if (GUILayout.Button("Reset Counters"))
            {
                counterManager.ResetAllCounters();
            }

            GUILayout.Label("All Counters:", EditorStyles.boldLabel);

            for (int playerIndex = 0; playerIndex < (int)(CounterManager.COUNTER_PER_PLAYER_COUNT); ++playerIndex)
            {
                PlayerID playerID = (PlayerID)playerIndex;

                if ((playerID != PlayerID.Count) && !PlayerManager.instance.HasPlayerJoined(playerID))
                {
                    // only show active players
                    continue;
                }

                string playerName = playerIndex < (int)PlayerID.Count ? ((PlayerID)playerIndex).ToString() : "Total";
                playerFoldout[playerIndex] = EditorGUILayout.Foldout(playerFoldout[playerIndex], playerName);

                if (!playerFoldout[playerIndex])
                {
                    // collapse separate players
                    continue;
                }

                foreach (CounterType type in System.Enum.GetValues(typeof(CounterType)))
                {
                    CounterValue fallbackValue = new CounterValue();
                    fallbackValue.Init();

                    Dictionary<TCounterContext, CounterValue> contextToValue;
                    bool foundDictionary = counterManager.counters[playerIndex].TryGetValue(type, out contextToValue);

                    if (!foundDictionary)
                    {
                        // Nothing happened to this counter yet
                        continue;
                    }

                    GUILayout.Label(type + ":");

                    foreach (KeyValuePair<string, CounterValue> contextValuePair in contextToValue)
                    {
                        string keyString = (contextValuePair.Key == CounterManager.NO_CONTEXT) ? "Total" : contextValuePair.Key;
                        string valueString = contextValuePair.Value.CurrentCount.ToString();

                        GUILayout.Label("- " + keyString + ": " + valueString, EditorStyles.miniLabel);
                    }
                }
            }

            if (counterManager.countersAreDirty)
            {
                GUILayout.Label("Dirty");
            }
        }
    }
}
