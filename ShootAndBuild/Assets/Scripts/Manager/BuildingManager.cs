﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////

    public class BuildingManager : MonoBehaviour
    {

		///////////////////////////////////////////////////////////////////////////

		public static BuildingManager instance	{ get; private set; }
        public List<Building> allBuildings		{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

        void Awake()
        {
            instance = this;
            allBuildings = new List<Building>();
        }

		///////////////////////////////////////////////////////////////////////////

        public void RegisterBuilding(Building building, bool unregister)
        {
            if (unregister)
            {
                allBuildings.Remove(building);
            }
            else
            {
                allBuildings.Add(building);
            }
        }
    }
}