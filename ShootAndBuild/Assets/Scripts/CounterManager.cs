using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

using TCounterContext = System.String;

public enum CounterType
{
	KilledEnemies,
	ItemsUsed,
}

public struct CounterValue
{
	public int		CurrentCount;

	public void Init()
	{
		CurrentCount	= 0;
	}
}

public class CounterManager : MonoBehaviour 
{
	[System.NonSerialized]
	public const string NO_CONTEXT = "";
	public static int COUNTER_PER_PLAYER_COUNT = (int) PlayerID.Count + 1;

	// example:
	// If we kill an enemy, we increment
	// [KilledEnemies][specificType]++		//< counts kills of specific types
	// [KilledEnemies][null]++				//< counts sum of all kills / unspecific kills
	public Dictionary<CounterType, Dictionary<TCounterContext, CounterValue>>[] counters
	{
		get; private set;
	}

	void Awake()
	{
		instance = this;
		ResetAllCounters();
	}

	int PlayerIDToCounterIndex(PlayerID? srcPlayer)
	{
		if (!srcPlayer.HasValue || srcPlayer.Value == PlayerID.Count)
		{
			return (int) PlayerID.Count;
		}

		return (int) srcPlayer.Value;
	}

	void Start() 
	{
		
	}
	
	public void ResetAllCounters()
	{
		counters = new Dictionary<CounterType, Dictionary<TCounterContext, CounterValue>>[COUNTER_PER_PLAYER_COUNT];

		for (int i = 0; i < COUNTER_PER_PLAYER_COUNT; ++i)
		{
			counters[i] = new Dictionary<CounterType, Dictionary<TCounterContext, CounterValue>>();
		}
	}

	void Update() 
	{
		
	}

	public void AddToCounter(PlayerID? player, CounterType type, int delta, TCounterContext context = NO_CONTEXT, bool specificShouldIncludeUnspecificPlayer = true, bool specificShouldIncludeUnspecificContext = true)
	{
		if (specificShouldIncludeUnspecificPlayer && player.HasValue)
		{
			// also add to sum playerIndependent
			AddToCounter(null, type, delta, context);
		}

		if (specificShouldIncludeUnspecificContext && (context != NO_CONTEXT))
		{
			// also add to contextless counter
			AddToCounter(player, type, delta, NO_CONTEXT, false, false);
		}

		if (delta == 0)
		{
			Debug.LogWarning("Playercounter Change with delta 0");
			return;
		}

		int playerIndex = PlayerIDToCounterIndex(player);

		Dictionary<TCounterContext, CounterValue> contextToValue;
		bool foundDictionary = counters[playerIndex].TryGetValue(type, out contextToValue);

		if (!foundDictionary)
		{
			counters[playerIndex].Add(type, new Dictionary<TCounterContext, CounterValue>());
			contextToValue = counters[playerIndex][type];
		}

		CounterValue value;
		bool foundValue = contextToValue.TryGetValue(context, out value);

		if (!foundValue)
		{
			value.Init();
		}

		value.CurrentCount += delta;

		contextToValue[context] = value;

		OnValueChanged(type, delta, context);
	}

	public CounterValue GetCounterValue(PlayerID? player, CounterType type, TCounterContext contextObject = NO_CONTEXT)
	{
		CounterValue fallbackValue = new CounterValue();
		fallbackValue.Init();

		int playerIndex = PlayerIDToCounterIndex(player);

		Dictionary<string, CounterValue> contextToValue;
		bool foundDictionary = counters[playerIndex].TryGetValue(type, out contextToValue);

		if (!foundDictionary)
		{
			return fallbackValue;
		}

		CounterValue value;
		bool foundValue = contextToValue.TryGetValue(contextObject, out value);

		if (!foundValue)
		{
			return fallbackValue;
		}

		return value;
	}

	const float KillingSpreeInterval = 1.0f;

	// Here we can trigger CounterType specific stuff
	private void OnValueChanged(CounterType type, int delta, TCounterContext context = NO_CONTEXT)
	{

	}

	public static CounterManager instance
	{
		get; private set;
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////

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
	}
}