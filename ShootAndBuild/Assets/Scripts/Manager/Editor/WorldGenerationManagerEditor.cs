using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB
{
	[CustomEditor(typeof(WorldGenerationManager))]
	public class WorldGenerationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			WorldGenerationManager worldGenerator = (WorldGenerationManager)target;

			DrawDefaultInspector();

			if (GUILayout.Button("Generate"))
			{
				worldGenerator.RegenerateAll(WorldGenerationManager.ForceGeneration.NoForce);
				EditorUtility.SetDirty(worldGenerator);
				EditorUtility.SetDirty(FindObjectOfType<Grid>());
			}

			if (GUILayout.Button("Generate (Force Same)"))
			{
				worldGenerator.RegenerateAll(WorldGenerationManager.ForceGeneration.ForceRegenerateSame);
				EditorUtility.SetDirty(worldGenerator);
				EditorUtility.SetDirty(FindObjectOfType<Grid>());
			}

			if (GUILayout.Button("Generate (Force different)"))
			{
				worldGenerator.RegenerateAll(WorldGenerationManager.ForceGeneration.ForceGenerateDifferent);
				EditorUtility.SetDirty(worldGenerator);
				EditorUtility.SetDirty(FindObjectOfType<Grid>());
			}
		}
	}
}
