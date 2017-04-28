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
	public float	LastChange;

	public void Init()
	{
		CurrentCount	= 0;
		LastChange		= 0.0f;
	}
}

public class CounterManager : MonoBehaviour 
{
	public const System.String NO_CONTEXT = "";

	// example:
	// If we kill an enemy, we increment
	// [KilledEnemies][specificType]++		//< counts kills of specific types
	// [KilledEnemies][null]++				//< counts sum of all kills / unspecific kills
	public Dictionary<CounterType, Dictionary<TCounterContext, CounterValue>> counters
	{
		get; private set;
	}

	void Awake()
	{
		instance = this;
		ResetAllCounters();
	}

	void Start() 
	{
		
	}
	
	public void ResetAllCounters()
	{
		counters = new Dictionary<CounterType, Dictionary<TCounterContext, CounterValue>>();
	}

	void Update() 
	{
		
	}

	public void AddToCounter(CounterType type, int delta, TCounterContext context = NO_CONTEXT)
	{
		if (context != NO_CONTEXT)
		{
			// also add to contextless counter
			AddToCounter(type, delta, NO_CONTEXT);
		}

		if (delta == 0)
		{
			Debug.LogWarning("Playercounter Change with delta 0");
			return;
		}

		Dictionary<TCounterContext, CounterValue> contextToValue;
		bool foundDictionary = counters.TryGetValue(type, out contextToValue);

		if (!foundDictionary)
		{
			counters.Add(type, new Dictionary<TCounterContext, CounterValue>());
			contextToValue = counters[type];
		}

		CounterValue value;
		bool foundValue = contextToValue.TryGetValue(context, out value);

		if (!foundValue)
		{
			value.Init();
		}

		value.CurrentCount += delta;
		value.LastChange = Time.time;

		contextToValue[context] = value;
	}

	public CounterValue GetCounterValue(CounterType type, TCounterContext contextObject = NO_CONTEXT)
	{
		CounterValue fallbackValue = new CounterValue();
		fallbackValue.Init();

		Dictionary<string, CounterValue> contextToValue;
		bool foundDictionary = counters.TryGetValue(type, out contextToValue);

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

	public static CounterManager instance
	{
		get; private set;
	}
}

////////////////////////////////////////////////////////////////////////////////////////////////

[CustomEditor(typeof(CounterManager))]
public class CounterManagerEditor : Editor
{
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

		foreach (CounterType type in System.Enum.GetValues(typeof(CounterType)))
		{
			CounterValue fallbackValue = new CounterValue();
			fallbackValue.Init();

			Dictionary<TCounterContext, CounterValue> contextToValue;
			bool foundDictionary = counterManager.counters.TryGetValue(type, out contextToValue);

			if (!foundDictionary)
			{
				GUILayout.Label(type + ": /");
				continue;
			}

			GUILayout.Label(type + ":");

			foreach (KeyValuePair<string, CounterValue> contextValuePair in contextToValue)
			{
				string keyString = (contextValuePair.Key == CounterManager.NO_CONTEXT) ? "Total" : contextValuePair.Key;
				string valueString = contextValuePair.Value.CurrentCount + "      (" + contextValuePair.Value.LastChange + ")";

				GUILayout.Label("- " + keyString + ": " + valueString);
			}		
		}

		GUILayout.Label("All Counters:", EditorStyles.boldLabel);
	}
}