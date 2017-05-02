using System;
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

        public static SpawnManager Instance
        {
            get; private set;
        }

        //----------------------------------------------------------------------

        private float waveTimer = 0;
        private int currentWaveIndex = 0;

        //----------------------------------------------------------------------

        //[HideInInspector]
        public List<EnemyWave> waves;

        //----------------------------------------------------------------------

        public EnemySpawner[] spawners;

        //TODO write editor and match with enemy type enum
        public EnemyBehaviour[] enemyTemplates;

        //----------------------------------------------------------------------

        void Awake()
        {

            Instance = this;

            if (spawners == null || spawners.Length == 0)
            {
                Debug.LogError("No Spawners assigned!");
            }
        }

        //----------------------------------------------------------------------

        // Use this for initialization
        void Start()
        {

            if (this.waves == null || this.waves.Count == 0)
            {
                Debug.LogError("no waves setup");
                return;
            }

            this.InitWave();

        }

        //----------------------------------------------------------------------

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance.Status == GameStatus.Running)

                if (this.waves == null || this.waves.Count == 0)
                {
                    Debug.LogError("no waves setup");
                    return;
                }


            waveTimer += Time.deltaTime;

            //next wave
            if (waveTimer > this.waves[currentWaveIndex].duration)
            {
                this.NextWave();
            }

        }

        //----------------------------------------------------------------------

        private void NextWave()
        {
            currentWaveIndex++;
            if (currentWaveIndex >= this.waves.Count)
            {
                currentWaveIndex = 0;
                Debug.LogWarning("reached end of waves. starting over.");
            }

            Debug.Log("Wave " + this.currentWaveIndex);

            this.InitWave();

        }

        //----------------------------------------------------------------------

        private void InitWave()
        {

            waveTimer = 0;

            EnemyWave currentWave = this.waves[currentWaveIndex];
            for (int s = 0; s < this.spawners.Length; s++)
            {
                this.spawners[s].SetSpawnRate(currentWave.spawnPropability[s]);
            }
        }

        //----------------------------------------------------------------------

        public GameObject GetEnemyTemplate(EnemyType _type)
        {
            return this.enemyTemplates[(int)_type].gameObject;
        }


    }
}