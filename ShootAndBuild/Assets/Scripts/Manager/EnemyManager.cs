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

		public string GetDebugInfo()
		{
			string outInfo = "Enemies: " + allEnemies.Count;

			if (allEnemies.Count > 0)
			{
				EnemyBehaviourBase rndEnemy = allEnemies[allEnemies.Count - 1];
				outInfo += "\n>> " + rndEnemy.type + " at " + rndEnemy.transform.position.GetDebugInfo();
			}

			return outInfo;
		}
    }
}