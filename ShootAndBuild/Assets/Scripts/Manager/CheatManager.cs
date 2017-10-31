using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB
{
	///////////////////////////////////////////////////////////////////////////
	
	public class CheatManager : MonoBehaviour 
	{
		[Header("Invincible")]
		[SerializeField] private bool m_InvinciblePlayers	= false;
		[SerializeField] private bool m_InvincibleEnemies	= false;
		[SerializeField] private bool m_InvincibleBuildings	= false;

		[Header("Freeze")]
		[SerializeField] private bool m_FreezeEnemies		= false;
		[SerializeField] private bool m_FreezeTowers		= false;
		[SerializeField] private bool m_StopEnemySpawns		= false;

		[Header("Rules")]
		[SerializeField] private bool m_DisableWin			= false;
		[SerializeField] private bool m_DisableLose			= false;
        
		[SerializeField] private bool m_NoResourceCosts		= false;
		[SerializeField] private bool m_UltraHighDamage		= false;

		[Header("Disable Systems")]
		[SerializeField] private bool m_DisableAudio		= false;
		[SerializeField] private bool m_DisableMusic		= false;
        [SerializeField] private bool m_DisableVibration    = false;

		///////////////////////////////////////////////////////////////////////////
		
		public bool invinciblePlayers	{ get { return m_InvinciblePlayers; } }
		public bool invincibleEnemies	{ get { return m_InvincibleEnemies; } }
		public bool invincibleBuildings { get { return m_InvincibleBuildings; } }

		public bool freezeEnemies		{ get { return m_FreezeEnemies; } }
		public bool freezeTowers		{ get { return m_FreezeTowers; } }
		public bool stopEnemySpawns		{ get { return m_StopEnemySpawns; } set {m_StopEnemySpawns = value; } }

		public bool disableWin			{ get { return m_DisableWin; } }
		public bool disableLose			{ get { return m_DisableLose; } }

		public bool noResourceCosts		{ get { return m_NoResourceCosts; } }
		public bool ultraHighDamage		{ get { return m_UltraHighDamage; } }

		public bool disableAudio		{ get { return m_DisableAudio; } }
		public bool disableMusic		{ get { return m_DisableMusic; } }
		public bool disableVibration	{ get { return m_DisableVibration ; } }

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

