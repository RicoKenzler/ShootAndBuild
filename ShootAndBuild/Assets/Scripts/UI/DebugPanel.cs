using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SAB
{
	public class DebugPanel : MonoBehaviour 
	{
		[SerializeField] private Text m_DebugStatusText = null;

		private CanvasGroup m_CanvasGroup = null;

		///////////////////////////////////////////////////////////////////////////

		void Awake() 
		{
			m_CanvasGroup = GetComponent<CanvasGroup>();
		}
	
		///////////////////////////////////////////////////////////////////////////

		void Update() 
		{
			bool isEnabled = CheatManager.instance.enableDebugPanel;

			m_CanvasGroup.alpha = isEnabled ? 1.0f : 0.0f;

			if (isEnabled)
			{
				PollGameInfos();
			}
		}

		///////////////////////////////////////////////////////////////////////////

		void PollGameInfos()
		{
			string debugText = "";
			debugText += Spawn.SpawnManagerPrototype.instance.GetDebugInfo()	+ "\n";
			debugText += EnemyManager.instance.GetDebugInfo()					+ "\n";
			debugText += CameraController.instance.GetDebugInfo()				+ "\n";
			m_DebugStatusText.text = debugText;
		}
	}
}