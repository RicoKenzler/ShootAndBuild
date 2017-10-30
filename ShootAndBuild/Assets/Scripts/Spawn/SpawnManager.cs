﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Spawn
{

    //----------------------------------------------------------------------

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

    //----------------------------------------------------------------------

    [Serializable]
    public class SpawnPropabilityBlock
    {
        public SpawnPropabilityBlock()
        {
            this.enemies = new List<EnemyType>();
            this.spawnRate = new List<float>();
        }

        public List<EnemyType> enemies;
        public List<float> spawnRate;

    }

    //----------------------------------------------------------------------

    public class SpawnManager : MonoBehaviour
    {

        public static SpawnManager instance
        {
            get; private set;
        }

        //----------------------------------------------------------------------

        private float waveTimer = 0;
        private int currentWaveIndex = -1;

        //----------------------------------------------------------------------

        //[HideInInspector]
        public List<EnemyWave> waves;

        //----------------------------------------------------------------------

        public EnemySpawner[] spawners;

        //TODO write editor and match with enemy type enum
        public EnemyBehaviourBase[] enemyTemplates;

		public AudioData newWaveSound;
		public AudioData finishedWaveSound;

        //----------------------------------------------------------------------

        void Awake()
        {
            instance = this;

            if (spawners == null || spawners.Length == 0)
            {
                Debug.LogError("No Spawners assigned!");
            }
        }

        //----------------------------------------------------------------------

        // Use this for initialization
        void Start()
        {
			
        }

        //----------------------------------------------------------------------

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
                if (this.waves == null || this.waves.Count == 0)
                {
                    Debug.LogError("no waves setup");
                    return;
                }
			}

            waveTimer += Time.deltaTime;

            //next wave
            if (currentWaveIndex == -1 || waveTimer > this.waves[currentWaveIndex].duration)
            {
                this.NextWave();
            }

        }

        //----------------------------------------------------------------------

        public void NextWave()
        {
            currentWaveIndex++;
            if (currentWaveIndex >= this.waves.Count)
            {
                currentWaveIndex = 0;
				NotificationManager.instance.ShowNotification(new Notification("Restarting Waves", NotificationType.NeutralNews));
            }

            this.InitWave();
        }

        //----------------------------------------------------------------------

        private void InitWave()
        {
            waveTimer = 0;

			bool isEmptyWave = true;

            EnemyWave currentWave = this.waves[currentWaveIndex];
            for (int s = 0; s < this.spawners.Length; s++)
            {
                this.spawners[s].SetSpawnRate(currentWave.spawnPropability[s]);
				isEmptyWave &= (currentWave.spawnPropability[s].enemies.Count == 0);
            }

			if (isEmptyWave)
			{
				AudioManager.instance.PlayAudio(finishedWaveSound);
				NotificationManager.instance.ShowNotification(new Notification("Wave End", NotificationType.NeutralNews));
			}
			else
			{
				AudioManager.instance.PlayAudio(newWaveSound);
				NotificationManager.instance.ShowNotification(new Notification("Wave " + currentWaveIndex, NotificationType.BadNews));
			}
        }

        //----------------------------------------------------------------------

        public GameObject GetEnemyTemplate(EnemyType _type)
        {
            return this.enemyTemplates[(int)_type].gameObject;
        }
    }
}