using UnityEngine;

// ExtensionMethods-Helper to allow for syntax vector3.xz();
// TODO: Put in separate file

public static class VectorExtensions
{
	public static Vector2 xz(this Vector3 vec3D)
	{
		return new Vector2(vec3D.x, vec3D.z);
	}

	public static Vector3 To3D(this Vector2 vec2D, float y)
    {
		return new Vector3(vec2D.x, y, vec2D.y);
	}

	public static GameObject FindImmediateChildOfName(this Transform parent, string name)
	{
		for (int i = 0; i < parent.childCount; ++i)
		{
			if (parent.GetChild(i).name == name)
			{
				return parent.GetChild(i).gameObject;
			}
		}

		return null;
	}
}