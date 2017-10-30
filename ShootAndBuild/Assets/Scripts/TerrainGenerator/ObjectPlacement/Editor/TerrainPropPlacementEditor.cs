using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB.Terrain
{
	[CustomEditor(typeof(TerrainPropPlacement))]
	public class TerrainPropPlacementEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainPropPlacement propPlacement = (TerrainPropPlacement)target;

			DrawDefaultInspector();

			GUILayout.Label("Generate", EditorStyles.boldLabel);
			if (GUILayout.Button("Regenerate"))
			{
				propPlacement.RegenerateAll((int)Time.time * 10);
				EditorUtility.SetDirty(propPlacement);
			}

			if (GUILayout.Button("RegenerateFixedSeed"))
			{
				propPlacement.RegenerateAll(0);
				EditorUtility.SetDirty(propPlacement);
			}
		}
	}
}
