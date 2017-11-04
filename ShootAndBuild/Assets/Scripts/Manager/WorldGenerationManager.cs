using SAB.Terrain;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

///////////////////////////////////////////////////////////

namespace SAB
{
	[System.Serializable]
	public struct GenerationParams
	{
		public bool	SkipGeneration;
		public bool	KeepOldSeed;
		public int	Seed;

		public void TryUpdateSeed(int seed)
		{
			if (SkipGeneration || KeepOldSeed)
			{
				return;
			}

			Seed = seed;
		}

		public void ApplyForceGeneration(WorldGenerationManager.ForceGeneration forceGeneration)
		{
			switch (forceGeneration)
			{
				case WorldGenerationManager.ForceGeneration.ForceGenerateDifferent:
					KeepOldSeed		= false;
					SkipGeneration	= false;
					break;

				case WorldGenerationManager.ForceGeneration.ForceRegenerateSame:
					KeepOldSeed		= true;
					SkipGeneration	= false;
					break;
			}
		}
	}

	public class WorldGenerationManager : MonoBehaviour
	{
		[SerializeField] private GenerationParams m_TerrainGeneration;
		[SerializeField] private GenerationParams m_PropGeneration;

		///////////////////////////////////////////////////////////

		public enum ForceGeneration
		{
			ForceRegenerateSame,
			ForceGenerateDifferent,
			NoForce,
		}

		public void RegenerateAll(ForceGeneration forceGeneration)
		{
			GenerationParams terrainParamsOld	= m_TerrainGeneration;
			GenerationParams propParamsOld		= m_PropGeneration;

			m_TerrainGeneration.ApplyForceGeneration(forceGeneration);
			m_PropGeneration.ApplyForceGeneration(forceGeneration);

			int timeSeed = (System.DateTime.Now.Millisecond + System.DateTime.Now.Second * 1000) % 100000;
			
			m_TerrainGeneration.TryUpdateSeed(timeSeed);
			m_PropGeneration.TryUpdateSeed(timeSeed);

			if (!m_TerrainGeneration.SkipGeneration)
			{
				GetComponent<TerrainGenerationManager>().RegenerateAll(m_TerrainGeneration.Seed);
			}

			if (!m_PropGeneration.SkipGeneration)
			{
				GetComponent<TerrainPropPlacement>().RegenerateAll(m_PropGeneration.Seed);
			}

			m_TerrainGeneration = terrainParamsOld;
			m_PropGeneration	= propParamsOld;
		}
	}
}


