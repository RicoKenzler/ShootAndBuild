using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
	[CustomEditor(typeof(MusicManager))]
	public class MusicManagerEditor : Editor
	{
		private bool m_PositiveBuffTriggered = false;
		private bool m_NegativeBuffTriggered = false;

		public override void OnInspectorGUI()
		{
			MusicManager musicManager = (MusicManager)target;

			DrawDefaultInspector();

			GUILayout.Label("Trigger", EditorStyles.boldLabel);
			if (GUILayout.Button((m_PositiveBuffTriggered ? "(-)" : "(+)") + " Positive Buff"))
			{
				int delta = m_PositiveBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, true);

				m_PositiveBuffTriggered = !m_PositiveBuffTriggered;
			}

			if (GUILayout.Button((m_NegativeBuffTriggered ? "(-)" : "(+)") + " Negative Buff"))
			{
				int delta = m_NegativeBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, false);

				m_NegativeBuffTriggered = !m_NegativeBuffTriggered;
			}

			if (GUILayout.Button("Signal Combat"))
			{
				musicManager.SignalIsInCombat();
			}

			if (GUILayout.Button("Signal Danger"))
			{
				musicManager.SignalIsDanger();
			}

			if (GUILayout.Button("Next Track"))
			{
				musicManager.NextTrack();
			}

		}
	}
}
