using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SAB
{

	public class MetaManager : MonoBehaviour
	{
		[SerializeField] private List<GameObject> m_TopLevelPrefabs;
		[SerializeField] private List<GameObject> m_ManagerPrefabs;

		const string MANAGER_FOLDER_NAME = "Manager";

		///////////////////////////////////////////////////////////////////////////

		// Note: We have to do this in AWAKE because PreplacedAttackable register to AttackManager in their OnEnabled
		void Awake()
		{
			// 1) Create Managers
			GameObject managerFolder = GameObject.Find(MANAGER_FOLDER_NAME);

			if (!managerFolder)
			{
				managerFolder = new GameObject();
				managerFolder.name = MANAGER_FOLDER_NAME;

				managerFolder.transform.SetSiblingIndex(0);
			}

			List<GameObject> reversedManagerList = m_ManagerPrefabs;
			reversedManagerList.Reverse();

			foreach (GameObject obj in reversedManagerList)
			{
				GameObject newManager = GameObject.Instantiate(obj);
				int deleteNameFrom = obj.name.IndexOf("Manager");

				if (deleteNameFrom == -1)
				{
					newManager.name = obj.name;
				}
				else
				{
					newManager.name = obj.name.Substring(0, deleteNameFrom);
				}

				newManager.transform.parent = managerFolder.transform;
			}

			// 2) Create TopLevel Stuff
			List<GameObject> reversedTopLevelPrefabs = m_TopLevelPrefabs;
			reversedTopLevelPrefabs.Reverse();

			foreach (GameObject obj in reversedTopLevelPrefabs)
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

}