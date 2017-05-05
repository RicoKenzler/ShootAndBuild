using System.Collections.Generic;
using UnityEngine;

public class BloodDecal : MonoBehaviour
{
	private static Queue<BloodDecal> decals = new Queue<BloodDecal>();
	private static int decalNum = 0;
	private const int maxDecals = 1000;
	private const float fadeTime = 1.0f;

	private float progress = 0.0f;
	private bool fadeOut = false;
	private bool fadeIn = false;


	void Start()
	{
		while (decals.Count > maxDecals)
		{
			BloodDecal decal = decals.Dequeue();
			decal.Vanish();
		}

		decals.Enqueue(this);

		fadeIn = true;

		transform.localScale = Vector3.zero;

		Renderer renderer = GetComponentInChildren<Renderer>();
		float x = Random.Range(1, 3) * 0.5f;
		float y = Random.Range(1, 3) * 0.5f;
		renderer.material.mainTextureOffset = new Vector2(x, y);
		
		Vector3 pos = transform.localPosition;
		pos.y = 0.05f + 0.000001f * decalNum;
		transform.localPosition = pos;

		transform.Rotate(Vector3.up, Random.Range(0.0f, 360.0f));

		decalNum = (decalNum + 1) % maxDecals;
	}

	void Update()
	{
		if (fadeIn)
		{
			progress += Time.deltaTime / fadeTime;
			progress = Mathf.Clamp01(progress);
			transform.localScale = Vector3.one * progress;

			if (progress == 1.0f)
			{
				fadeIn = false;
			}
		}

		if (fadeOut)
		{
			progress -= Time.deltaTime / fadeTime;
			progress = Mathf.Clamp01(progress);
			transform.localScale = Vector3.one * progress;

			if (progress == 0.0f)
			{
				Destroy(gameObject);
			}
		}
	}

	private void Vanish()
	{
		fadeIn = false;
		fadeOut = true;
	}
}
