using UnityEngine;
using UnityEngine.Serialization;

namespace SAB.Spawn
{
    public class EnemySpawner : MonoBehaviour
    {
		[FormerlySerializedAs("spawnRadius")]
        [SerializeField] private float m_SpawnRadius = 3; 

        private SpawnPropabilityBlock	m_CurrentSpwanBlock = null;
        private float[]					m_SpawnTimer		= null;

        //??
		// profit!
        int totalSpawnCount = 0;

		///////////////////////////////////////////////////////////////////////////

		public SpawnPropabilityBlock currentSpawnBlock { get { return m_CurrentSpwanBlock; } }		

        ///////////////////////////////////////////////////////////////////////////

		public float spawnRadius { get { return m_SpawnRadius; } }

		///////////////////////////////////////////////////////////////////////////

        public string ID { get { return this.gameObject.name; } }

        ///////////////////////////////////////////////////////////////////////////

        void Update()
        {

            if (m_CurrentSpwanBlock == null)
            {
                return;
            }


            for (int i = 0; i < m_SpawnTimer.Length; ++i)
            {
                m_SpawnTimer[i] += Time.deltaTime * this.m_CurrentSpwanBlock.spawnRate[i];

                while (m_SpawnTimer[i] >= 1)
                {
                    m_SpawnTimer[i] -= 1;
                    if (CheatManager.instance.stopEnemySpawns)
                    {
                        return;
                    }

                    Spawn(this.m_CurrentSpwanBlock.enemies[i]);
                }
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public void ForceImmediateSpawn()
        {
            Spawn(EnemyType.Bat);
        }

        ///////////////////////////////////////////////////////////////////////////

        void Spawn(EnemyType _enemy)
        {
            GameObject enemyPrefab = SpawnManager.instance.GetEnemyTemplate(_enemy);

            if (enemyPrefab != null)
            {
                totalSpawnCount++;
                Vector3 spawnPosition = this.transform.position + Random.insideUnitSphere *m_SpawnRadius;
                spawnPosition.y = this.transform.position.y;
                GameObject enemyInstance = GameObject.Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
                enemyInstance.name = enemyPrefab.name + " " + totalSpawnCount;
            }
        }

        ///////////////////////////////////////////////////////////////////////////

        public void SetSpawnRate(SpawnPropabilityBlock _spawnData)
        {

            this.m_CurrentSpwanBlock = _spawnData;
            this.m_SpawnTimer = new float[this.m_CurrentSpwanBlock.spawnRate.Count];

        }

        ///////////////////////////////////////////////////////////////////////////

    }

}