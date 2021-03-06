﻿using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

    public class AttackableManager : MonoBehaviour
    {
		
		///////////////////////////////////////////////////////////////////////////

        public static AttackableManager instance	{ get; private set; }
        public List<Attackable> allAttackables		{ get; private set; }
        
		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
            allAttackables = new List<Attackable>();
        }

		///////////////////////////////////////////////////////////////////////////

        public void RegisterAttackable(Attackable behaviour, bool unregister)
        {
            if (unregister)
            {
                bool removed = allAttackables.Remove(behaviour);
                Debug.Assert(removed);
            }
            else
            {
                allAttackables.Add(behaviour);
            }
        }
    }
}