using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB
{
    [System.Serializable]
    public struct KillingSpreeDefinition
    {
        public int KillCount;
        public string Title;
        public AudioData AudioMessage;
    }

	///////////////////////////////////////////////////////////////////////////

    public class KillCounters : MonoBehaviour
    {
		[FormerlySerializedAs("killingSpreeDefinitions")]
        [SerializeField] private KillingSpreeDefinition[]	m_KillingSpreeDefinitions;
        [SerializeField] private float						m_KillingSpreeInterval = 2.0f;

		///////////////////////////////////////////////////////////////////////////

        private KillCounter[] m_KillCounters;

		///////////////////////////////////////////////////////////////////////////

		public KillingSpreeDefinition[] killingSpreeDefinitions { get { return m_KillingSpreeDefinitions; } }
		public float					killingSpreeInterval	{ get { return m_KillingSpreeInterval; } }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;

            m_KillCounters = new KillCounter[(int)PlayerID.Count];

            for (int i = 0; i < m_KillCounters.Length; ++i)
            {
                m_KillCounters[i] = new KillCounter();
                m_KillCounters[i].playerID = (PlayerID)i;
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
            for (int p = 0; p < (int)PlayerID.Count; ++p)
            {
                PlayerID playerID = (PlayerID)p;

                if (!PlayerManager.instance.HasPlayerJoined(playerID))
                {
                    continue;
                }

                int killCount = CounterManager.instance.GetCounterValue(playerID, CounterType.KilledEnemies, "").CurrentCount;

                m_KillCounters[(int)playerID].OnCountersChanged(Time.time, killCount);
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
        public PlayerID playerID;

        //                 . |K  KK .  .  . |K  K
        // Time            0  1  2  3  4  5  6  7
        // Count           0  1  3  3  3  3  4  5 
        // LastKillTime     0  1  2  2  2  2  6  7  
        // LastKillCount    -  1  3  3  3  3  4  5
        // LastKillCountBIS -  0  0  0  0  0  2  2    
        float lastKillTime = 0.0f;
        int lastKillCount = 0;
        int lastKillCountBeforeIntervalStart = 0;
        int lastSpreeLevelThisInterval = -1;

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
                lastKillCountBeforeIntervalStart = lastKillCount;
                lastSpreeLevelThisInterval = -1;
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

                if (definition.AudioMessage)
                {
                    AudioManager.instance.PlayAudio(definition.AudioMessage);
                }
            }
        }
    }
}