using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	public class GeneratedTerrain : MonoBehaviour 
	{
		[SerializeField] private Vector2				m_SizeWS;

		[HideInInspector]
		[SerializeField] private AmbientSoundGrid		m_AmbientSoundGrid;

		[HideInInspector]
		[SerializeField] private UnityEngine.Terrain	m_Terrain;

		[HideInInspector]
		[SerializeField] private RegionGrid				m_RegionGrid;

		///////////////////////////////////////////////////////////////////////////	

		public const string GENERATED_TERRAIN_TAG = "GeneratedTerrain";

		///////////////////////////////////////////////////////////////////////////	

		public Vector2				sizeWS				{ get { return m_SizeWS; }				set { m_SizeWS = value; } }
		public AmbientSoundGrid		ambientSoundGrid	{ get { return m_AmbientSoundGrid; }	set { m_AmbientSoundGrid = value; } }
		public RegionGrid			regionGrid			{ get { return m_RegionGrid; }			set { m_RegionGrid = value; } }
		public UnityEngine.Terrain	terrain				{ get { return m_Terrain; }				set { m_Terrain = value; } }
		
		///////////////////////////////////////////////////////////////////////////

		public static GeneratedTerrain FindInScene()
		{
			GameObject foundObject = GameObject.FindWithTag(GENERATED_TERRAIN_TAG);

			if (!foundObject)
			{
				Debug.Assert(false, "Did not find Object with Tag " + GENERATED_TERRAIN_TAG);
				return null;
			}

			GeneratedTerrain generatedTerrain = foundObject.GetComponent<GeneratedTerrain>();

			if (!generatedTerrain)
			{
				Debug.Assert(false, "Object with Tag " + GENERATED_TERRAIN_TAG + " does not have Component GeneratedTerrain");
				return null;
			}

			return generatedTerrain;
		}

		///////////////////////////////////////////////////////////////////////////

		public float GetInterpolatedHeight(float xWS, float zWS)
		{
			if (m_Terrain == null)
			{
				Debug.Assert(m_Terrain != null, "You did not specify a terrain. Please Run Terrain Generator.");
				return 0.0f;
			}

			float height = m_Terrain.SampleHeight(new Vector3(xWS, 0.5f, zWS));
			height += m_Terrain.gameObject.transform.position.y;

			return height;
		}

		///////////////////////////////////////////////////////////////////////////
	}
}
