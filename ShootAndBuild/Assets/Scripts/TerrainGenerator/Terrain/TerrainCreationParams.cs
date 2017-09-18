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

		public float MaxWaterDistanceAscension	=  1.0f;
		public float UnderwaterTerrainHeight	= -1.0f;

		[Header("Debug")]
		public bool ShowRegions			= false;
		public bool ShowIndices			= false;
		public bool ShowWaterDistance	= false;
		public bool ShowRegionGrid		= false;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class WaterParameters
	{
		public GameObject	WaterPlanePrefab;
		public bool			UseWater		= true;
		public float		WaterHeight		= 0.0f;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class TransformParameters
	{
		public Vector2 TerrainCenter = new Vector2(0.0f, 0.0f);
		public Vector2 TerrainSizeWS = new Vector2(64.0f, 64.0f);
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class RegionDesc
	{
		public RegionHeightDesc		HeightDesc;
		public RegionTextureDesc	TextureDesc;
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class RegionHeightDesc
	{
		[Range(0.005f, 0.1f)]
		public float PerlinFrequencyCoarse	= 0.015f;
		[Range(0.005f, 0.5f)]
		public float PerlinFrequencyFine	= 0.10f;
		[Range(0.0f, 1.0f)]
		public float PerlinWeightFine		= 0.15f;

		public float MaxAdditionalRandomAbsHeight	= 0.1f;

		public float GenerateRandomAdditionalHeightAbs(float seed, int x, int z)
		{
			float perlinCoarse = Mathf.PerlinNoise(seed + x * PerlinFrequencyCoarse,	seed + z * PerlinFrequencyCoarse);
			float perlinFine   = Mathf.PerlinNoise(seed + x * PerlinFrequencyFine,		seed + z * PerlinFrequencyFine);
			
			float noiseTotal = Mathf.Lerp(perlinCoarse, perlinFine, PerlinWeightFine);

			noiseTotal *= MaxAdditionalRandomAbsHeight;

			return noiseTotal;
		}
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class RegionTextureDesc
	{
		public Texture2D Albedo1;
		public Texture2D Albedo2;

		public Texture2D Normal1;
		public Texture2D Normal2;

		[Range(0.5f, 20.0f)]
		public float Tiling1 = 4.0f;
		
		[Range(0.5f, 20.0f)]
		public float Tiling2 = 3.0f;

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
			seed += 20.0f;

			float perlinCoarse = Mathf.PerlinNoise(seed + x * PerlinFrequency12, seed + z * PerlinFrequency12);
			
			float amount2nd = TerrainTextureTypes.RedistributeBlendFactor(perlinCoarse, Share2ndTexture);

			return TerrainTextureTypes.ApplySharpness(amount2nd, BlendingSharpness12);
		}
	}

	// --------------------------------------------------------

	[System.Serializable]
	public class RegionDescParameters
	{
		public RegionDesc[] RegionDescs = new RegionDesc[(int) RegionType.Count];
	}

	// --------------------------------------------------------

}

