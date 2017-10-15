using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace SAB.Terrain
{
	
	[System.Serializable]
	public class Prop
	{
		public GameObject	Prefab;

		[Range(0.0f, 3.0f)]
		public float		MinSize				= 1.0f;

		[Range(0.0f, 3.0f)]
		public float		MaxSize				= 0.2f;

		[Range(0.01f, 1.0f)]
		public float		FlattenFactor		= 1.0f;

		[Range(0.0f, 1000.0f)]
		public float		RelativeProbability	= 100;
	}

	[System.Serializable]
	public class PropGroup
	{
		public Prop[] Props;
		public RegionType[] Regions;

		[Range(0.0f, 1.0f)]
		public float Density		= 1.0f;

		[Range(0.0f, 1.0f)]
		public float MaskDensity	= 1.0f;

		[Range(0.01f, 0.5f)]
		public float MaskFrequency  = 1.0f;

		[Range(0.01f, 0.5f)]
		public float RndPropFrequency = 1.0f;
	}

	public class TerrainPropPlacement : MonoBehaviour 
	{
		[SerializeField]
		private TerrainManager			terrainManager;
		private GameObject				containerObject;
		private float					seed;
		private RegionMapTransformation	regionMapTransformation;

		public PropGroup[]	propGroups;

		const string PropGroupName = "Props";

		void Awake()
		{
			Instance = this;
		}

		void Start() 
		{
			
		}
		
		void Update() 
		{
			
		}

		public void RecreateEmptyContainerObject()
		{
			containerObject = null;

			GameObject terrainObject = terrainManager.Terrain.gameObject;
			
			Transform oldGroupObjectTransform = terrainObject.transform.Find(PropGroupName);
			GameObject groupObject = oldGroupObjectTransform == null ? null : oldGroupObjectTransform.gameObject;
			if (groupObject)
			{
				GameObject.DestroyImmediate(groupObject);
			}

			groupObject = new GameObject(PropGroupName);
			groupObject.transform.parent = terrainObject.transform;

			containerObject = groupObject;
		}

		public void RegenerateAll(int _seed)
		{
			seed = 0.174f * _seed;

			Random.InitState(_seed);

			Debug.Assert(terrainManager);

			if (!terrainManager.Terrain)
			{
				Debug.LogWarning("No Terrain found");
				return;
			}

			if (terrainManager.RegionGrid == null)
			{
				Debug.LogWarning("Currently RegionGrid is not serialized, so you first have to generate a terrain");
				return;
			}
			
			regionMapTransformation = terrainManager.RegionMapTransformation;

			RecreateEmptyContainerObject();
			CreateProps();
		}

		void CreateProps()
		{
			foreach (PropGroup propGroup in propGroups)
			{
				if (propGroup.Regions.Length == 0)
				{
					continue;
				}

				GameObject subContainerObject = new GameObject();
				subContainerObject.transform.parent = containerObject.transform;
				
				subContainerObject.name = "";

				foreach (RegionType regionType in propGroup.Regions)
				{
					subContainerObject.name += regionType.ToString();
				}

				GenerateProps(propGroup, subContainerObject.transform);
			}
		}

		void GenerateProps(PropGroup propGroup, Transform subContainerObject)
		{
			float probabilitySum = 0.0f;

			foreach (Prop prop in propGroup.Props)
			{
				probabilitySum += prop.RelativeProbability;
			}

			if (probabilitySum == 0.0f)
			{
				return;
			}

			RegionTile[,] regionGrid = terrainManager.RegionGrid;

			for (int x = 0; x < regionGrid.GetLength(0); ++x)
			{
				for (int z = 0; z < regionGrid.GetLength(1); ++z)
				{
					float maskRnd = Mathf.PerlinNoise(seed + x * propGroup.MaskFrequency,	seed + z * propGroup.MaskFrequency);
					if (maskRnd > propGroup.MaskDensity)
					{
						continue;
					}

					float rnd = Random.Range(0.0f, 1.0f);
					if (rnd > propGroup.Density)
					{
						continue;
					}

					RegionType regionType = regionGrid[x,z].MainRegion;
					bool isOfDesiredRegion = false;

					foreach (RegionType containedType in propGroup.Regions)
					{
						if (regionType == containedType)
						{
							isOfDesiredRegion = true;
							break;
						}
					}

					if (!isOfDesiredRegion)
					{
						continue;
					}

					PlaceProp(propGroup, subContainerObject, x, z, probabilitySum, seed + 0.123f);
				}
			}
		}

		public void PlaceProp(PropGroup propGroup, Transform subContainerObject, int x, int z, float probabilitySum, float seed)
		{
			Vector2 tileMin = regionMapTransformation.GetTileMin(x, z);
			Vector2 tileMax = tileMin + regionMapTransformation.CellSize;
			
			Vector3 rndPos;
			rndPos.x = Mathf.Lerp(tileMin.x, tileMax.x, Random.Range(0.0f, 1.0f));
			rndPos.y = 0.0f;
			rndPos.z = Mathf.Lerp(tileMin.y, tileMax.y, Random.Range(0.0f, 1.0f));

			rndPos.y = terrainManager.GetInterpolatedHeight(rndPos.x, rndPos.z);

			// rnd in [0,1], but distribution is not uniform :(
			float rnd = Mathf.PerlinNoise(seed + x * propGroup.RndPropFrequency, seed + z * propGroup.RndPropFrequency);

			// bring to [0.25, 0.75]
			if (rnd < 0.25f)
			{
				// 0.25 -> 0.25, 0.0 -> 0.5
				rnd = 0.5f - rnd;
			}
			else if (rnd > 0.75)
			{
				// 0.75 -> 0.75, 1.0 -> 0.5
				rnd = 1.5f - rnd;
			}

			// bring back to [0,1]
			rnd = (rnd - 0.25f) * 2.0f;

			// bring to [0, p]
			rnd *= probabilitySum;

			float curSum = 0.0f;

			foreach (Prop prop in propGroup.Props)
			{
				curSum += prop.RelativeProbability;

				if (rnd > curSum)
				{
					continue;
				}

				GameObject newObject = GameObject.Instantiate(prop.Prefab, subContainerObject.transform);
				newObject.transform.position = rndPos;

				float rndRotY = Random.Range(0.0f, 360.0f);
				float rndScaleX = Random.Range(prop.MinSize, prop.MaxSize);
				float rndScaleY = Random.Range(prop.MinSize, prop.MaxSize) * prop.FlattenFactor;
				float rndScaleZ = Random.Range(prop.MinSize, prop.MaxSize);
				newObject.transform.rotation = Quaternion.AngleAxis(rndRotY, Vector3.up);
				newObject.transform.localScale = new Vector3(rndScaleX, rndScaleY, rndScaleZ);
				return;
			}
		}

		public static TerrainPropPlacement Instance
		{
			get; private set;
		}
	}

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