using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using SAB.Terrain;

namespace SAB
{
	public class TerrainGenerationManager : MonoBehaviour
	{
		[SerializeField]
		private TerrainManager TerrainManager;

		[SerializeField]
		private AmbientSoundManager	AmbientSoundManager;

		public int Resolution = 129;

		public bool UseTimeAsSeed = true;

		public int TerrainSeed = 1000;
		public int VoronoiSeed = 1000;
		public int RegionSeed  = 1000;

		public TransformParameters			TransformParams;
		public RegionDescParameters			RegionDescParams;
		public WaterParameters				WaterParams;
		public VoronoiParameters			VoronoiParams;
		public RegionParameters				RegionParams;

		GameObject TerrainObject;
		public VoronoiCreator VoronoiGenerator			= new VoronoiCreator();
		public RegionMapGenerator RegionGenerator		= new RegionMapGenerator();
		public RegionGridGenerator RegionGridGenerator	= new RegionGridGenerator();
		public TerrainGenerator TerrainGenerator		= new TerrainGenerator();


		// -----------------------------------------------------------------

		public void DeleteTerrain()
		{
			if (!TerrainObject)
			{
				UnityEngine.Terrain childTerrain = GetComponentInChildren<UnityEngine.Terrain>();
				TerrainObject = childTerrain ? childTerrain.gameObject : null;
			}

			if (TerrainObject)
			{
				if (Application.isPlaying)
				{
					Destroy(TerrainObject.GetComponent<UnityEngine.Terrain>());
					Destroy(TerrainObject.GetComponent<UnityEngine.TerrainCollider>());
					Destroy(TerrainObject);
				}
				else
				{
					DestroyImmediate(TerrainObject.GetComponent<UnityEngine.Terrain>());
					DestroyImmediate(TerrainObject.GetComponent<UnityEngine.TerrainCollider>());
					DestroyImmediate(TerrainObject);
				}
			}

			TerrainManager.ReplaceTerrain(null, null, new Vector2(0.0f, 0.0f));
			AmbientSoundManager.GenerateAmbientGrid(null, null, new Vector2(0.0f, 0.0f));
		}
		
		// -----------------------------------------------------------------

		public void OnDrawGizmosSelected()
		{
			VoronoiGenerator.DebugDraw(VoronoiParams);
			RegionGenerator.DebugDraw();
			RegionGridGenerator.DebugDraw();
			TerrainGenerator.DebugDraw();
		}

		// -----------------------------------------------------------------

		public void RegenerateAll()
		{
			if (UseTimeAsSeed)
			{
				int timeSeed = (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000) % 100000;
				TerrainSeed = timeSeed;
				VoronoiSeed = timeSeed;
				RegionSeed  = timeSeed;
			}

			List<VoronoiCell> voronoiCells = VoronoiGenerator.GenerateVoronoi(VoronoiSeed, VoronoiParams, TransformParams.TerrainSizeWS);

			if (voronoiCells == null)
			{
				return;
			}

			RegionMapTransformation regionMapTransformation = new RegionMapTransformation(TransformParams.TerrainSizeWS, Resolution);
			RegionGenerator.GenerateRegions(RegionSeed, voronoiCells, RegionParams, regionMapTransformation);

			RegionGridGenerator.GenerateRegionGrid(RegionGenerator.RegionMap, regionMapTransformation, RegionParams);

			DeleteTerrain();

			TerrainObject = TerrainGenerator.GenerateTerrain(RegionGenerator.RegionMap, RegionGridGenerator.RegionGrid, regionMapTransformation, TransformParams, RegionGridGenerator.HeightRangeY, RegionDescParams, WaterParams, Resolution, TerrainSeed);

			if (TerrainObject)
			{
				TerrainObject.transform.parent = this.transform;
			}

			TerrainManager.ReplaceTerrain(TerrainObject.GetComponent<UnityEngine.Terrain>(), RegionGridGenerator.RegionGrid, TransformParams.TerrainSizeWS);
			AmbientSoundManager.GenerateAmbientGrid(RegionGridGenerator.RegionGrid, RegionGenerator.RegionMap, TransformParams.TerrainSizeWS);


		}
	}

	// -----------------------------------------------------------------

	[CustomEditor(typeof(TerrainGenerationManager))]
	public class TerrainGeneratorEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			TerrainGenerationManager terrainGenerator = (TerrainGenerationManager)target;

			DrawDefaultInspector();

			GUILayout.Label("Generate", EditorStyles.boldLabel);
			if (GUILayout.Button("Regenerate"))
			{
				terrainGenerator.RegenerateAll();
				EditorUtility.SetDirty(terrainGenerator);
			}
		}
	}
}