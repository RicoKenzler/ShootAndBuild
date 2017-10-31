using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////
	
	public class CheatManager : MonoBehaviour 
	{
		[Header("Invincible")]
		public bool invinciblePlayers		= false;
		public bool invincibleEnemies		= false;
		public bool invincibleBuildings		= false;

		[Header("Freeze")]
		public bool freezeEnemies			= false;
		public bool freezeTowers			= false;
		public bool stopEnemySpawns			= false;

		[Header("Rules")]
		public bool disableWin				= false;
		public bool disableLose				= false;
        
		public bool noResourceCosts			= false;
		public bool ultraHighDamage			= false;

		[Header("Disable Systems")]
		public bool disableAudio			= false;
		public bool disableMusic			= false;
        public bool disableVibration        = false;

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
		}
		
		///////////////////////////////////////////////////////////////////////////

		public static CheatManager instance
		{
			get; private set;
		}
	}
}

