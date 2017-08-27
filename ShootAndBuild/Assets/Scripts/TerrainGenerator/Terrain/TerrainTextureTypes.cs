using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{

	// --------------------------------------------------------

	public enum TerrainTexturePairs
	{
		PlaneA,
		PlaneB,
		Rock,

		Count
	};

	// --------------------------------------------------------

	public class TerrainTextureTypes
	{
		public static int TexturePairToSplatIndex(TerrainTexturePairs pair, bool second)
		{
			return ((int) pair) * 2 + (second ? 1 : 0);
		}

		// --------------------------------------------------------

		public static float RedistributeBlendFactor(float blend12, float share2)
		{
			if (share2 == 0.0f) { return 0.0f; }
			if (share2 == 1.0f) { return 1.0f; }

			float amount1st = Mathf.InverseLerp(Mathf.Max(2.0f * share2 - 1.0f, 0.0f), Mathf.Min(2.0f * share2, 1.0f), blend12);

			return 1.0f - amount1st;
		}

		// --------------------------------------------------------

		public static float ApplySharpness(float oldBlendValue, float blendingSharpness)
		{
			if (oldBlendValue < 0.5f)
			{
				return Mathf.Pow(oldBlendValue * 2.0f, blendingSharpness) * 0.5f;
			}
			
			return 1.0f - (Mathf.Pow((1.0f - oldBlendValue) * 2.0f, blendingSharpness) * 0.5f);
		}
	}

	// -----------------------------------------------------------------
}