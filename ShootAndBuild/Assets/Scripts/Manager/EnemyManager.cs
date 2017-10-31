using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
    public class EnemyManager : MonoBehaviour
    {
		public static EnemyManager instance			{ get; private set; }
        public List<EnemyBehaviourBase> allEnemies	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
            allEnemies = new List<EnemyBehaviourBase>();
        }

		///////////////////////////////////////////////////////////////////////////

        public void RegisterEnemy(EnemyBehaviourBase behaviour, bool unregister)
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

		///////////////////////////////////////////////////////////////////////////
    }
}