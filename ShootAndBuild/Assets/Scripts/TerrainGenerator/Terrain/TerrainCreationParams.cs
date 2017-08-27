using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SAB.Terrain
{
	// --------------------------------------------------------

	[System.Serializable]
	public class VoronoiParameters
	{
		[Header("Generation")]
		[Range(3, 1000)]
		public int VoronoiPointCount = 600;

		[Range(0.0f, 4.0f)]
		public float RelaxationAmount = 4.0f;

		[Header("Debug")]
		public bool ShowDelauney		= false;
		public bool ShowVoronoi			= false;
		public bool ShowPoints			= false;
		public bool ShowIndices			= false;
		public bool ShowBorder			= false;
		public bool ShowVorArea			= false;
		public bool ShowVorOrigentation = false;
		public bool NoRecomputation		= false;

		[Range(-1, 50)]
		public int DebugDrawOnlyVertexIndex		= -1;

		[Range(-1, 50)]
		public int DebugDrawOnlyTriangleIndex	= -1;
		
		public bool SuppressClamping		= false;
		public bool SuppressNewBorderEdges	= false;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class RegionParameters
	{
		[Header("Generation")]
		[Range(0, 20)]
		public int WaterCircles			= 4;

		[Range(0, 0.4f)]
		public float WaterCircleSize	= 0.2f;

		[Range(0, 1.0f)]
		public float BeachSize			= 0.14f;

		[Header("Debug")]
		public bool ShowRegions			= false;
		public bool ShowIndices			= false;
		public bool ShowWaterDistance	= false;
		public bool ShowRegionGrid		= false;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class TransformParameters
	{
		public Vector2 TerrainCenter = new Vector2(0.0f, 0.0f);
		public Vector2 TerrainSizeWS = new Vector2(64.0f, 64.0f);

		[Range(-5.0f, 20.0f)]
		public float HeightMax =  6.0f;

		[Range(-10.0f, 5.0f)]
		public float HeightMin =  0.0f;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class HeightGenerationParameters
	{
		[Range(0.005f, 0.1f)]
		public float PerlinFrequencyCoarse	= 0.015f;
		[Range(0.005f, 0.5f)]
		public float PerlinFrequencyFine	= 0.10f;

		[Range(0.0f, 1.0f)]
		public float PerlinWeightCoarse		= 0.50f;

		[Range(0.0f, 1.0f)]
		public float PerlinWeightFine		= 0.15f;
		public bool DebugHeightRange		= false;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class TexturePair
	{
		public Texture2D Albedo1;
		public Texture2D Albedo2;

		public Texture2D Normal1;
		public Texture2D Normal2;

		[Range(0.5f, 20.0f)]
		public float Tiling1 = 4.0f;
		
		[Range(0.5f, 20.0f)]
		public float Tiling2 = 4.0f;

		[Range(1.0f, 20.0f)]
		public float BlendingSharpness12 = 7.0f;

		[Range(0.005f, 1.0f)]
		public float PerlinFrequency12	= 0.015f;

		[Range(0.000f, 1.0f)]
		public float Share2ndTexture = 0.4f;

		public SplatPrototype CreateSplatPrototype(bool second)
		{
			SplatPrototype prototype = new SplatPrototype();

			prototype.texture	= second ? Albedo2 : Albedo1;
			prototype.normalMap = second ? Normal2 : Normal1;

			float tiling = second ? Tiling2 : Tiling1;
			prototype.tileSize	= new Vector2(tiling, tiling);

			return prototype;
		}

		public float GetBlendValue12(float seed, float x, float z)
		{
			float perlinCoarse = Mathf.PerlinNoise(seed + x * PerlinFrequency12, seed + z * PerlinFrequency12);
			
			float amount2nd = TerrainTextureTypes.RedistributeBlendFactor(perlinCoarse, Share2ndTexture);

			return TerrainTextureTypes.ApplySharpness(amount2nd, BlendingSharpness12);
		}
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class TextureParameters
	{
		public TexturePair TexturePlaneA;
		public TexturePair TexturePlaneB;
		public TexturePair TextureRock;
		
		[Range(1.0f, 20.0f)]
		public float RockBlendingSharpness	= 7.0f;

		[Range(0.0f, 1.0f)]
		public float PlaneBShare = 0.5f;

		[Range(0.0f, 90.0f)]
		public float RockSteepnessAngle	= 55.0f;
	}

	// --------------------------------------------------------

}

