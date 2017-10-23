using UnityEditor.SceneManagement;

namespace SAB
{
	public class SceneHelper
	{
		public static string sceneName
		{
			get
			{
				string sceneName = "";

				#if UNITY_EDITOR
					sceneName = EditorSceneManager.GetActiveScene().name;
					if (sceneName != "")
					{
						sceneName = sceneName.Substring(sceneName.LastIndexOf("/") + 1);
						sceneName = sceneName.Remove(sceneName.LastIndexOf("."));
					}
				#else
					sceneName = Application.loadedLevelName;
				#endif
				
				return sceneName;
			}
		}

		public static bool isSceneSaved
		{
			get
			{
				return sceneName != "";
			}
		}
	}
}