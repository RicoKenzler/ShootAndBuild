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
		[SerializeField] private bool m_PauseWaves			= false;

		[Header("Rules")]
		[SerializeField] private bool m_DisableWin			= false;
		[SerializeField] private bool m_DisableLose			= false;
        
		[SerializeField] private bool m_NoResourceCosts		= false;
		[SerializeField] private bool m_UltraHighDamage		= false;

		[Header("Disable Systems")]
		[SerializeField] private bool m_DisableAudio		= false;
		[SerializeField] private bool m_DisableMusic		= false;
        [SerializeField] private bool m_DisableVibration    = false;
		[SerializeField] private bool m_EnableDebugPanel    = false;

		///////////////////////////////////////////////////////////////////////////
		
		public bool invinciblePlayers	{ get { return m_InvinciblePlayers; } }
		public bool invincibleEnemies	{ get { return m_InvincibleEnemies; } }
		public bool invincibleBuildings { get { return m_InvincibleBuildings; } }

		public bool freezeEnemies		{ get { return m_FreezeEnemies; } }
		public bool freezeTowers		{ get { return m_FreezeTowers; } }
		public bool pauseWaves			{ get { return m_PauseWaves; } set {m_PauseWaves = value; } }

		public bool disableWin			{ get { return m_DisableWin; } }
		public bool disableLose			{ get { return m_DisableLose; } }

		public bool noResourceCosts		{ get { return m_NoResourceCosts; } }
		public bool ultraHighDamage		{ get { return m_UltraHighDamage; } }

		public bool disableAudio		{ get { return m_DisableAudio; } }
		public bool disableMusic		{ get { return m_DisableMusic; } }
		public bool disableVibration	{ get { return m_DisableVibration ; } }
		public bool enableDebugPanel	{ get { return m_EnableDebugPanel; }}

		public bool completeCurrentStage	{ get; set; }
		public bool completeCurrentWave		{ get; set; }

		public static CheatManager instance	{ get; private set; }

		///////////////////////////////////////////////////////////////////////////

		void Awake()
		{
			instance = this;
		}

		///////////////////////////////////////////////////////////////////////////
	}
}

