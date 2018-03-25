#if UNITY_EDITOR

public class AkSceneUtils
{
	private static UnityEngine.SceneManagement.Scene m_currentScene;

	public static void CreateNewScene()
	{
		m_currentScene =
			UnityEditor.SceneManagement.EditorSceneManager.NewScene(UnityEditor.SceneManagement.NewSceneSetup
				.DefaultGameObjects);
	}

	public static void OpenExistingScene(string scene)
	{
		if (string.IsNullOrEmpty(scene))
			return;

		m_currentScene = UnityEditor.SceneManagement.EditorSceneManager.OpenScene(scene);
	}

	public static string GetCurrentScene()
	{
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		return scene.path;
	}

	public static void SaveCurrentScene(string scene)
	{
		bool result;

		if (scene == null)
			result = !UnityEditor.SceneManagement.EditorSceneManager.SaveScene(m_currentScene);
		else
			result = !UnityEditor.SceneManagement.EditorSceneManager.SaveScene(m_currentScene, scene);

		if (result)
			throw new System.Exception("Error occurred while saving migrated scenes.");
	}
}

#endif