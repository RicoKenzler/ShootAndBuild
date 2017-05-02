using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class EnemyManager : MonoBehaviour
    {
        void Awake()
        {
            instance = this;
            allEnemies = new List<EnemyBehaviour>();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RegisterEnemy(EnemyBehaviour behaviour, bool unregister)
        {
            if (unregister)
            {
                bool removed = allEnemies.Remove(behaviour);
                Debug.Assert(removed);
            }
            else
            {
                allEnemies.Add(behaviour);
            }
        }

        public static EnemyManager instance
        {
            get; private set;
        }

        public List<EnemyBehaviour> allEnemies
        {
            get; private set;
        }
    }
}