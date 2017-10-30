using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SAB
{
	public class DebugHelper : MonoBehaviour 
	{
		public static List<Vector3> BufferedTrianglesVertices	= new List<Vector3>();
		public static List<int>		BufferedTrianglesIndices	= new List<int>();
		public static List<Color>	BufferedTriangleColors		= new List<Color>();

		public static void BufferQuad(Vector3 quadMin, Vector3 quadMax, Color col)
		{
			float halfHeight = (quadMin.y + quadMax.y) * 0.5f;

			Vector3 p1 = new Vector3(quadMin.x, quadMin.y,	quadMin.z);
			Vector3 p2 = new Vector3(quadMax.x, halfHeight,	quadMin.z);
			Vector3 p3 = new Vector3(quadMax.x, quadMin.y,	quadMax.z);
			Vector3 p4 = new Vector3(quadMin.x, halfHeight,	quadMax.z);

			BufferQuad(p1, p2, p3, p4, col); 
		}

		public static void BufferQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color col)
		{
			BufferTriangle(p1, p2, p3, col);
			BufferTriangle(p1, p3, p4, col);
		}

		public static void BufferTriangle(Vector3 p1, Vector3 p2, Vector3 p3, Color col)
		{
			if (BufferedTrianglesVertices.Count > 100000)
			{
				Debug.Log("Using BufferTriangle() without Drawing Buffers?");
				return;
			}

			int firstIndex = BufferedTrianglesVertices.Count;
			BufferedTrianglesVertices.Add(p1);
			BufferedTrianglesVertices.Add(p2);
			BufferedTrianglesVertices.Add(p3);

			BufferedTriangleColors.Add(col);
			BufferedTriangleColors.Add(col);
			BufferedTriangleColors.Add(col); 

			BufferedTrianglesIndices.Add(firstIndex);
			BufferedTrianglesIndices.Add(firstIndex + 1);
			BufferedTrianglesIndices.Add(firstIndex + 2);
		}

		public static void DrawBufferedTriangles()
		{
			if (BufferedTrianglesVertices.Count == 0)
			{
				return;
			}

			Mesh tmpMesh = new Mesh();
			Vector3[] verticesArray = new Vector3[BufferedTrianglesVertices.Count];
			BufferedTrianglesVertices.CopyTo(verticesArray);
			tmpMesh.SetVertices(BufferedTrianglesVertices);
						
			int[] indicesArray = new int[BufferedTrianglesIndices.Count];
			BufferedTrianglesIndices.CopyTo(indicesArray);
			tmpMesh.SetTriangles(BufferedTrianglesIndices, 0);

			tmpMesh.SetColors(BufferedTriangleColors);
			tmpMesh.RecalculateNormals();

			Material mat = new Material(Shader.Find("Unlit/DebugPrimitive"));
			mat.SetPass(0);
			Graphics.DrawMeshNow(tmpMesh, Vector3.zero, Quaternion.identity);
			Object.DestroyImmediate(tmpMesh);

			BufferedTrianglesVertices.Clear();
			BufferedTrianglesIndices.Clear();
			BufferedTriangleColors.Clear();
		}

		// -------------------------------------------------------------------

		public static void DrawText(Vector2 pos2, float height, Color col, string text)
		{
			DrawText(new Vector3(pos2.x, height, pos2.y), col, text);
		}

		// -------------------------------------------------------------------

		public static void DrawText(Vector3 pos3, Color col, string text)
		{
			#if UNITY_EDITOR
				GUIStyle style = new GUIStyle();
				style.normal.textColor = col;
				UnityEditor.Handles.Label (pos3, text, style);
			#endif
		}
	}
}