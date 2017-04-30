using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct KillingSpreeDefinition
{
	public int		KillCount;
	public string	Title; 
}

public class KillCounters : MonoBehaviour 
{
	public KillingSpreeDefinition[]	killingSpreeDefinitions;
	public float					killingSpreeInterval		= 3.0f;

	KillCounter[] killCounters;

	void Awake()
	{
		instance = this;

		killCounters = new KillCounter[(int) PlayerID.Count];

		for (int i = 0; i < killCounters.Length; ++i)
		{
			killCounters[i] = new KillCounter();
		}
	}

	void Start() 
	{
		CounterManager.instance.OnCountersChanged += OnCountersChanged;
	}
	
	void Update() 
	{
		
	}

	void OnCountersChanged()
	{
		for (int p = 0; p < (int) PlayerID.Count; ++p)
		{
			PlayerID playerID = (PlayerID) p;

			if (!PlayerManager.instance.HasPlayerJoined(playerID))
			{
				continue;
			}

			int killCount = CounterManager.instance.GetCounterValue(playerID, CounterType.KilledEnemies, "").CurrentCount;

			killCounters[(int) playerID].OnCountersChanged(Time.time, killCount);
		}
	}

	public static KillCounters instance
	{
		get; private set;
	}
}

//////////////////////////////////////////////////////////////

public class KillCounter
{
	//                 . |K  KK .  .  . |K  K
	// Time            0  1  2  3  4  5  6  7
	// Count           0  1  3  3  3  3  4  5 
	// LastKillTime     0  1  2  2  2  2  6  7  
	// LastKillCount    -  1  3  3  3  3  4  5
	// LastKillCountBIS -  0  0  0  0  0  2  2    
	
	float lastKillTime						= 0.0f;
	int	  lastKillCount						= 0;
	int   lastKillCountBeforeIntervalStart	= 0;
	int	  lastSpreeLevelThisInterval		= -1;

	public void OnCountersChanged(float currentTime, int currentKillCount)
	{
		if (currentKillCount == lastKillCount)
		{
			return;
		}

		// so we had new kills....
		float elapsedTime = currentTime - lastKillTime;

		lastKillTime = currentTime;

		if (elapsedTime > KillCounters.instance.killingSpreeInterval)
		{
			// start new interval
			lastKillCountBeforeIntervalStart	= lastKillCount;
			lastSpreeLevelThisInterval			= -1;
		}

		int killsThisInterval = currentKillCount - lastKillCountBeforeIntervalStart;
		
		lastKillCount = currentKillCount;

		KillingSpreeDefinition[] killingSpreeDefinitions = KillCounters.instance.killingSpreeDefinitions;

		int triggerLevel = -1;

		for (int lvl = lastSpreeLevelThisInterval + 1; lvl < killingSpreeDefinitions.Length; ++lvl)
		{
			KillingSpreeDefinition definition = killingSpreeDefinitions[lvl];

			if (killsThisInterval < definition.KillCount)
			{
				break;
			}

			triggerLevel = lvl;
		}

		if (triggerLevel != -1)
		{
			lastSpreeLevelThisInterval = triggerLevel;

			KillingSpreeDefinition definition = killingSpreeDefinitions[triggerLevel];

			Debug.Log("Killing Spree: " + definition.Title + " (" + killsThisInterval + ")");
		}
	}
}