using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace SAB.Spawn
{

    ///////////////////////////////////////////////////////////////////////////

    //TODO maybe expand this to something much bigger with update logic, conditions, etc..
    [Serializable]
    public class EnemyWave
    {
		public EnemyWave(int _spwanerCount)
        {
            //index will be spwaner id
            spawnPropability = new SpawnPropabilityBlock[_spwanerCount];
            for (int i = 0; i < spawnPropability.Length; ++i)
            {
                spawnPropability[i] = new SpawnPropabilityBlock();
            }
            duration = 30f;

        }

        public float duration;
        public SpawnPropabilityBlock[] spawnPropability;
    }

    ///////////////////////////////////////////////////////////////////////////

    [Serializable]
    public class SpawnPropabilityBlock
    {
        public SpawnPropabilityBlock()
        {
            this.m_Enemies = new List<EnemyType>();
            this.m_SpawnRate = new List<float>();
        }

        [SerializeField] private List<EnemyType>	m_Enemies;
        [SerializeField] private List<float>		m_SpawnRate;

		public List<EnemyType>	enemies		{ get { return m_Enemies; }		set { m_Enemies = value; } }
		public List<float>		spawnRate	{ get { return m_SpawnRate; }	set { m_SpawnRate = value; } }

    }

    ///////////////////////////////////////////////////////////////////////////

    public class SpawnManager : MonoBehaviour
    {
        public static SpawnManager instance
        {
            get; private set;
        }

        ///////////////////////////////////////////////////////////////////////////

        private float m_WaveTimer = 0;
        private int m_CurrentWaveIndex = -1;

        ///////////////////////////////////////////////////////////////////////////

		[FormerlySerializedAs("waves")]
        [SerializeField] private List<EnemyWave> m_Waves;

        ///////////////////////////////////////////////////////////////////////////

		[FormerlySerializedAs("spawners")]
        [SerializeField] private EnemySpawner[] m_Spawners;

        //TODO write editor and match with enemy type enum
		[FormerlySerializedAs("enemyTemplates")]
        [SerializeField] private EnemyBehaviourBase[] m_EnemyTemplates;

		[FormerlySerializedAs("newWaveSound")]
		[SerializeField] private AudioData m_NewWaveSound;

		[FormerlySerializedAs("finishedWaveSound")]
		[SerializeField] private AudioData m_FinishedWaveSound;

        ///////////////////////////////////////////////////////////////////////////

		public List<EnemyWave> waves				{ get { return m_Waves; } }
		public EnemySpawner[] spawners				{ get { return m_Spawners; } }
		public EnemyBehaviourBase[] enemyTemplates	{ get { return m_EnemyTemplates; }  set { m_EnemyTemplates = value; }}

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;

            if (m_Spawners == null || m_Spawners.Length == 0)
            {
                Debug.LogError("No Spawners assigned!");
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        // Update is called once per frame
        void Update()
        {
			if (!PlayerManager.instance.HasPlayerJoined(PlayerID.Player1))
			{
				// No Waves until player is ready
				return;
			}

            if (GameManager.Instance.Status == GameStatus.Running)
			{
                if (this.m_Waves == null || this.m_Waves.Count == 0)
                {
                    Debug.LogError("no waves setup");
                    return;
                }
			}

            m_WaveTimer += Time.deltaTime;

            //next wave
            if (m_CurrentWaveIndex == -1 || m_WaveTimer > this.m_Waves[m_CurrentWaveIndex].duration)
            {
                this.NextWave();
            }

        }

        ///////////////////////////////////////////////////////////////////////////

        public void NextWave()
        {
            m_CurrentWaveIndex++;
            if (m_CurrentWaveIndex >= this.m_Waves.Count)
            {
                m_CurrentWaveIndex = 0;
				NotificationManager.instance.ShowNotification(new Notification("Restarting Waves", NotificationType.NeutralNews));
            }

            this.InitWave();
        }

        ///////////////////////////////////////////////////////////////////////////

        private void InitWave()
        {
            m_WaveTimer = 0;

			bool isEmptyWave = true;

            EnemyWave currentWave = this.m_Waves[m_CurrentWaveIndex];
            for (int s = 0; s < this.m_Spawners.Length; s++)
            {
                this.m_Spawners[s].SetSpawnRate(currentWave.spawnPropability[s]);
				isEmptyWave &= (currentWave.spawnPropability[s].enemies.Count == 0);
            }

			if (isEmptyWave)
			{
				AudioManager.instance.PlayAudio(m_FinishedWaveSound);
				NotificationManager.instance.ShowNotification(new Notification("Wave End", NotificationType.NeutralNews));
			}
			else
			{
				AudioManager.instance.PlayAudio(m_NewWaveSound);
				NotificationManager.instance.ShowNotification(new Notification("Wave " + m_CurrentWaveIndex, NotificationType.BadNews));
			}
        }

        ///////////////////////////////////////////////////////////////////////////

        public GameObject GetEnemyTemplate(EnemyType _type)
        {
            return this.m_EnemyTemplates[(int)_type].gameObject;
        }
    }
}