using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MetaManager : MonoBehaviour
{
	[SerializeField] private List<GameObject> m_TopLevelPrefabs;
	[SerializeField] private List<GameObject> m_ManagerPrefabs;

	const string MANAGER_FOLDER_NAME = "Manager";

	///////////////////////////////////////////////////////////////////////////

	void Start()
	{
		// 1) Create Managers
		GameObject managerFolder = GameObject.Find(MANAGER_FOLDER_NAME);

		if (!managerFolder)
		{
			managerFolder = new GameObject();
			managerFolder.name = MANAGER_FOLDER_NAME;

			managerFolder.transform.SetSiblingIndex(0);
		}

		foreach (GameObject obj in m_ManagerPrefabs)
		{
			GameObject newManager = GameObject.Instantiate(obj);
			int deleteNameFrom = obj.name.IndexOf("Manager");

			newManager.name = obj.name.Substring(0, deleteNameFrom);

			newManager.transform.parent = managerFolder.transform;
		}

		// 2) Create TopLevel Stuff
		foreach (GameObject obj in m_TopLevelPrefabs)
		{
			GameObject newObj = GameObject.Instantiate(obj);
			newObj.name = obj.name;

			newObj.transform.SetSiblingIndex(0);
		}

		gameObject.transform.SetSiblingIndex(0);

		// do not need this manager anymore
		GameObject.Destroy(gameObject);
	}
	
}
