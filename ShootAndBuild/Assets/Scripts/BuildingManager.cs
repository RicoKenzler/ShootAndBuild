﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{

    public class BuildingManager : MonoBehaviour
    {
        void Awake()
        {
            instance = this;
            allBuildings = new List<Building>();
        }

        // Use this for initialization
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }

        public void RegisterBuilding(Building building, bool unregister)
        {
            if (unregister)
            {
                bool removed = allBuildings.Remove(building);
                Debug.Assert(removed);
            }
            else
            {
                allBuildings.Add(building);
            }
        }

        public static BuildingManager instance
        {
            get; private set;
        }

        public List<Building> allBuildings
        {
            get; private set;
        }
    }
}