using UnityEngine;

public class Builder : MonoBehaviour
{
	public GameObject towerPrefab;
	public float distance = 2;


	public void Build()
	{
		Vector3 pos = transform.position + transform.rotation * (distance * Vector3.forward);

		if (Grid.instance.IsFree(towerPrefab, pos))
		{
			GameObject instance = Instantiate(towerPrefab);
			instance.transform.position = pos;
		}
	}
}