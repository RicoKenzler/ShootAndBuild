using UnityEditor;
using UnityEngine;

namespace SAB.Spawn
{

    public class EnemySpawner : MonoBehaviour
    {
        public float spawnRadius = 3;

        private SpawnPropabilityBlock currentSpwanBlock = null;
        private float[] spawnTimer = null;

        //??
        int totalSpawnCount = 0;

        //----------------------------------------------------------------------

        public string ID
        {
            get
            {
                return this.gameObject.name;
            }
        }

        //----------------------------------------------------------------------

        void Start()
        {

        }

        //----------------------------------------------------------------------

        void Update()
        {

            if (currentSpwanBlock == null)
            {
                return;
            }


            for (int i = 0; i < spawnTimer.Length; ++i)
            {
                spawnTimer[i] += Time.deltaTime * this.currentSpwanBlock.spawnRate[i];

                while (spawnTimer[i] >= 1)
                {
                    spawnTimer[i] -= 1;
                    if (CheatManager.instance.stopEnemySpawns)
                    {
                        return;
                    }

                    Spawn(this.currentSpwanBlock.enemies[i]);
                }
            }
        }

        //----------------------------------------------------------------------

        public void ForceImmediateSpawn()
        {
            Spawn(EnemyType.Bat);
        }

        //----------------------------------------------------------------------

        void Spawn(EnemyType _enemy)
        {
            GameObject enemyPrefab = SpawnManager.instance.GetEnemyTemplate(_enemy);

            if (enemyPrefab != null)
            {
                totalSpawnCount++;
                Vector3 spawnPosition = this.transform.position + Random.insideUnitSphere *spawnRadius;
                spawnPosition.y = this.transform.position.y;
                GameObject enemyInstance = GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                enemyInstance.name = enemyPrefab.name + " " + totalSpawnCount;
            }
        }

        //----------------------------------------------------------------------


        public void SetSpawnRate(SpawnPropabilityBlock _spawnData)
        {

            this.currentSpwanBlock = _spawnData;
            this.spawnTimer = new float[this.currentSpwanBlock.spawnRate.Count];

        }

        //----------------------------------------------------------------------

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Handles.Disc(Quaternion.identity, this.transform.position, Vector3.up, this.spawnRadius, false, 0);
        }
#endif
        //----------------------------------------------------------------------

    }

}