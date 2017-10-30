using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
	[CustomEditor(typeof(MusicManager))]
	public class MusicManagerEditor : Editor
	{
		private bool positiveBuffTriggered = false;
		private bool negativeBuffTriggered = false;


		public override void OnInspectorGUI()
		{
			MusicManager musicManager = (MusicManager)target;

			DrawDefaultInspector();

			GUILayout.Label("Trigger", EditorStyles.boldLabel);
			if (GUILayout.Button((positiveBuffTriggered ? "(-)" : "(+)") + " Positive Buff"))
			{
				int delta = positiveBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, true);

				positiveBuffTriggered = !positiveBuffTriggered;
			}

			if (GUILayout.Button((negativeBuffTriggered ? "(-)" : "(+)") + " Negative Buff"))
			{
				int delta = negativeBuffTriggered ? -1 : 1;
				musicManager.OnAddPlayerBuffCount(delta, false);

				negativeBuffTriggered = !negativeBuffTriggered;
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
